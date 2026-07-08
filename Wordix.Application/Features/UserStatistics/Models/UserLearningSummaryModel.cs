namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Kullanıcının öğrenme özetini repository'den handler'a taşıyan internal modeldir.
/// 
/// Bu model API response değildir.
/// Response DTO'ya dönüşüm UserStatisticsMapper içinde yapılacaktır.
/// </summary>
public sealed class UserLearningSummaryModel
{
    public int TotalSavedItemCount { get; init; }

    public int ActiveSavedItemCount { get; init; }

    public int WordCount { get; init; }

    public int PhraseCount { get; init; }

    public int SentenceCount { get; init; }

    public int NewItemCount { get; init; }

    public int LearningItemCount { get; init; }

    public int ReviewingItemCount { get; init; }

    public int LearnedItemCount { get; init; }

    public int MasteredItemCount { get; init; }

    public int ReviewDueItemCount { get; init; }

    public double AverageConfidenceScore { get; init; }

    public int FavoriteItemCount { get; init; }

    public int DifficultItemCount { get; init; }

    public int WantMorePracticeItemCount { get; init; }

    public int IgnoredItemCount { get; init; }

    public int TotalCorrectAnswerCount { get; init; }

    public int TotalIncorrectAnswerCount { get; init; }

    public int TotalPartiallyCorrectAnswerCount { get; init; }

    public int TotalSkippedAnswerCount { get; init; }

    public double OverallAccuracyRate { get; init; }

    public DateTime? LastReviewedAtUtc { get; init; }

    public DateTime? NextReviewDateUtc { get; init; }

    public DateTime? LastQuizStartedAtUtc { get; init; }

    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}