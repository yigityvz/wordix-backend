using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;
using Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;
using Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;
using Wordix.Application.Features.UserDictionary.Requests;
using Wordix.Application.Features.UserDictionary.Responses;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

namespace Wordix.Api.Controllers;

/// <summary>
/// Kullanıcının kişisel dictionary işlemlerini yöneten controller.
/// 
/// Bu controller ne yapar?
/// - Kullanıcının bir LearningItem'ı dictionary'sine kaydetmesini sağlar.
/// - Kullanıcının kendi dictionary listesini döner.
/// - Kullanıcının kendi dictionary item detayını döner.
/// 
/// Bu controller ne yapmaz?
/// - Current user çözmez.
/// - LearningItem var mı diye database'e gitmez.
/// - Duplicate kontrolü yapmaz.
/// - UserLearningItem oluşturmaz.
/// - UserLearningProgress oluşturmaz.
/// - Ownership/business rule kontrolü yapmaz.
/// 
/// Bunların tamamı Application katmanındaki command/query handler'larda yapılır.
/// </summary>
[ApiController]
[Route("api/user-dictionary")]
[Authorize]
public sealed class UserDictionaryController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden command/query göndermek için kullanılır.
    /// 
    /// Neden ISender?
    /// - Controller sadece command/query gönderiyor.
    /// - Notification publish etmiyor.
    /// - IMediator yerine daha dar interface kullanmak daha temizdir.
    /// </summary>
    public UserDictionaryController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Kullanıcının bir LearningItem'ı kendi dictionary'sine kaydetmesini sağlar.
    /// 
    /// Endpoint:
    /// POST /api/user-dictionary
    /// 
    /// Örnek request:
    /// {
    ///   "learningItemId": "...",
    ///   "selectedMeaningId": "...",
    ///   "sourceLookupHistoryId": "..."
    /// }
    /// 
    /// Akış:
    /// 1. Request body alınır.
    /// 2. SaveLearningItemCommand'e manual map edilir.
    /// 3. MediatR üzerinden handler'a gönderilir.
    /// 4. Handler UserLearningItem + UserLearningProgress + LearningProgressHistory oluşturur.
    /// 5. Response ApiResponse içine sarılarak döner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SaveLearningItemResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SaveLearningItemResponse>>> SaveLearningItem(
        [FromBody] SaveLearningItemRequest? request,
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
        var command = new SaveLearningItemCommand
        {
            LearningItemId = request.LearningItemId,
            SelectedMeaningId = request.SelectedMeaningId,
            SourceLookupHistoryId = request.SourceLookupHistoryId
        };

        // Command'i MediatR'a gönderiyoruz.
        // Bundan sonra LoggingBehavior, ValidationBehavior ve Handler devreye girer.
        var response = await _sender.Send(command, cancellationToken);

        var apiResponse = ApiResponse<SaveLearningItemResponse>.Ok(
            data: response,
            message: "Learning item saved to dictionary successfully.");

        return CreatedAtAction(
            actionName: nameof(GetUserDictionaryItemById),
            routeValues: new { id = response.UserLearningItemId },
            value: apiResponse);
    }

    /// <summary>
    /// Current user'ın kendi dictionary listesini döner.
    /// 
    /// Endpoint:
    /// GET /api/user-dictionary
    /// 
    /// Kullanıcı id request'ten alınmaz.
    /// Current user token üzerinden handler tarafında bulunur.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMyDictionaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMyDictionaryResponse>>> GetMyDictionary(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetMyDictionaryQuery(),
            cancellationToken);

        return Ok(ApiResponse<GetMyDictionaryResponse>.Ok(
            data: response,
            message: "Dictionary retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın dictionary'sindeki tek bir item detayını döner.
    /// 
    /// Endpoint:
    /// GET /api/user-dictionary/{id}
    /// 
    /// Buradaki id:
    /// UserLearningItemId değeridir.
    /// 
    /// Global LearningItemId değildir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDictionaryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDictionaryItemResponse>>> GetUserDictionaryItemById(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Route'tan Guid.Empty gelirse bu gerçek bir dictionary item id değildir.
        // Query validator yazmadığımız için bu temel kontrolü controller sınırında yapıyoruz.
        if (id == Guid.Empty)
        {
            throw new WordixValidationException(new[]
            {
                new ValidationError(
                    propertyName: "Id",
                    errorMessage: "User dictionary item id is required.",
                    errorCode: "USER_DICTIONARY_ITEM_ID_REQUIRED")
            });
        }

        var response = await _sender.Send(
            new GetUserDictionaryItemByIdQuery(id),
            cancellationToken);

        return Ok(ApiResponse<UserDictionaryItemResponse>.Ok(
            data: response,
            message: "Dictionary item retrieved successfully."));
    }
}