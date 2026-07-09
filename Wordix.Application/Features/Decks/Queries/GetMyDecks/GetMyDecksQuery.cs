using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Queries.GetMyDecks;

/// <summary>
/// Current user'ın kendi deck listesini getiren query modelidir.
/// 
/// Current user bilgisi CurrentUserBehavior tarafından pipeline içinde doldurulur.
/// </summary>
public sealed record GetMyDecksQuery
    : IRequest<GetMyDecksResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}