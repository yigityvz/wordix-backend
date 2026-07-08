namespace Wordix.Application.Features.AdminAnalytics.Dtos.Responses;

/// <summary>
/// En çok kaydedilen LearningItem içerikleri endpointinin ana response modelidir.
/// 
/// Endpoint hedefi:
/// GET /api/admin/analytics/top-saved
/// 
/// Kaynak tablolar:
/// - UserLearningItems
/// - LearningItems
/// - Words / Phrases / Sentences
/// - Meanings
/// 
/// Bu endpoint admin'e şunu gösterir:
/// - Kullanıcılar hangi içerikleri dictionary'ye en çok kaydediyor?
/// - İçeriklerin kaynak/kalite durumu ne?
/// - CEFR ve Difficulty dağılımı nasıl?
/// </summary>
public sealed class TopSavedLearningItemsAnalyticsResponse
{
    public DateTimeOffset? From { get; init; }

    public DateTimeOffset? To { get; init; }

    public int Limit { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyCollection<TopSavedLearningItemResponse> Items { get; init; }
        = Array.Empty<TopSavedLearningItemResponse>();
}

/// <summary>
/// Tek bir LearningItem için save analytics satırıdır.
/// </summary>
public sealed class TopSavedLearningItemResponse
{
    public Guid LearningItemId { get; init; }

    public string ItemType { get; init; } = string.Empty;

    public string DisplayText { get; init; } = string.Empty;

    public string? PrimaryMeaning { get; init; }

    public int SaveCount { get; init; }

    public int ActiveSaveCount { get; init; }

    public int UniqueUserCount { get; init; }

    public string ContentSource { get; init; } = string.Empty;

    public string QualityStatus { get; init; } = string.Empty;

    public string CefrLevel { get; init; } = string.Empty;

    public string DifficultyGroup { get; init; } = string.Empty;

    public DateTimeOffset LastSavedAt { get; init; }
}