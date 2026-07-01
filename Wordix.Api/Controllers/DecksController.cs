using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.Decks.Dtos.Requests;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Application.Features.Decks.Queries.GetDeckById;
using Wordix.Application.Features.Decks.Queries.GetMyDecks;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Kullanıcının deck işlemlerini yöneten controller.
/// 
/// Deck nedir?
/// - Kullanıcının kendi dictionary itemlarını grupladığı çalışma koleksiyonudur.
/// - İleride deck üzerinden quiz başlatılabilir.
/// 
/// Bu controller ne yapar?
/// - HTTP request alır.
/// - Request DTO'yu mapper üzerinden command/query modeline çevirir.
/// - MediatR üzerinden Application katmanına gönderir.
/// - ApiResponse içine sarıp döner.
/// 
/// Bu controller ne yapmaz?
/// - Current user çözmez.
/// - Duplicate deck kontrolü yapmaz.
/// - Database'e gitmez.
/// - Business rule çalıştırmaz.
/// - Elle validation yapmaz.
/// </summary>
[ApiController]
[Route("api/decks")]
[Authorize]
public sealed class DecksController : ControllerBase
{
    private readonly ISender _sender;

    public DecksController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Current user için yeni bir deck oluşturur.
    /// 
    /// Endpoint:
    /// POST /api/decks
    /// 
    /// Örnek request:
    /// {
    ///   "name": "Software English",
    ///   "description": "Internship and backend related words"
    /// }
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDeckResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateDeckResponse>>> CreateDeck(
        [FromBody] CreateDeckRequest? request,
        CancellationToken cancellationToken)
    {
        // Request DTO -> Command dönüşümü feature mapper üzerinden yapılır.
        //
        // Controller burada null body veya empty name kontrolü yapmaz.
        // CreateDeckCommandValidator bu hataları ValidationBehavior üzerinden yakalar.
        var command = DeckMapper.ToCreateDeckCommand(request);

        var response = await _sender.Send(
            command,
            cancellationToken);

        var apiResponse = ApiResponse<CreateDeckResponse>.Ok(
            data: response,
            message: "Deck created successfully.");

        return CreatedAtAction(
            actionName: nameof(GetDeckById),
            routeValues: new { id = response.DeckId },
            value: apiResponse);
    }


    /// <summary>
    /// Current user'ın kendi deck listesini döner.
    /// 
    /// Endpoint:
    /// GET /api/decks
    /// 
    /// Kullanıcı id request'ten alınmaz.
    /// Current user token üzerinden handler tarafında bulunur.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMyDecksResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetMyDecksResponse>>> GetMyDecks(
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetMyDecksQuery(),
            cancellationToken);

        return Ok(ApiResponse<GetMyDecksResponse>.Ok(
            data: response,
            message: "Decks retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın tek bir deck detayını döner.
    /// 
    /// Endpoint:
    /// GET /api/decks/{id}
    /// 
    /// Buradaki id DeckId değeridir.
    /// Kullanıcı sadece kendi deck detayını görebilir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeckDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DeckDetailResponse>>> GetDeckById(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Route'tan gelen id query modeline aktarılır.
        //
        // Controller burada Guid.Empty kontrolü yapmaz.
        // GetDeckByIdQueryValidator bu kontrolü ValidationBehavior üzerinden yapar.
        var query = new GetDeckByIdQuery(id);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<DeckDetailResponse>.Ok(
            data: response,
            message: "Deck detail retrieved successfully."));
    }


    /// <summary>
    /// Current user'ın kendi deck'ine bir dictionary item ekler.
    /// 
    /// Endpoint:
    /// POST /api/decks/{deckId}/items
    /// 
    /// Örnek request:
    /// {
    ///   "userLearningItemId": "..."
    /// }
    /// 
    /// Önemli:
    /// userLearningItemId global LearningItemId değildir.
    /// Kullanıcının kendi dictionary item id değeridir.
    /// </summary>
    [HttpPost("{deckId:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<AddItemToDeckResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AddItemToDeckResponse>>> AddItemToDeck(
        Guid deckId,
        [FromBody] AddItemToDeckRequest? request,
        CancellationToken cancellationToken)
    {
        // Route deckId + body request DTO -> Command dönüşümü DeckMapper üzerinden yapılır.
        //
        // Controller burada Guid.Empty, deck ownership veya duplicate kontrolü yapmaz.
        // Bu kontroller validator ve handler sorumluluğundadır.
        var command = DeckMapper.ToAddItemToDeckCommand(
            deckId: deckId,
            request: request);

        var response = await _sender.Send(
            command,
            cancellationToken);

        var apiResponse = ApiResponse<AddItemToDeckResponse>.Ok(
            data: response,
            message: "Item added to deck successfully.");

        return Created(
            uri: $"/api/decks/{response.DeckId}",
            value: apiResponse);
    }


    /// <summary>
    /// Current user'ın kendi deck'inden bir dictionary item çıkarır.
    /// 
    /// Endpoint:
    /// DELETE /api/decks/{deckId}/items/{userLearningItemId}
    /// 
    /// Önemli:
    /// userLearningItemId global LearningItemId değildir.
    /// DeckItem.UserLearningItemId değeridir.
    /// </summary>
    [HttpDelete("{deckId:guid}/items/{userLearningItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RemoveItemFromDeckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RemoveItemFromDeckResponse>>> RemoveItemFromDeck(
        Guid deckId,
        Guid userLearningItemId,
        CancellationToken cancellationToken)
    {
        // Route değerleri Command modeline DeckMapper üzerinden dönüştürülür.
        //
        // Controller burada deck ownership, deck item var mı veya Guid.Empty kontrolü yapmaz.
        // Bu kontroller validator ve handler sorumluluğundadır.
        var command = DeckMapper.ToRemoveItemFromDeckCommand(
            deckId: deckId,
            userLearningItemId: userLearningItemId);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(ApiResponse<RemoveItemFromDeckResponse>.Ok(
            data: response,
            message: "Item removed from deck successfully."));
    }

}