using Wordix.Domain.Enums;

namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// En çok aranan içerikler için repository aggregate sonucunu temsil eder.
/// 
/// API response değildir.
/// InputType enum olarak tutulur, response'a string olarak mapper ile taşınır.
/// </summary>
public sealed class TopSearchedItemAnalyticsModel
{
    public string QueryText { get; init; } = string.Empty;

    public string NormalizedQueryText { get; init; } = string.Empty;

    public InputType InputType { get; init; }

    public int SearchCount { get; init; }

    public int UniqueUserCount { get; init; }

    public int DatabaseHitCount { get; init; }

    public int ProviderUsageCount { get; init; }

    public int ProviderCreatedCount { get; init; }

    public int NotFoundCount { get; init; }

    public Guid? LearningItemId { get; init; }

    public DateTime LastSearchedAtUtc { get; init; }
}