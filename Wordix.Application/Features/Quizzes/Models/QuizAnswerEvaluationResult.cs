namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// QuizAnswerEvaluator tarafından üretilen değerlendirme sonucudur.
/// 
/// Bu model:
/// - cevabın doğru/yanlış sonucunu,
/// - seçilen option bilgisini,
/// - doğru cevap bilgisini,
/// - soru cevaplama süresini
/// taşır.
/// </summary>
public sealed class QuizAnswerEvaluationResult
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
    /// Cevap doğru mu?
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği option text değeridir.
    /// </summary>
    public string SelectedOptionText { get; init; } = string.Empty;

    /// <summary>
    /// Doğru cevap metnidir.
    /// 
    /// Cevap gönderildikten sonra frontend'e gösterilebilir.
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }
}