namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz cevabını değerlendirmek için QuizAnswerEvaluator'a gönderilecek request modelidir.
/// 
/// Bu model entity değildir.
/// Handler, repository'den aldığı QuizOption ve QuizQuestion verilerini bu modele map eder.
/// Evaluator ise sadece bu modele bakarak doğru/yanlış sonucunu üretir.
/// </summary>
public sealed class QuizAnswerEvaluationRequest
{
    /// <summary>
    /// Cevap verilen quiz session id değeridir.
    /// </summary>
    public Guid QuizSessionId { get; init; }

    /// <summary>
    /// Cevap verilen quiz question id değeridir.
    /// </summary>
    public Guid QuizQuestionId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği quiz option id değeridir.
    /// </summary>
    public Guid SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği option text değeridir.
    /// 
    /// Örnek:
    /// başarmak
    /// </summary>
    public string SelectedOptionText { get; init; } = string.Empty;

    /// <summary>
    /// Seçilen option doğru cevap mı?
    /// 
    /// Bu değer QuizOption.IsCorrect alanından gelir.
    /// Doğru/yanlış kararını client değil backend verir.
    /// </summary>
    public bool SelectedOptionIsCorrect { get; init; }

    /// <summary>
    /// QuizQuestion üzerinde tutulan doğru cevap metnidir.
    /// 
    /// Örnek:
    /// başarmak
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// Sadece bu QuizQuestion için ölçülen cevap süresidir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }
}