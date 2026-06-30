using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Profile.Dtos.Responses;
using Wordix.Application.Features.Profile.Mappers;

namespace Wordix.Application.Features.Profile.Queries.GetCurrentUserInfo;

/// <summary>
/// GetCurrentUserInfoQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - ICurrentUserService üzerinden current user's token bilgilerine erişir.
/// - Response DTO üretimini ProfileMapper'a bırakır.
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
/// 
/// Neden mapping burada yapılmıyor?
/// - Handler use-case akışını yönetmelidir.
/// - Response DTO propertylerini tek tek doldurma işi mapper sorumluluğudur.
/// - Böylece mapping kuralları feature seviyesinde merkezi kalır.
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
        // Handler burada response propertylerini tek tek doldurmaz.
        // Current user → response dönüşümü ProfileMapper içinde merkezi olarak yapılır.
        var response = ProfileMapper.ToCurrentUserInfoResponse(_currentUserService);

        return Task.FromResult(response);
    }
}