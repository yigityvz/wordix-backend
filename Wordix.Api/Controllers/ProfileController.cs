using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.Profile.Queries.GetCurrentUserInfo;
using Wordix.Application.Features.Profile.Dtos.Responses;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Kullanıcının authenticated token bilgileriyle ilgili endpointleri yöneten controller.
/// 
/// Bu controller login/register endpointi değildir.
/// Kullanıcı kimlik doğrulaması Keycloak tarafından yapılır.
/// 
/// Bu controller yalnızca geçerli Keycloak access token'ı ile gelen kullanıcının
/// backend tarafından okunabilen token bilgilerini döndürür.
/// 
/// Önemli:
/// - Bu endpoint UserProfile oluşturmaz.
/// - Database'e gitmez.
/// - UserPreference oluşturmaz.
/// - Sadece doğrulanmış JWT token içindeki current user bilgilerini döndürür.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR'ın sadece request gönderme operasyonları için kullanılan abstraction'ıdır.
    /// 
    /// Neden IMediator değil ISender?
    /// - Bu controller sadece request gönderiyor.
    /// - Publish/notification kullanmıyor.
    /// - Daha dar interface kullanmak Interface Segregation prensibine daha uygundur.
    /// </summary>
    public ProfileController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// O anki authenticated kullanıcının token bilgilerini döndürür.
    /// 
    /// Endpoint:
    /// GET /api/profile/me
    /// 
    /// Bu endpoint login/register işlemi yapmaz.
    /// Backend bu endpointte kullanıcı adı/şifre almaz.
    /// Kullanıcı zaten Keycloak tarafından doğrulanmış ve backend'e access token ile gelmiştir.
    /// 
    /// Yeni akış:
    /// 1. Endpoint [Authorize] ile korunur.
    /// 2. JWT Bearer middleware Keycloak token'ını doğrular.
    /// 3. Controller GetCurrentUserInfoQuery oluşturur.
    /// 4. Query MediatR'a gönderilir.
    /// 5. Handler ICurrentUserService üzerinden token claim bilgilerini okur.
    /// 6. Response DTO döner.
    /// 7. Controller standart ApiResponse formatında sonucu döner.
    /// 
    /// Bu endpointin amacı:
    /// - Mobil uygulamanın "ben kimim?" bilgisini alması.
    /// - Swagger/Postman testlerinde token claimlerinin doğru okunduğunu görmek.
    /// - Role/email/username bilgisini frontend tarafına güvenli şekilde sunmak.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CurrentUserInfoResponse>>> GetMe(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetCurrentUserInfoQuery(),
            cancellationToken);

        return Ok(ApiResponse<CurrentUserInfoResponse>.Ok(
            data: response,
            message: "Current user information retrieved successfully."));
    }
}
