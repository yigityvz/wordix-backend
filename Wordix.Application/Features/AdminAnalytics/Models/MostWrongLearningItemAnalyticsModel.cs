using Wordix.Domain.Enums;

namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// En çok yanlış yapılan LearningItem içerikleri için repository aggregate sonucudur.
/// 
/// Bu model QuizAnswer + QuizQuestion üzerinden hesaplanır.
/// LearningItem merkezli tutulduğu için Word/Phrase/Sentence genişlemesine uygundur.
/// </summary>
public sealed class MostWrongLearningItemAnalyticsModel
{
    public Guid LearningItemId { get; init; }

    public LearningItemType ItemType { get; init; }

    public string DisplayText { get; init; } = string.Empty;

    public int WrongAnswerCount { get; init; }

    public int CorrectAnswerCount { get; init; }

    public int TotalAnswerCount { get; init; }

    public double WrongRate { get; init; }

    public double AverageResponseTimeInMilliseconds { get; init; }

    public int SystemRecommendedWrongAnswerCount { get; init; }

    public DateTime LastWrongAtUtc { get; init; }
}