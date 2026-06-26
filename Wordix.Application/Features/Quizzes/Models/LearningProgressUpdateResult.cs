using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// LearningProgressUpdater tarafından üretilen yeni progress state sonucudur.
/// 
/// Bu model UserLearningProgress entity'sini doğrudan değiştirmez.
/// Handler bu sonucu alıp mevcut entity methodları üzerinden uygulayacaktır.
/// </summary>
public sealed class LearningProgressUpdateResult
{
    /// <summary>
    /// Güncellenen UserLearningProgress id değeridir.
    /// </summary>
    public Guid UserLearningProgressId { get; init; }

    /// <summary>
    /// Progress'in bağlı olduğu UserLearningItem id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Güncelleme öncesi learning status.
    /// LearningProgressHistory için kullanılacak.
    /// </summary>
    public LearningStatus PreviousLearningStatus { get; init; }

    /// <summary>
    /// Güncelleme sonrası learning status.
    /// </summary>
    public LearningStatus NewLearningStatus { get; init; }

    /// <summary>
    /// Güncelleme öncesi confidence score.
    /// LearningProgressHistory için kullanılacak.
    /// </summary>
    public int PreviousConfidenceScore { get; init; }

    /// <summary>
    /// Güncelleme sonrası confidence score.
    /// </summary>
    public int NewConfidenceScore { get; init; }

    /// <summary>
    /// Güncelleme sonrası doğru cevap sayısı.
    /// </summary>
    public int CorrectCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası yanlış cevap sayısı.
    /// </summary>
    public int WrongCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası art arda doğru cevap sayısı.
    /// </summary>
    public int ConsecutiveCorrectCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası art arda yanlış cevap sayısı.
    /// </summary>
    public int ConsecutiveWrongCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası repetition level.
    /// İlk basit kural:
    /// - Doğru cevapta 1 artar.
    /// - Yanlış cevapta 0'a düşer.
    /// </summary>
    public int RepetitionLevel { get; init; }

    /// <summary>
    /// Son tekrar/çalışma zamanı.
    /// </summary>
    public DateTimeOffset LastReviewedAt { get; init; }

    /// <summary>
    /// Bir sonraki tekrar tarihi.
    /// Faz 16F'te ReviewScheduleCalculator ile daha sistemli hale getireceğiz.
    /// Bu adımda basit bir ilk değer üretiyoruz.
    /// </summary>
    public DateTimeOffset? NextReviewDate { get; init; }


    /// <summary>
    /// Review schedule kararında kaç gün sonra tekrar planlandığıdır.
    /// </summary>
    public int ReviewIntervalDays { get; init; }

    /// <summary>
    /// Review schedule kararının açıklamasıdır.
    /// </summary>
    public string ReviewScheduleReason { get; init; } = string.Empty;

    /// <summary>
    /// Progress değişim sebebi.
    /// LearningProgressHistory.ChangeReason için kullanılabilir.
    /// </summary>
    public string ChangeReason { get; init; } = string.Empty;
}