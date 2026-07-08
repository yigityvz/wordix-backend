using MediatR;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetTopSearches;

/// <summary>
/// En çok aranan metinleri getiren MediatR query modelidir.
/// 
/// Kaynak veri:
/// - LookupHistories
/// 
/// Bu query sonucunda admin:
/// - Kullanıcılar en çok ne arıyor?
/// - Database hit/provider fallback oranı nasıl?
/// - Hangi aramalar yeni içerik oluşturuyor?
/// sorularına cevap alır.
/// </summary>
public sealed class GetTopSearchesQuery
    : IRequest<TopSearchesAnalyticsResponse>
{
    public GetTopSearchesQuery(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int? limit = null)
    {
        FromUtc = fromUtc;
        ToUtc = toUtc;
        Limit = limit;
    }

    public DateTime? FromUtc { get; }

    public DateTime? ToUtc { get; }

    /// <summary>
    /// Dönecek maksimum kayıt sayısıdır.
    /// Null ise handler default limit kullanır.
    /// </summary>
    public int? Limit { get; }
}