namespace Wordix.Application.Features.Quizzes.Dtos.Responses;

/// <summary>
/// Bir quiz session'ın özet sonucunu temsil eder.
/// 
/// Bu response:
/// - quiz genel bilgilerini,
/// - kaç soru cevaplandığını,
/// - kaç doğru/yanlış olduğunu,
/// - başarı oranını,
/// - soru bazlı cevap sürelerini,
/// - her soru için özet cevabı
/// frontend'e döner.
/// </summary>
public sealed class QuizSummaryResponse
{
    /// <summary>
    /// Quiz session id değeridir.
    /// </summary>
    public Guid QuizSessionId { get; init; }

    /// <summary>
    /// Quiz type bilgisidir.
    /// </summary>
    public string QuizType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz source type bilgisidir.
    /// </summary>
    public string QuizSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz content mode bilgisidir.
    /// </summary>
    public string QuizContentMode { get; init; } = string.Empty;

    /// <summary>
    /// Quiz session status bilgisidir.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Quiz'in başladığı zamandır.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Quiz'deki toplam soru sayısıdır.
    /// </summary>
    public int TotalQuestionCount { get; init; }

    /// <summary>
    /// Cevaplanan soru sayısıdır.
    /// </summary>
    public int AnsweredQuestionCount { get; init; }

    /// <summary>
    /// Henüz cevaplanmamış soru sayısıdır.
    /// </summary>
    public int UnansweredQuestionCount { get; init; }

    /// <summary>
    /// Doğru cevap sayısıdır.
    /// </summary>
    public int CorrectAnswerCount { get; init; }

    /// <summary>
    /// Yanlış cevap sayısıdır.
    /// </summary>
    public int WrongAnswerCount { get; init; }

    /// <summary>
    /// Cevaplanan sorular üzerinden başarı oranıdır.
    /// 
    /// Örnek:
    /// 75.0
    /// </summary>
    public double AccuracyRate { get; init; }

    /// <summary>
    /// Quiz tamamlanma oranıdır.
    /// 
    /// Örnek:
    /// 4 sorudan 2'si cevaplandıysa 50.0
    /// </summary>
    public double CompletionRate { get; init; }

    /// <summary>
    /// Cevaplanan soruların ortalama soru bazlı cevap süresidir.
    /// 
    /// Response time ölçülmemişse null döner.
    /// </summary>
    public int? AverageQuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// En hızlı cevaplama süresidir.
    /// </summary>
    public int? FastestQuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// En yavaş cevaplama süresidir.
    /// </summary>
    public int? SlowestQuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Soru bazlı summary listesi.
    /// </summary>
    public IReadOnlyCollection<QuizSummaryQuestionResponse> Questions { get; init; }
        = Array.Empty<QuizSummaryQuestionResponse>();
}
