namespace Wordix.Application.Features.Quizzes.Responses;

/// <summary>
/// Quiz summary içinde tek bir sorunun özet sonucunu temsil eder.
/// 
/// Bu response frontend'e şunu söyleyebilir:
/// - Bu soru cevaplandı mı?
/// - Kullanıcının cevabı neydi?
/// - Cevap doğru muydu?
/// - Doğru cevap neydi?
/// - Bu soruya kaç milisaniyede cevap verildi?
/// </summary>
public sealed class QuizSummaryQuestionResponse
{
    /// <summary>
    /// QuizQuestion id değeridir.
    /// </summary>
    public Guid QuizQuestionId { get; init; }

    /// <summary>
    /// Sorunun quiz içindeki sırasıdır.
    /// </summary>
    public int QuestionOrder { get; init; }

    /// <summary>
    /// Soru metnidir.
    /// 
    /// Örnek:
    /// achieve
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Sorunun bağlı olduğu LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Bu soru cevaplandı mı?
    /// </summary>
    public bool IsAnswered { get; init; }

    /// <summary>
    /// Cevap doğru muydu?
    /// 
    /// Soru henüz cevaplanmadıysa null döner.
    /// </summary>
    public bool? IsCorrect { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği option id değeridir.
    /// 
    /// Soru cevaplanmadıysa null döner.
    /// </summary>
    public Guid? SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının verdiği cevap metnidir.
    /// 
    /// Multiple choice için seçilen option text değeridir.
    /// </summary>
    public string? SelectedAnswerText { get; init; }

    /// <summary>
    /// Doğru cevap metnidir.
    /// 
    /// Summary aşamasında doğru cevap dönebilir.
    /// Çünkü quiz cevaplama/senaryo sonrası özet ekranıdır.
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğidir.
    /// 
    /// 0 veya ölçülmemiş değerler null olarak döndürülür.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Cevabın kaydedildiği zamandır.
    /// 
    /// Soru cevaplanmadıysa null döner.
    /// </summary>
    public DateTimeOffset? AnsweredAt { get; init; }
}