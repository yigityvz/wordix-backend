namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Tek bir deck için repository'den gelen statistics modelidir.
/// 
/// Bu model API response değildir.
/// Mapper içinde DeckStatisticsItemResponse'a dönüştürülür.
/// </summary>
public sealed class DeckStatisticsModel
{
    public Guid DeckId { get; init; }

    public string DeckName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public int ItemCount { get; init; }

    public int ActiveItemCount { get; init; }

    public double AverageConfidenceScore { get; init; }

    public int DueReviewItemCount { get; init; }

    public int DifficultItemCount { get; init; }

    public int QuizSessionCount { get; init; }

    public int CompletedQuizSessionCount { get; init; }

    public int TotalAnswerCount { get; init; }

    public int CorrectAnswerCount { get; init; }

    public int IncorrectAnswerCount { get; init; }

    public double AccuracyRate { get; init; }

    public DateTime? LastQuizStartedAtUtc { get; init; }

    public DateTime? LastItemAddedAtUtc { get; init; }
}