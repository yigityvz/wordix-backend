using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Common.Interfaces.Persistence;

namespace Wordix.Application.Features.Decks.Commands.AddItemToDeck;

/// <summary>
/// Kullanıcının kendi deck'ine dictionary item ekleme isteğini temsil eden command modelidir.
/// </summary>
public sealed record AddItemToDeckCommand
    : IRequest<AddItemToDeckResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

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