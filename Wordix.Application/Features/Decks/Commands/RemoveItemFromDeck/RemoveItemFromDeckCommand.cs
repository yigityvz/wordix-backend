using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Common.Interfaces.Persistence;

namespace Wordix.Application.Features.Decks.Commands.RemoveItemFromDeck;

/// <summary>
/// Kullanıcının kendi deck'inden dictionary item çıkarma isteğini temsil eden command modelidir.
/// </summary>
public sealed record RemoveItemFromDeckCommand
    : IRequest<RemoveItemFromDeckResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// İşlem yapılacak deck id değeridir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Deck'ten çıkarılacak UserLearningItem id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }
}