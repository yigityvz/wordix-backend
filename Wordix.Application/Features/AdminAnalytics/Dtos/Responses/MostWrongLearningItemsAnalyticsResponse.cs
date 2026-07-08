namespace Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

/// <summary>
/// En çok yanlış yapılan LearningItem içerikleri endpointinin ana response modelidir.
/// 
/// Endpoint hedefi:
/// GET /api/admin/analytics/most-wrong
/// 
/// Kaynak tablolar:
/// - QuizAnswers
/// - QuizQuestions
/// - LearningItems
/// - Words / Phrases / Sentences
/// 
/// Bu endpoint admin'e şunu gösterir:
/// - Kullanıcılar hangi içeriklerde en çok hata yapıyor?
/// - Hata oranı yüksek içerikler neler?
/// - Sistem önerisi olarak gelen içerikler yanlış yapılıyor mu?
/// - Ortalama cevap süresi nasıl?
/// </summary>
public sealed class MostWrongLearningItemsAnalyticsResponse
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public int Limit { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyCollection<MostWrongLearningItemResponse> Items { get; init; }
        = Array.Empty<MostWrongLearningItemResponse>();
}

/// <summary>
/// Tek bir LearningItem için wrong-answer analytics satırıdır.
/// </summary>
public sealed class MostWrongLearningItemResponse
{
    public Guid LearningItemId { get; init; }

    public string ItemType { get; init; } = string.Empty;

    public string DisplayText { get; init; } = string.Empty;

    public int WrongAnswerCount { get; init; }

    public int CorrectAnswerCount { get; init; }

    public int TotalAnswerCount { get; init; }

    /// <summary>
    /// Yanlış oranıdır.
    /// 
    /// Örnek:
    /// 42.75
    /// </summary>
    public double WrongRate { get; init; }

    public double AverageResponseTimeInMilliseconds { get; init; }

    public int SystemRecommendedWrongAnswerCount { get; init; }

    public DateTimeOffset LastWrongAt { get; init; }
}