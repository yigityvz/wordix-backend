using MediatR;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetTopSavedLearningItems;

/// <summary>
/// Kullanıcılar tarafından en çok dictionary'ye kaydedilen LearningItem kayıtlarını getiren query modelidir.
/// 
/// Kaynak veri:
/// - UserLearningItems
/// - LearningItems
/// - Words / Phrases / Sentences
/// - Meanings
/// 
/// LearningItem merkezli çalışır.
/// Böylece sadece Word değil, Phrase ve ileride Sentence içerikleri de desteklenir.
/// </summary>
public sealed class GetTopSavedLearningItemsQuery
    : IRequest<TopSavedLearningItemsAnalyticsResponse>
{
    public GetTopSavedLearningItemsQuery(
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

    public int? Limit { get; }
}