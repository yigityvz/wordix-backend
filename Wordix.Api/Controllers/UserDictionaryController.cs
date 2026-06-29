using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;
using Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;
using Wordix.Application.Features.UserDictionary.Requests;
using Wordix.Application.Features.UserDictionary.Responses;
using Wordix.Shared.Responses;

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
/// - Elle validation yapmaz.
/// 
/// Bunların tamamı Application katmanındaki command/query handler'larda
/// ve ValidationBehavior üzerinden çalışan validatorlarda yapılır.
/// </summary>
[ApiController]
[Route("api/user-dictionary")]
[Authorize]
public sealed class UserDictionaryController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden command/query göndermek için kullanılır.
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
    /// 2. SaveLearningItemRequest, UserDictionaryMapper üzerinden SaveLearningItemCommand'e dönüştürülür.
    /// 3. MediatR üzerinden handler'a gönderilir.
    /// 4. ValidationBehavior, SaveLearningItemCommandValidator'ı çalıştırır.
    /// 5. Handler UserLearningItem + UserLearningProgress + LearningProgressHistory oluşturur.
    /// 6. Response ApiResponse içine sarılarak döner.
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
        // Request DTO → Command dönüşümü feature mapper üzerinden yapılır.
        //
        // Controller burada null body veya Guid.Empty kontrolü yapmaz.
        // Eğer request null gelirse mapper LearningItemId = Guid.Empty olan command üretir.
        // SaveLearningItemCommandValidator bu durumu ValidationBehavior üzerinden yakalar.
        var command = UserDictionaryMapper.ToSaveLearningItemCommand(request);

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
        // Route'tan gelen id query modeline aktarılır.
        //
        // Controller burada Guid.Empty kontrolü yapmaz.
        // GetUserDictionaryItemByIdQueryValidator, ValidationBehavior üzerinden bu kontrolü yapar.
        var query = new GetUserDictionaryItemByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<UserDictionaryItemResponse>.Ok(
            data: response,
            message: "Dictionary item retrieved successfully."));
    }
}