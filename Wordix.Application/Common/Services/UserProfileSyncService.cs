using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Services;

/// <summary>
/// Keycloak token'ındaki kullanıcı ile Wordix database'indeki UserProfile kaydını
/// senkronize eden application servisidir.
/// 
/// Bu servis ne yapar?
/// - Current user bilgisini ICurrentUserService üzerinden okur.
/// - Kullanıcı authenticated değilse hata fırlatır.
/// - UserProfiles tablosunda KeycloakUserId ile kayıt arar.
/// - Varsa mevcut UserProfile kaydını döndürür.
/// - Yoksa yeni UserProfile ve UserPreference oluşturur.
/// - UnitOfWork ile değişiklikleri database'e kaydeder.
/// 
/// Neden Application katmanında?
/// - Bu iş bir use-case/application workflow'dur.
/// - HttpContext veya Keycloak detaylarını bilmez.
/// - EF Core DbContext bilmez.
/// - Sadece Application interface'leri üzerinden çalışır.
/// </summary>
public sealed class UserProfileSyncService : IUserProfileSyncService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserProfile> _userProfileRepository;
    private readonly IRepository<UserPreference> _userPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Gerekli bağımlılıklar constructor injection ile alınır.
    /// 
    /// ICurrentUserService:
    /// Token'dan okunmuş kullanıcı bilgisini sağlar.
    /// 
    /// IRepository<UserProfile>:
    /// UserProfiles tablosunda arama ve ekleme yapmak için kullanılır.
    /// 
    /// IRepository<UserPreference>:
    /// Yeni kullanıcı oluştuğunda varsayılan tercihleri oluşturmak için kullanılır.
    /// 
    /// IUnitOfWork:
    /// Repository işlemlerini tek SaveChanges ile database'e yazmak için kullanılır.
    /// </summary>
    public UserProfileSyncService(
        ICurrentUserService currentUserService,
        IRepository<UserProfile> userProfileRepository,
        IRepository<UserPreference> userPreferenceRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userProfileRepository = userProfileRepository;
        _userPreferenceRepository = userPreferenceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<UserProfile> GetOrCreateCurrentUserProfileAsync(
        CancellationToken cancellationToken = default)
    {
        // 1. Token'dan gelen current user bilgisini alıyoruz.
        // Burada HttpContext veya claim okuma yapmıyoruz.
        // O detaylar KeycloakCurrentUserService içinde kaldı.
        var currentUser = _currentUserService.GetCurrentUser();

        // 2. Kullanıcı authenticated değilse bu endpoint/use-case devam edemez.
        // Normalde [Authorize] olan endpointlerde buraya düşmemesi gerekir.
        // Ama servis kendi güvenliğini de sağlamalıdır.
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User must be authenticated.");
        }

        // 3. KeycloakUserId bizim için zorunlu bilgidir.
        // Çünkü UserProfile ile Keycloak kullanıcısını bu alan üzerinden eşleştiriyoruz.
        if (string.IsNullOrWhiteSpace(currentUser.KeycloakUserId))
        {
            throw new UnauthorizedAccessException("Keycloak user id could not be read from token.");
        }

        // 4. Email de UserProfile için zorunlu kabul edilir.
        // Keycloak client scope içinde email claiminin geldiğinden emin olmalıyız.
        if (string.IsNullOrWhiteSpace(currentUser.Email))
        {
            throw new UnauthorizedAccessException("User email could not be read from token.");
        }

        // 5. Önce bu KeycloakUserId ile Wordix database'inde UserProfile var mı bakıyoruz.
        // FirstOrDefaultAsync generic repository üzerinden gelir.
        // Application katmanı DbContext bilmez.
        var existingProfile = await _userProfileRepository.FirstOrDefaultAsync(
            profile => profile.KeycloakUserId == currentUser.KeycloakUserId,
            cancellationToken);

        // 6. Kayıt varsa tekrar oluşturmayız.
        // Böylece her /api/profile/me çağrısında yeni kullanıcı oluşmaz.
        if (existingProfile is not null)
        {
            return existingProfile;
        }

        // 7. Kayıt yoksa token bilgileriyle yeni Wordix UserProfile oluşturuyoruz.
        var accountType = ResolveAccountType(currentUser.Roles);

        var displayName = ResolveDisplayName(
            username: currentUser.Username,
            email: currentUser.Email);

        var username = ResolveUsername(
            username: currentUser.Username,
            email: currentUser.Email);

        var newProfile = new UserProfile(
            keycloakUserId: currentUser.KeycloakUserId,
            email: currentUser.Email,
            username: username,
            displayName: displayName,
            accountType: accountType,
            nativeLanguageId: null,
            targetLanguageId: null);

        // 8. Yeni kullanıcı için varsayılan preference oluşturuyoruz.
        // Böylece ileride quiz/default ayarlar için ayrıca null kontrolü yapmak zorunda kalmayız.
        var userPreference = new UserPreference(
            userProfileId: newProfile.Id);

        // 9. Repository'lere ekliyoruz.
        // Burada henüz database'e yazılmaz.
        // Sadece DbContext change tracker'a eklenir.
        await _userProfileRepository.AddAsync(newProfile, cancellationToken);
        await _userPreferenceRepository.AddAsync(userPreference, cancellationToken);

        // 10. Tek SaveChanges ile hem UserProfile hem UserPreference birlikte kaydedilir.
        // Bu Unit of Work yaklaşımının temel faydasıdır.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newProfile;
    }

    /// <summary>
    /// Token rollerine göre Wordix AccountType değerini belirler.
    /// 
    /// İlk prototipte admin ve basic user rollerimiz var.
    /// İleride teacher/student rolleri geldiğinde burası genişletilebilir.
    /// </summary>
    private static AccountType ResolveAccountType(IReadOnlyCollection<string> roles)
    {
        if (roles.Any(role => string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)))
        {
            return AccountType.Admin;
        }

        if (roles.Any(role => string.Equals(role, "teacher", StringComparison.OrdinalIgnoreCase)))
        {
            return AccountType.Teacher;
        }

        if (roles.Any(role => string.Equals(role, "student", StringComparison.OrdinalIgnoreCase)))
        {
            return AccountType.Student;
        }

        return AccountType.BasicUser;
    }

    /// <summary>
    /// Username bilgisini güvenli şekilde belirler.
    /// 
    /// Öncelik:
    /// 1. Token'daki username
    /// 2. Email
    /// 
    /// Çünkü UserProfile.Username zorunlu alan olarak tasarlandı.
    /// </summary>
    private static string ResolveUsername(string? username, string email)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            return username.Trim();
        }

        return email.Trim();
    }

    /// <summary>
    /// DisplayName bilgisini güvenli şekilde belirler.
    /// 
    /// DisplayName opsiyonel olsa da ilk oluşturma sırasında kullanıcıya daha anlamlı
    /// bir başlangıç değeri vermek için username/email üzerinden dolduruyoruz.
    /// </summary>
    private static string ResolveDisplayName(string? username, string email)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            return username.Trim();
        }

        return email.Trim();
    }
}