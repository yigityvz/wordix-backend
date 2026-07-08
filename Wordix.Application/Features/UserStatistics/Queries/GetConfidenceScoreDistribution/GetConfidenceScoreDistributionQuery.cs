using MediatR;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetConfidenceScoreDistribution;

/// <summary>
/// Current user'ın confidence score dağılımını getiren query modelidir.
/// 
/// Bu veri frontend'de grafik/chart çizimi için kullanılabilir.
/// </summary>
public sealed class GetConfidenceScoreDistributionQuery
    : IRequest<ConfidenceScoreDistributionResponse>
{
}