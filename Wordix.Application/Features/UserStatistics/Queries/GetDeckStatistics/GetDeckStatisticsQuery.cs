using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDeckStatistics;

/// <summary>
/// Current user'ın deck bazlı learning/quiz statistics verilerini getiren query modelidir.
/// 
/// Query parametresi yoktur.
/// Kullanıcı id client'tan alınmaz.
/// CurrentUserBehavior tarafından request.KeycloakUserId üzerine yazılır.
/// </summary>
public sealed class GetDeckStatisticsQuery
    : IRequest<DeckStatisticsResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}