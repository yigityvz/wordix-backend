namespace Wordix.Application.Features.UserStatistics.Dtos.Responses;

/// <summary>
/// Kullanıcının zorlandığı tek bir learning item satırını temsil eder.
/// 
/// Bu DTO doğrudan PagedResult içinde dönecektir:
/// PagedResult&lt;DifficultLearningItemResponse&gt;
/// 
/// Neden ayrı DifficultLearningItemsResponse yazmıyoruz?
/// - Pagination bilgisi zaten Shared/PagedResult içinde standart hale getirildi.
/// - Bu sayede ileride başka list endpointlerinde de aynı response formatı kullanılır.
/// </summary>
public sealed class DifficultLearningItemResponse
{
    public Guid UserLearningItemId { get; init; }

    public Guid LearningItemId { get; init; }

    public string ItemType { get; init; } = string.Empty;

    public string DisplayText { get; init; } = string.Empty;

    public string? PrimaryMeaning { get; init; }

    public string LearningStatus { get; init; } = string.Empty;

    public int ConfidenceScore { get; init; }

    public int CorrectCount { get; init; }

    public int WrongCount { get; init; }

    public int ConsecutiveWrongCount { get; init; }

    public int RepetitionLevel { get; init; }

    public bool IsManuallyMarkedDifficult { get; init; }

    public bool IsProgressDifficult { get; init; }

    /// <summary>
    /// Kullanıcıya veya frontend'e neden difficult sayıldığını açıklayan kısa metindir.
    /// 
    /// Örnek:
    /// - Manually marked as difficult
    /// - Low confidence score
    /// - Consecutive wrong answers
    /// - Review is due
    /// </summary>
    public string DifficultyReason { get; init; } = string.Empty;

    public DateTimeOffset SavedAt { get; init; }

    public DateTimeOffset? LastReviewedAt { get; init; }

    public DateTimeOffset? NextReviewDate { get; init; }
}