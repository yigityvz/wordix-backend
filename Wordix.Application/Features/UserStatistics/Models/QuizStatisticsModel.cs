namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Quiz statistics aggregate sonucunu repository'den handler'a taşıyan internal modeldir.
/// 
/// Bu model API response değildir.
/// Mapper içinde QuizStatisticsResponse'a dönüştürülür.
/// </summary>
public sealed class QuizStatisticsModel
{
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

    public DateTime? LastQuizStartedAtUtc { get; init; }

    public DateTime? LastAnsweredAtUtc { get; init; }

    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}