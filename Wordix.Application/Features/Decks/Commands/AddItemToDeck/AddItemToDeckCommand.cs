using MediatR;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Commands.AddItemToDeck;

/// <summary>
/// Kullanıcının kendi deck'ine dictionary item ekleme isteğini temsil eden command modelidir.
/// </summary>
public sealed record AddItemToDeckCommand : IRequest<AddItemToDeckResponse>
{
    /// <summary>
    /// Item eklenecek deck id değeridir.
    /// Route üzerinden gelir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Kullanıcının dictionary item id değeridir.
    /// Body üzerinden gelir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }
}