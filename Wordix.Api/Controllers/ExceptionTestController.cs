using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Common.Exceptions;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

namespace Wordix.Api.Controllers;

/// <summary>
/// ExceptionMiddleware davranışını test etmek için oluşturulmuş geçici controller.
/// 
/// Dikkat:
/// Bu controller production endpoint değildir.
/// Sadece Faz 10'da merkezi hata yakalama mekanizmasını Swagger üzerinden
/// test etmek için kullanılır.
/// 
/// Amaç:
/// - NotFoundException → 404 dönüyor mu?
/// - BusinessRuleException → 400 dönüyor mu?
/// - ForbiddenException → 403 dönüyor mu?
/// - ValidationException → 400 + validationErrors dönüyor mu?
/// - Beklenmeyen Exception → 500 dönüyor mu?
/// </summary>
[ApiController]
[Route("api/exception-test")]
[Authorize(Policy = "AdminOnly")]
public sealed class ExceptionTestController : ControllerBase
{
    /// <summary>
    /// Başarılı response formatını test etmek için basit endpoint.
    /// 
    /// Beklenen:
    /// HTTP 200
    /// success: true
    /// data dolu
    /// </summary>
    [HttpGet("success")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Success()
    {
        var data = new
        {
            Message = "Exception test controller is working.",
            Controller = nameof(ExceptionTestController)
        };

        return Ok(ApiResponse<object>.Ok(data, "Success response test completed."));
    }

    /// <summary>
    /// NotFoundException test endpointidir.
    /// 
    /// Beklenen:
    /// HTTP 404
    /// errorCode: NOT_FOUND
    /// </summary>
    [HttpGet("not-found")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult ThrowNotFound()
    {
        throw new NotFoundException("LearningItem", Guid.NewGuid());
    }

    /// <summary>
    /// BusinessRuleException test endpointidir.
    /// 
    /// Beklenen:
    /// HTTP 400
    /// errorCode: DICTIONARY_ITEM_ALREADY_EXISTS
    /// </summary>
    [HttpGet("business-rule")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowBusinessRule()
    {
        throw new BusinessRuleException(
            "This learning item is already saved in your dictionary.",
            "DICTIONARY_ITEM_ALREADY_EXISTS");
    }

    /// <summary>
    /// ForbiddenException test endpointidir.
    /// 
    /// Beklenen:
    /// HTTP 403
    /// errorCode: FORBIDDEN
    /// </summary>
    [HttpGet("forbidden")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public IActionResult ThrowForbidden()
    {
        throw new ForbiddenException();
    }

    /// <summary>
    /// ValidationException test endpointidir.
    /// 
    /// Beklenen:
    /// HTTP 400
    /// errorCode: VALIDATION_ERROR
    /// validationErrors dolu
    /// </summary>
    [HttpGet("validation")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult ThrowValidation()
    {
        var validationErrors = new List<ValidationError>
        {
            new(
                propertyName: "Text",
                errorMessage: "Text cannot be empty."),

            new(
                propertyName: "QuestionCount",
                errorMessage: "Question count must be between 1 and 50.")
        };

        throw new WordixValidationException(validationErrors);
    }

    /// <summary>
    /// Beklenmeyen sistem hatası test endpointidir.
    /// 
    /// Beklenen:
    /// HTTP 500
    /// errorCode: INTERNAL_SERVER_ERROR
    /// Development ortamında detail dolu olabilir.
    /// Production ortamında detail null dönmelidir.
    /// </summary>
    [HttpGet("unhandled")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowUnhandled()
    {
        throw new InvalidOperationException("This is a test unhandled exception.");
    }
}