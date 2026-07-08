namespace Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

/// <summary>
/// Admin dashboard endpointinin response modelidir.
/// 
/// Bu response sistemin genel sağlık ve kullanım özetini verir.
/// 
/// Endpoint hedefi:
/// GET /api/admin/analytics/dashboard
/// 
/// Bu DTO neyi gösterir?
/// - Lookup kullanımını
/// - Dictionary save davranışını
/// - Quiz ve cevap istatistiklerini
/// - Provider/cache/import sağlık verilerini
/// 
/// DTO olduğu için domain davranışı içermez.
/// Sadece API'ye dönecek veri contract'ıdır.
/// </summary>
public sealed class AdminDashboardAnalyticsResponse
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

    /// <summary>
    /// Doğruluk oranıdır.
    /// 
    /// Örnek:
    /// 75.50
    /// </summary>
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

    public DateTimeOffset? LastLookupAt { get; init; }

    public DateTimeOffset? LastDictionarySaveAt { get; init; }

    public DateTimeOffset? LastQuizStartedAt { get; init; }

    public DateTimeOffset? LastProviderRequestAt { get; init; }

    public DateTimeOffset? LastImportJobAt { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}