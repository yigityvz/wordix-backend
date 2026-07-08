using MediatR;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetAdminDashboard;

/// <summary>
/// Admin dashboard analytics verisini getiren MediatR query modelidir.
/// 
/// Bu query ne yapacak?
/// - Sistem genelindeki lookup, dictionary, quiz, provider, cache ve import özetlerini getirecek.
/// 
/// Bu class sadece veri taşıma modelidir.
/// Gerçek sorgu handler içinde IAdminAnalyticsRepository üzerinden yapılacaktır.
/// </summary>
public sealed class GetAdminDashboardQuery
    : IRequest<AdminDashboardAnalyticsResponse>
{
    public GetAdminDashboardQuery(
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        FromUtc = fromUtc;
        ToUtc = toUtc;
    }

    /// <summary>
    /// Dashboard analiz başlangıç tarihidir.
    /// Null ise handler default tarih aralığı kullanabilir.
    /// </summary>
    public DateTime? FromUtc { get; }

    /// <summary>
    /// Dashboard analiz bitiş tarihidir.
    /// Null ise handler güncel UTC zamanı kullanabilir.
    /// </summary>
    public DateTime? ToUtc { get; }
}