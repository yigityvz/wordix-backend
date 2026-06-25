using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Profile.Responses;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Profile.Queries.GetCurrentUserProfile;

/// <summary>
/// GetCurrentUserProfileQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - IUserProfileSyncService üzerinden current user's UserProfile kaydını getirir veya oluşturur.
/// - Domain entity olan UserProfile'ı CurrentUserProfileResponse DTO'suna manual map eder.
/// - Controller'a response DTO döndürür.
/// 
/// Neden handler var?
/// - Controller'ın iş mantığını bilmemesi için.
/// - Profile okuma/sync use-case'ini Application katmanında tutmak için.
/// - İleride validation/logging pipeline behavior'larının bu akışa otomatik dahil olabilmesi için.
/// </summary>
public sealed class GetCurrentUserProfileQueryHandler
    : IRequestHandler<GetCurrentUserProfileQuery, CurrentUserProfileResponse>
{
    private readonly IUserProfileSyncService _userProfileSyncService;

    /// <summary>
    /// Handler ihtiyacı olan application servisini constructor injection ile alır.
    /// 
    /// IUserProfileSyncService:
    /// Token'daki kullanıcıyı Wordix UserProfile kaydıyla eşleştirir.
    /// </summary>
    public GetCurrentUserProfileQueryHandler(
        IUserProfileSyncService userProfileSyncService)
    {
        _userProfileSyncService = userProfileSyncService;
    }

    /// <summary>
    /// Query çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<CurrentUserProfileResponse> Handle(
        GetCurrentUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Current user'ın UserProfile kaydını getirir.
        // Eğer daha önce oluşturulmadıysa UserProfile + UserPreference oluşturur.
        var userProfile = await _userProfileSyncService
            .GetOrCreateCurrentUserProfileAsync(cancellationToken);

        // Domain entity'yi dış API response DTO'suna çeviriyoruz.
        // AutoMapper/Mapster kullanmıyoruz; proje kararımız manual mapping.
        return MapToResponse(userProfile);
    }

    /// <summary>
    /// UserProfile domain entity'sini CurrentUserProfileResponse DTO'suna çevirir.
    /// 
    /// Neden burada?
    /// - Bu response'u üreten use-case bu handler'dır.
    /// - Controller mapping bilmesin.
    /// - Domain entity dışarı direkt açılmasın.
    /// </summary>
    private static CurrentUserProfileResponse MapToResponse(UserProfile userProfile)
    {
        return new CurrentUserProfileResponse
        {
            UserProfileId = userProfile.Id,
            Email = userProfile.Email,
            Username = userProfile.Username,
            DisplayName = userProfile.DisplayName,
            AccountType = userProfile.AccountType.ToString(),
            NativeLanguageId = userProfile.NativeLanguageId,
            TargetLanguageId = userProfile.TargetLanguageId,
            IsActive = userProfile.IsActive
        };
    }
}