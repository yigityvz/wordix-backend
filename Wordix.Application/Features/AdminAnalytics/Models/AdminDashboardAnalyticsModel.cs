namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// Dashboard analytics verisini repository'den handler'a taşıyan internal modeldir.
/// 
/// Bu model API response DTO değildir.
/// Persistence katmanındaki aggregate sorguların sonucunu Application handler'a taşır.
/// Response DTO'ya dönüşüm AdminAnalyticsMapper içinde yapılacaktır.
/// </summary>
public sealed class AdminDashboardAnalyticsModel
{
    public int LookupCount { get; init; }

    public int UniqueLookupUserCount { get; init; }

    public int DatabaseLookupCount { get; init; }

    public int ProviderLookupCount { get; init; }

    public int ProviderCreatedLookupCount { get; init; }

    public int DictionarySaveCount { get; init; }

    public int ActiveDictionarySaveCount { get; init; }

    public int UniqueDictionaryUserCount { get; init; }

    public int QuizSessionCount { get; init; }

    public int QuizAnswerCount { get; init; }

    public int CorrectAnswerCount { get; init; }

    public int WrongAnswerCount { get; init; }

    public double AverageAccuracyRate { get; init; }

    public int SystemRecommendedQuestionCount { get; init; }

    public int SystemRecommendedWrongAnswerCount { get; init; }

    public int ProviderRequestCount { get; init; }

    public int ProviderSuccessCount { get; init; }

    public int ProviderFailureCount { get; init; }

    public int ProviderTimeoutCount { get; init; }

    public int ProviderRateLimitedCount { get; init; }

    public int ProviderServedFromCacheCount { get; init; }

    public double AverageProviderDurationMs { get; init; }

    public int ExternalCacheEntryCount { get; init; }

    public int ActiveExternalCacheEntryCount { get; init; }

    public int ExternalCacheHitCount { get; init; }

    public int ImportJobCount { get; init; }

    public int CompletedImportJobCount { get; init; }

    public int FailedImportJobCount { get; init; }

    public int RunningImportJobCount { get; init; }

    public DateTime? LastLookupAtUtc { get; init; }

    public DateTime? LastDictionarySaveAtUtc { get; init; }

    public DateTime? LastQuizStartedAtUtc { get; init; }

    public DateTime? LastProviderRequestAtUtc { get; init; }

    public DateTime? LastImportJobAtUtc { get; init; }

    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}