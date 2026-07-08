namespace Wordix.Application.Features.UserStatistics.Dtos.Responses;

/// <summary>
/// Kullanıcının quiz performans istatistiklerini temsil eden response DTO'sudur.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/quizzes
/// 
/// Bu response şunları gösterir:
/// - Kullanıcı kaç quiz başlatmış?
/// - Kaçı tamamlanmış?
/// - Doğru/yanlış/skipped cevap dağılımı nedir?
/// - Ortalama cevap süresi nedir?
/// - Test/Writing/Mixed dağılımı nedir?
/// - Deck/Difficult/SystemRecommendation kaynaklı quiz performansı nasıldır?
/// </summary>
public sealed class QuizStatisticsResponse
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public string? QuizType { get; init; }

    public string? QuizSourceType { get; init; }

    public string? QuizContentMode { get; init; }

    public string? DifficultyGroup { get; init; }

    public int TotalQuizSessionCount { get; init; }

    public int CompletedQuizSessionCount { get; init; }

    public int InProgressQuizSessionCount { get; init; }

    public int CancelledQuizSessionCount { get; init; }

    public int TestQuizSessionCount { get; init; }

    public int WritingQuizSessionCount { get; init; }

    public int MixedQuizSessionCount { get; init; }

    public int UserDictionaryQuizSessionCount { get; init; }

    public int DeckQuizSessionCount { get; init; }

    public int DifficultItemsQuizSessionCount { get; init; }

    public int SystemRecommendationsQuizSessionCount { get; init; }

    public int TotalQuestionCount { get; init; }

    public int SystemRecommendedQuestionCount { get; init; }

    public int TotalAnswerCount { get; init; }

    public int CorrectAnswerCount { get; init; }

    public int IncorrectAnswerCount { get; init; }

    public int PartiallyCorrectAnswerCount { get; init; }

    public int SkippedAnswerCount { get; init; }

    public int SystemRecommendedCorrectAnswerCount { get; init; }

    public int SystemRecommendedIncorrectAnswerCount { get; init; }

    public double AccuracyRate { get; init; }

    public double AverageResponseTimeMs { get; init; }

    public DateTimeOffset? LastQuizStartedAt { get; init; }

    public DateTimeOffset? LastAnsweredAt { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}