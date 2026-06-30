using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Profile.Responses;

namespace Wordix.Application.Features.Profile.Queries.GetCurrentUserInfo;

/// <summary>
/// GetCurrentUserInfoQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - ICurrentUserService üzerinden current user's token bilgilerini okur.
/// - KeycloakUserId, email, username ve rollerden response DTO üretir.
/// - Database'e gitmez.
/// - UserProfile oluşturmaz.
/// - UserPreference oluşturmaz.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - User-owned kayıtlar token içindeki "sub" claiminden gelen KeycloakUserId ile ilişkilendirilir.
/// 
/// Neden handler var?
/// - Controller'ın token claim detaylarını bilmemesi için.
/// - /api/profile/me endpointinin use-case mantığını Application katmanında tutmak için.
/// - İleride validation/logging pipeline behavior'larının bu akışa otomatik dahil olabilmesi için.
/// </summary>
public sealed class GetCurrentUserInfoQueryHandler
    : IRequestHandler<GetCurrentUserInfoQuery, CurrentUserInfoResponse>
{
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Handler ihtiyacı olan current user servisini constructor injection ile alır.
    /// 
    /// ICurrentUserService:
    /// - HttpContext/JWT claim okuma detayını Application katmanından saklar.
    /// - Application katmanına sadece current user bilgisini abstraction olarak verir.
    /// </summary>
    public GetCurrentUserInfoQueryHandler(
        ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Query çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public Task<CurrentUserInfoResponse> Handle(
        GetCurrentUserInfoQuery request,
        CancellationToken cancellationToken)
    {
        // KeycloakUserId zorunludur.
        // Eğer kullanıcı authenticated değilse veya token içinde "sub" claim'i okunamazsa
        // ICurrentUserService uygun UnauthorizedAccessException fırlatır.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // Bu response tamamen token bilgisiyle üretilir.
        // Burada repository yok, UnitOfWork yok, SaveChanges yok.
        var response = new CurrentUserInfoResponse
        {
            IsAuthenticated = _currentUserService.IsAuthenticated,
            KeycloakUserId = keycloakUserId,
            Email = _currentUserService.Email ?? string.Empty,
            Username = _currentUserService.Username ?? string.Empty,

            // Rolleri temizleyip tekrar edenleri ayıklıyoruz.
            // Bu sadece response kalitesini artırır; authorization zaten policy üzerinden çalışır.
            Roles = _currentUserService.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(role => role)
                .ToArray()
        };

        return Task.FromResult(response);
    }
}