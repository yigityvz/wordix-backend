using MediatR;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetProviderStats;

/// <summary>
/// Provider, cache ve import job istatistiklerini getiren query modelidir.
/// 
/// Kaynak veri:
/// - ProviderRequestLogs
/// - ExternalContentCaches
/// - ImportJobs
/// 
/// Bu query admin'e şunu gösterir:
/// - Provider kaç kez çağrıldı?
/// - Başarı/hata/cache oranları nedir?
/// - Cache provider maliyetini düşürüyor mu?
/// - Import joblarda hata var mı?
/// </summary>
public sealed class GetProviderStatsQuery
    : IRequest<ProviderStatsAnalyticsResponse>
{
    public GetProviderStatsQuery(
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        FromUtc = fromUtc;
        ToUtc = toUtc;
    }

    public DateTime? FromUtc { get; }

    public DateTime? ToUtc { get; }
}