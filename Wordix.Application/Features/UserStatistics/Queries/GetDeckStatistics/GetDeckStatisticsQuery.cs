using MediatR;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDeckStatistics;

/// <summary>
/// Current user'ın deck bazlı learning/quiz statistics verilerini getiren query modelidir.
/// 
/// Query parametresi yoktur.
/// Kullanıcı id token'dan alınır.
/// </summary>
public sealed class GetDeckStatisticsQuery
    : IRequest<DeckStatisticsResponse>
{
}