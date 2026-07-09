using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetUserLearningSummary;

/// <summary>
/// Current user'ın genel öğrenme özetini getiren MediatR query modelidir.
/// 
/// Bu query parametre almaz.
/// Kullanıcı id client'tan alınmaz.
/// CurrentUserBehavior tarafından request.KeycloakUserId üzerine yazılır.
/// </summary>
public sealed class GetUserLearningSummaryQuery
    : IRequest<UserLearningSummaryResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}