namespace Wordix.Application.Features.UserStatistics.Dtos.Responses;

/// <summary>
/// Kullanıcının deck bazlı istatistiklerini temsil eden response DTO'sudur.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/decks
/// 
/// Bu response current user'ın decklerini ve her deck'in öğrenme/quiz performansını döner.
/// </summary>
public sealed class DeckStatisticsResponse
{
    public int TotalDeckCount { get; init; }

    public int ActiveDeckCount { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyCollection<DeckStatisticsItemResponse> Items { get; init; }
        = Array.Empty<DeckStatisticsItemResponse>();
}

/// <summary>
/// Tek bir deck için statistics satırıdır.
/// </summary>
public sealed class DeckStatisticsItemResponse
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

    public DateTimeOffset? LastQuizStartedAt { get; init; }

    public DateTimeOffset? LastItemAddedAt { get; init; }
}