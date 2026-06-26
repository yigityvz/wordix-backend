using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// LearningProgressUpdater servisine gönderilecek progress güncelleme input modelidir.
/// 
/// Bu model entity değildir.
/// UserLearningProgress'in mevcut değerlerini ve quiz cevabı sonrası hesaplanan score sonucunu taşır.
/// </summary>
public sealed class LearningProgressUpdateRequest
{
    /// <summary>
    /// Güncellenecek UserLearningProgress id değeridir.
    /// </summary>
    public Guid UserLearningProgressId { get; init; }

    /// <summary>
    /// Progress'in bağlı olduğu UserLearningItem id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Güncelleme öncesi learning status değeridir.
    /// </summary>
    public LearningStatus CurrentLearningStatus { get; init; }

    /// <summary>
    /// Güncelleme öncesi doğru cevap sayısıdır.
    /// </summary>
    public int CurrentCorrectCount { get; init; }

    /// <summary>
    /// Güncelleme öncesi yanlış cevap sayısıdır.
    /// </summary>
    public int CurrentWrongCount { get; init; }

    /// <summary>
    /// Güncelleme öncesi art arda doğru cevap sayısıdır.
    /// </summary>
    public int CurrentConsecutiveCorrectCount { get; init; }

    /// <summary>
    /// Güncelleme öncesi art arda yanlış cevap sayısıdır.
    /// </summary>
    public int CurrentConsecutiveWrongCount { get; init; }

    /// <summary>
    /// Güncelleme öncesi repetition level değeridir.
    /// </summary>
    public int CurrentRepetitionLevel { get; init; }

    /// <summary>
    /// Güncelleme öncesi next review date değeridir.
    /// </summary>
    public DateTimeOffset? CurrentNextReviewDate { get; init; }

    /// <summary>
    /// Cevap doğru mu?
    /// Bu bilgi QuizAnswerEvaluationResult üzerinden gelir.
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// LearningScoreCalculator tarafından hesaplanan score sonucudur.
    /// </summary>
    public LearningScoreCalculationResult ScoreCalculationResult { get; init; } = default!;


    /// <summary>
    /// ReviewScheduleCalculator tarafından hesaplanan tekrar planlama sonucudur.
    /// 
    /// LearningProgressUpdater bu sonucu kullanarak NextReviewDate değerini belirler.
    /// Böylece review schedule hesaplama mantığı progress updater içine gömülmez.
    /// </summary>
    public ReviewScheduleEvent ReviewScheduleEvent { get; init; } = default!;

    /// <summary>
    /// Güncellemenin yapılacağı zaman.
    /// Handler bunu DateTimeOffset.UtcNow gibi merkezi bir değerle verebilir.
    /// </summary>
    public DateTimeOffset ReviewedAt { get; init; }
}