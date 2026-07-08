using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Kullanıcının zorlandığı tek bir learning item için repository aggregate/detail sonucudur.
/// 
/// Bu model API response değildir.
/// Mapper içinde DifficultLearningItemResponse'a dönüştürülür.
/// </summary>
public sealed class DifficultLearningItemModel
{
    public Guid UserLearningItemId { get; init; }

    public Guid LearningItemId { get; init; }

    public LearningItemType ItemType { get; init; }

    public string DisplayText { get; init; } = string.Empty;

    public string? PrimaryMeaning { get; init; }

    public LearningStatus LearningStatus { get; init; }

    public int ConfidenceScore { get; init; }

    public int CorrectCount { get; init; }

    public int WrongCount { get; init; }

    public int ConsecutiveWrongCount { get; init; }

    public int RepetitionLevel { get; init; }

    public bool IsManuallyMarkedDifficult { get; init; }

    public bool IsProgressDifficult { get; init; }

    public string DifficultyReason { get; init; } = string.Empty;

    public DateTime SavedAtUtc { get; init; }

    public DateTime? LastReviewedAtUtc { get; init; }

    public DateTime? NextReviewDateUtc { get; init; }
}