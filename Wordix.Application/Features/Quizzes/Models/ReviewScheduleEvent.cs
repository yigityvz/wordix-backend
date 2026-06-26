namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// ReviewScheduleCalculator tarafından üretilen tekrar planlama sonucudur.
/// 
/// Bu model neden "Event" olarak adlandırıldı?
/// - Çünkü quiz cevabı sonrası "bir sonraki tekrar zamanı belirlendi" olayını temsil eder.
/// - Şu an database'e ayrı event tablosu olarak yazmıyoruz.
/// - Handler bu modeldeki NextReviewDate değerini UserLearningProgress'e uygulayacak.
/// - İleride gerçek ReviewScheduleEvent entity/tablosu eklemek istersek bu model bize temel olur.
/// </summary>
public sealed class ReviewScheduleEvent
{
    /// <summary>
    /// Cevabın değerlendirildiği zaman.
    /// </summary>
    public DateTimeOffset ReviewedAt { get; init; }

    /// <summary>
    /// Bir sonraki tekrar tarihi.
    /// </summary>
    public DateTimeOffset NextReviewDate { get; init; }

    /// <summary>
    /// Kaç gün sonra tekrar planlandığını gösterir.
    /// </summary>
    public int ReviewIntervalDays { get; init; }

    /// <summary>
    /// Cevap doğru muydu?
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Yeni confidence score değeridir.
    /// </summary>
    public int NewConfidenceScore { get; init; }

    /// <summary>
    /// Review schedule kararının kısa açıklamasıdır.
    /// 
    /// Örnek:
    /// Correct answer with medium confidence. Next review scheduled in 4 days.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}