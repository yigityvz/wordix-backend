using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Common.Interfaces.Persistence;

namespace Wordix.Application.Features.Decks.Commands.CreateDeck;

/// <summary>
/// Kullanıcının yeni deck oluşturma isteğini temsil eden command modelidir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed record CreateDeckCommand
    : IRequest<CreateDeckResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Deck adıdır.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Deck açıklamasıdır.
    /// </summary>
    public string? Description { get; init; }
}