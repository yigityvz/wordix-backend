using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetConfidenceScoreDistribution;

/// <summary>
/// Current user'ın confidence score dağılımını getiren query modelidir.
/// 
/// Bu veri frontend'de grafik/chart çizimi için kullanılabilir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed class GetConfidenceScoreDistributionQuery
    : IRequest<ConfidenceScoreDistributionResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}