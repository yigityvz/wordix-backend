namespace Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

/// <summary>
/// En çok aranan içerikler endpointinin ana response modelidir.
/// 
/// Endpoint hedefi:
/// GET /api/admin/analytics/top-searches
/// 
/// Kaynak tablo:
/// LookupHistories
/// 
/// Bu endpoint admin'e şunu gösterir:
/// - Kullanıcılar en çok ne arıyor?
/// - Bu aramalar database'de bulunuyor mu?
/// - Provider fallback ne kadar kullanılıyor?
/// - Provider sonucunda yeni global içerik oluşturuluyor mu?
/// </summary>
public sealed class TopSearchesAnalyticsResponse
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public int Limit { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyCollection<TopSearchedItemResponse> Items { get; init; }
        = Array.Empty<TopSearchedItemResponse>();
}

/// <summary>
/// Tek bir arama metni için analytics satırıdır.
/// </summary>
public sealed class TopSearchedItemResponse
{
    public string QueryText { get; init; } = string.Empty;

    public string NormalizedQueryText { get; init; } = string.Empty;

    public string InputType { get; init; } = string.Empty;

    public int SearchCount { get; init; }

    public int UniqueUserCount { get; init; }

    public int DatabaseHitCount { get; init; }

    public int ProviderUsageCount { get; init; }

    public int ProviderCreatedCount { get; init; }

    public int NotFoundCount { get; init; }

    public Guid? LearningItemId { get; init; }

    public DateTimeOffset LastSearchedAt { get; init; }
}