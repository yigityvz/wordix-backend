using Wordix.Domain.Enums;

namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// En çok kaydedilen LearningItem içerikleri için repository aggregate sonucudur.
/// 
/// Bu model LearningItem merkezlidir.
/// Böylece sadece Word'e kilitlenmeyiz; Phrase ve ileride Sentence aynı analytics akışına girebilir.
/// </summary>
public sealed class TopSavedLearningItemAnalyticsModel
{
    public Guid LearningItemId { get; init; }

    public LearningItemType ItemType { get; init; }

    public string DisplayText { get; init; } = string.Empty;

    public string? PrimaryMeaning { get; init; }

    public int SaveCount { get; init; }

    public int ActiveSaveCount { get; init; }

    public int UniqueUserCount { get; init; }

    public ContentSource ContentSource { get; init; }

    public ContentQualityStatus QualityStatus { get; init; }

    public CefrLevel CefrLevel { get; init; }

    public DifficultyGroup DifficultyGroup { get; init; }

    public DateTime LastSavedAtUtc { get; init; }
}