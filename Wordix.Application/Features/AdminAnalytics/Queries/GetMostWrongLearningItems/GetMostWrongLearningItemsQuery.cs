using MediatR;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetMostWrongLearningItems;

/// <summary>
/// Quizlerde en çok yanlış yapılan LearningItem kayıtlarını getiren query modelidir.
/// 
/// Kaynak veri:
/// - QuizAnswers
/// - QuizQuestions
/// - LearningItems
/// 
/// Bu query LearningItem bazlı çalışır.
/// Böylece Word/Phrase/Sentence fark etmeksizin zorlanılan içerikler analiz edilebilir.
/// </summary>
public sealed class GetMostWrongLearningItemsQuery
    : IRequest<MostWrongLearningItemsAnalyticsResponse>
{
    public GetMostWrongLearningItemsQuery(
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