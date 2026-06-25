using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.Profile.Queries.GetCurrentUserProfile;
using Wordix.Application.Features.Profile.Responses;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Kullanıcının kendi profil bilgileriyle ilgili endpointleri yöneten controller.
/// 
/// Bu controller artık iş mantığını doğrudan application service üzerinden çağırmaz.
/// Bunun yerine MediatR üzerinden query gönderir.
/// 
/// Akış:
/// Controller
/// → ISender.Send(query)
/// → QueryHandler
/// → Application service/repository
/// → Response DTO
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
    /// O anki authenticated kullanıcının Wordix profilini döndürür.
    /// 
    /// Akış:
    /// 1. Endpoint [Authorize] ile korunur.
    /// 2. Controller GetCurrentUserProfileQuery oluşturur.
    /// 3. Query MediatR'a gönderilir.
    /// 4. GetCurrentUserProfileQueryHandler çalışır.
    /// 5. Handler UserProfile sync işlemini yapar.
    /// 6. Response DTO döner.
    /// 7. Controller standart ApiResponse formatında sonucu döner.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CurrentUserProfileResponse>>> GetMe(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetCurrentUserProfileQuery(),
            cancellationToken);

        return Ok(ApiResponse<CurrentUserProfileResponse>.Ok(
            data: response,
            message: "Current user profile retrieved successfully."));
    }
}