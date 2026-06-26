namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// LearningScoreCalculator tarafından üretilen hesaplama sonucudur.
/// 
/// Bu model:
/// - önceki score'u,
/// - yeni score'u,
/// - score değişim miktarını,
/// - değişim nedenini
/// taşır.
/// </summary>
public sealed class LearningScoreCalculationResult
{
    /// <summary>
    /// Hesaplama öncesindeki confidence score değeridir.
    /// </summary>
    public int PreviousConfidenceScore { get; init; }

    /// <summary>
    /// Hesaplama sonrası confidence score değeridir.
    /// </summary>
    public int NewConfidenceScore { get; init; }

    /// <summary>
    /// Score değişim miktarıdır.
    /// 
    /// Örnek:
    /// +12
    /// -10
    /// </summary>
    public int ScoreDelta => NewConfidenceScore - PreviousConfidenceScore;

    /// <summary>
    /// Cevap doğru muydu?
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya cevaplama süresidir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Score değişiminin açıklamasıdır.
    /// 
    /// Bu değer ileride LearningProgressHistory.ChangeReason içine yazılabilir.
    /// </summary>
    public string ChangeReason { get; init; } = string.Empty;
}