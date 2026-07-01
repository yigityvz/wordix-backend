using MediatR;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Commands.RemoveItemFromDeck;

/// <summary>
/// Kullanıcının kendi deck'inden dictionary item çıkarma isteğini temsil eden command modelidir.
/// </summary>
public sealed record RemoveItemFromDeckCommand : IRequest<RemoveItemFromDeckResponse>
{
    /// <summary>
    /// İşlem yapılacak deck id değeridir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Deck'ten çıkarılacak UserLearningItem id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }
}