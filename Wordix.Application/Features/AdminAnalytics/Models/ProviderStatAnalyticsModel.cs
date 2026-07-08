using Wordix.Domain.Enums;

namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// Provider istatistikleri için repository aggregate sonucudur.
/// 
/// ProviderName + ProviderType + OperationName kombinasyonu üzerinden raporlama yapılır.
/// Örnek:
/// - AzureTranslator / Translation / Translate
/// - Tatoeba / ExampleSentence / EnrichTatoebaExampleSentences
/// </summary>
public sealed class ProviderStatAnalyticsModel
{
    public string ProviderName { get; init; } = string.Empty;

    public ProviderType ProviderType { get; init; }

    public string OperationName { get; init; } = string.Empty;

    public int TotalRequestCount { get; init; }

    public int SucceededCount { get; init; }

    public int FailedCount { get; init; }

    public int TimeoutCount { get; init; }

    public int RateLimitedCount { get; init; }

    public int ServedFromCacheCount { get; init; }

    public double AverageDurationMs { get; init; }

    public int CacheEntryCount { get; init; }

    public int CacheHitCount { get; init; }

    public int ImportJobCount { get; init; }

    public int FailedImportJobCount { get; init; }

    public DateTime? LastRequestAtUtc { get; init; }
}