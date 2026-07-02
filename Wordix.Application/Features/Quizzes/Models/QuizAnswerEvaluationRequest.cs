using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz cevabı değerlendirme servisine gönderilen request modelidir.
/// 
/// Test quiz:
/// - SelectedQuizOptionId
/// - SelectedOptionText
/// - SelectedOptionIsCorrect
/// 
/// Writing quiz:
/// - UserAnswerText
/// - CorrectAnswerText
/// 
/// Bu model entity değildir.
/// Sadece evaluation sürecinde kullanılan application modelidir.
/// </summary>
public sealed class QuizAnswerEvaluationRequest
{
    public Guid QuizSessionId { get; init; }

    public Guid QuizQuestionId { get; init; }

    /// <summary>
    /// Quiz session tipi.
    /// Test veya Writing olabilir.
    /// </summary>
    public QuizType QuizType { get; init; }

    /// <summary>
    /// Tekil soru tipi.
    /// MultipleChoice veya TranslateToTargetLanguage olabilir.
    /// </summary>
    public QuestionType QuestionType { get; init; }

    public Guid? SelectedQuizOptionId { get; init; }

    public string? SelectedOptionText { get; init; }

    public bool? SelectedOptionIsCorrect { get; init; }

    public string? UserAnswerText { get; init; }

    public string CorrectAnswerText { get; init; } = string.Empty;

    public int? QuestionResponseTimeInMilliseconds { get; init; }
}