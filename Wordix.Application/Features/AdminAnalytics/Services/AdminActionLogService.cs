using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.AdminAnalytics.Services;

/// <summary>
/// Admin işlemlerini AdminActionLogs tablosuna kaydeden application servisidir.
/// 
/// Bu servis ne yapar?
/// - Current admin kullanıcının KeycloakUserId değerini ICurrentUserService üzerinden alır.
/// - AdminActionLog domain entity'sini oluşturur.
/// - Generic repository ile kaydı ekler.
/// - UnitOfWork ile database'e kaydeder.
/// 
/// Bu servis ne yapmaz?
/// - HttpContext okumaz.
/// - JWT claim parse etmez.
/// - DbContext kullanmaz.
/// - Controller veya endpoint bilgisi bilmez.
/// 
/// Mimari not:
/// Application katmanı Keycloak detayını bilmez.
/// Sadece ICurrentUserService abstraction'ından gelen current user bilgisini kullanır.
/// </summary>
public sealed class AdminActionLogService : IAdminActionLogService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<AdminActionLog> _adminActionLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminActionLogService(
        ICurrentUserService currentUserService,
        IRepository<AdminActionLog> adminActionLogRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));

        _adminActionLogRepository = adminActionLogRepository
            ?? throw new ArgumentNullException(nameof(adminActionLogRepository));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Geçerli admin kullanıcının yaptığı işlemi audit log olarak kaydeder.
    /// 
    /// Neden GetRequiredKeycloakUserId kullanıyoruz?
    /// - Bu servis admin endpointlerinden çağrılacak.
    /// - Admin endpointleri zaten [Authorize(Policy = "AdminOnly")] ile korunacak.
    /// - Buna rağmen token yoksa veya KeycloakUserId okunamazsa devam etmek güvenli değildir.
    /// 
    /// Neden burada role kontrolü yapmıyoruz?
    /// - Role kontrolü API controller üzerinde AdminOnly policy ile yapılacak.
    /// - Bu servis sadece audit log yazma sorumluluğuna sahip.
    /// - Böylece authorization ve audit sorumluluklarını karıştırmıyoruz.
    /// </summary>
    public async Task LogCurrentAdminActionAsync(
        string actionType,
        string description,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var adminKeycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        var adminActionLog = new AdminActionLog(
            adminKeycloakUserId: adminKeycloakUserId,
            actionType: actionType,
            description: description,
            relatedEntityType: relatedEntityType,
            relatedEntityId: relatedEntityId);

        await _adminActionLogRepository.AddAsync(
            adminActionLog,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}