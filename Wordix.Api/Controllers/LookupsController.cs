using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Lookups.Commands.CreateLookup;
using Wordix.Application.Features.Lookups.Requests;
using Wordix.Application.Features.Lookups.Responses;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

namespace Wordix.Api.Controllers;

/// <summary>
/// Lookup işlemleriyle ilgili endpointleri yöneten controller.
/// 
/// Lookup ne demek?
/// - Kullanıcı bir kelime/ifade/cümle arar.
/// - Sistem önce local database'de arar.
/// - Bulamazsa provider üzerinden anlam bulmaya çalışır.
/// - LookupHistory kaydı oluşturulur.
/// - Sonuç kullanıcıya döndürülür.
/// 
/// Bu controller ne yapmaz?
/// - Text normalize etmez.
/// - Database sorgusu yazmaz.
/// - Provider çağırmaz.
/// - LearningItem/Word/Meaning oluşturmaz.
/// - LookupHistory oluşturmaz.
/// 
/// Bunların tamamı CreateLookupCommandHandler içinde yapılır.
/// Controller sadece HTTP request/response sorumluluğunu taşır.
/// </summary>
[ApiController]
[Route("api/lookups")]
[Authorize]
public sealed class LookupsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden command/query göndermek için kullanılır.
    /// 
    /// Neden ISender?
    /// - Bu controller sadece command gönderiyor.
    /// - Notification publish etmiyor.
    /// - IMediator yerine daha dar interface kullanmak daha temizdir.
    /// </summary>
    public LookupsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Kullanıcının kelime lookup isteğini işler.
    /// 
    /// Endpoint:
    /// POST /api/lookups
    /// 
    /// Örnek request:
    /// {
    ///   "text": "achieve",
    ///   "sourceLanguageCode": "en",
    ///   "targetLanguageCode": "tr"
    /// }
    /// 
    /// Akış:
    /// 1. Request body LookupRequest olarak alınır.
    /// 2. LookupRequest, CreateLookupCommand'e manual map edilir.
    /// 3. Command MediatR'a gönderilir.
    /// 4. ValidationBehavior command'i doğrular.
    /// 5. CreateLookupCommandHandler lookup akışını çalıştırır.
    /// 6. LookupResponse döner.
    /// 7. Controller sonucu ApiResponse<LookupResponse> olarak döner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<LookupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LookupResponse>>> CreateLookup(
        [FromBody] LookupRequest? request,
        CancellationToken cancellationToken)
    {
        // Request body tamamen boş gelirse command'e map edemeyiz.
        // Bu durumda kendi ValidationException'ımızı fırlatıyoruz.
        // ExceptionMiddleware bunu standart 400 VALIDATION_ERROR formatına çevirir.
        if (request is null)
        {
            throw new WordixValidationException(new[]
            {
                new ValidationError(
                    propertyName: "Body",
                    errorMessage: "Request body is required.",
                    errorCode: "REQUEST_BODY_REQUIRED")
            });
        }

        // API request DTO'sunu Application command modeline manual map ediyoruz.
        // AutoMapper/Mapster kullanmıyoruz.
        var command = new CreateLookupCommand
        {
            Text = request.Text,
            SourceLanguageCode = request.SourceLanguageCode,
            TargetLanguageCode = request.TargetLanguageCode
        };

        // Command'i MediatR'a gönderiyoruz.
        // Bundan sonra ValidationBehavior, LoggingBehavior ve Handler devreye girer.
        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<LookupResponse>.Ok(
            data: response,
            message: "Lookup completed successfully."));
    }
}