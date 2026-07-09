using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Queries.GetDeckById;

/// <summary>
/// Current user'ın tek bir deck detayını getiren query modelidir.
/// 
/// Current user bilgisi CurrentUserBehavior tarafından pipeline içinde doldurulur.
/// </summary>
public sealed record GetDeckByIdQuery(Guid DeckId)
    : IRequest<DeckDetailResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}