using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// QuizAnswerEvaluator tarafından üretilen değerlendirme sonucudur.
/// 
/// Bu model:
/// - cevabın sonucunu,
/// - kullanıcının gönderdiği cevabı,
/// - doğru cevap bilgisini,
/// - soru cevaplama süresini
/// taşır.
/// </summary>
public sealed class QuizAnswerEvaluationResult
{
    public Guid QuizSessionId { get; init; }

    public Guid QuizQuestionId { get; init; }

    public Guid? SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği option text değeridir.
    /// Test quiz için doludur.
    /// </summary>
    public string SelectedOptionText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının yazdığı cevap metnidir.
    /// Writing quiz için doludur.
    /// </summary>
    public string UserAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının verdiği cevabın frontend'e gösterilecek ortak metnidir.
    /// 
    /// Test quizde selected option text,
    /// Writing quizde user answer text döner.
    /// </summary>
    public string SubmittedAnswerText =>
        !string.IsNullOrWhiteSpace(UserAnswerText)
            ? UserAnswerText
            : SelectedOptionText;

    public string CorrectAnswerText { get; init; } = string.Empty;

    public AnswerResult AnswerResult { get; init; }

    /// <summary>
    /// Cevap tam doğru mu?
    /// 
    /// Writing quizde kısmi doğru kabul etmiyoruz.
    /// Bu yüzden sadece AnswerResult.Correct true döner.
    /// </summary>
    public bool IsCorrect =>
        AnswerResult == AnswerResult.Correct;

    public bool IsPartiallyCorrect =>
        AnswerResult == AnswerResult.PartiallyCorrect;

    public int? QuestionResponseTimeInMilliseconds { get; init; }
}