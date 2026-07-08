namespace Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

/// <summary>
/// Provider istatistikleri endpointinin ana response modelidir.
/// 
/// Endpoint hedefi:
/// GET /api/admin/analytics/provider-stats
/// 
/// Kaynak tablolar:
/// - ProviderRequestLogs
/// - ExternalContentCaches
/// - ImportJobs
/// 
/// Bu endpoint admin'e şunu gösterir:
/// - Provider kaç kez çağrıldı?
/// - Başarı/hata/cache oranları nedir?
/// - Ortalama provider süresi nedir?
/// - Cache gerçekten kullanılıyor mu?
/// - İlgili import joblarda hata var mı?
/// </summary>
public sealed class ProviderStatsAnalyticsResponse
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public int TotalProviderRequestCount { get; init; }

    public int TotalExternalCacheEntryCount { get; init; }

    public int TotalExternalCacheHitCount { get; init; }

    public int TotalImportJobCount { get; init; }

    public IReadOnlyCollection<ProviderStatItemResponse> Items { get; init; }
        = Array.Empty<ProviderStatItemResponse>();
}

/// <summary>
/// Tek bir provider + operation kombinasyonu için istatistik satırıdır.
/// </summary>
public sealed class ProviderStatItemResponse
{
    public string ProviderName { get; init; } = string.Empty;

    public string ProviderType { get; init; } = string.Empty;

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

    public DateTimeOffset? LastRequestAt { get; init; }
}