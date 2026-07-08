namespace Wordix.Application.Features.UserStatistics.Dtos.Responses;

/// <summary>
/// Kullanıcının genel öğrenme özetini temsil eden response DTO'sudur.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/learning-summary
/// 
/// Bu response dashboard'un üst kartlarını besleyebilir:
/// - Toplam kaydedilen içerik
/// - Öğrenme durumları
/// - Ortalama confidence score
/// - Flag sayıları
/// - Genel quiz doğruluk oranı
/// - Review zamanı gelen item sayısı
/// </summary>
public sealed class UserLearningSummaryResponse
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

    /// <summary>
    /// Kullanıcının genel doğru cevap oranıdır.
    /// 
    /// Örnek:
    /// 78.45
    /// </summary>
    public double OverallAccuracyRate { get; init; }

    public DateTimeOffset? LastReviewedAt { get; init; }

    public DateTimeOffset? NextReviewDate { get; init; }

    public DateTimeOffset? LastQuizStartedAt { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}