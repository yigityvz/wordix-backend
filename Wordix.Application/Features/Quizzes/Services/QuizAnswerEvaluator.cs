using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabını değerlendiren servis implementation'ıdır.
/// 
/// İlk prototipte değerlendirme mantığı basittir:
/// - Kullanıcının seçtiği QuizOption'ın IsCorrect değeri true ise cevap doğrudur.
/// - false ise cevap yanlıştır.
/// 
/// Bu class ne yapmaz?
/// - DbContext kullanmaz.
/// - Repository kullanmaz.
/// - QuizAnswer entity oluşturmaz.
/// - UserLearningProgress güncellemez.
/// - LearningProgressHistory oluşturmaz.
/// 
/// Sadece doğru/yanlış değerlendirme sonucunu üretir.
/// </summary>
public sealed class QuizAnswerEvaluator : IQuizAnswerEvaluator
{
    /// <summary>
    /// Cevabı değerlendirir ve sonucu döner.
    /// </summary>
    public QuizAnswerEvaluationResult Evaluate(QuizAnswerEvaluationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        return new QuizAnswerEvaluationResult
        {
            QuizSessionId = request.QuizSessionId,
            QuizQuestionId = request.QuizQuestionId,
            SelectedQuizOptionId = request.SelectedQuizOptionId,
            IsCorrect = request.SelectedOptionIsCorrect,
            SelectedOptionText = request.SelectedOptionText.Trim(),
            CorrectAnswerText = request.CorrectAnswerText.Trim(),
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds
        };
    }

    /// <summary>
    /// Evaluator seviyesindeki defensive validation'dır.
    /// 
    /// Not:
    /// SubmitQuizAnswerCommandValidator zaten temel input kontrolü yapıyor.
    /// Ancak evaluator doğrudan testlerde veya farklı handler'lardan çağrılabileceği için
    /// burada da minimum güvenlik kontrolü bırakıyoruz.
    /// </summary>
    private static void ValidateRequest(QuizAnswerEvaluationRequest request)
    {
        if (request.QuizSessionId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Quiz session id is required for answer evaluation.",
                "QUIZ_SESSION_ID_REQUIRED_FOR_EVALUATION");
        }

        if (request.QuizQuestionId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Quiz question id is required for answer evaluation.",
                "QUIZ_QUESTION_ID_REQUIRED_FOR_EVALUATION");
        }

        if (request.SelectedQuizOptionId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Selected quiz option id is required for answer evaluation.",
                "SELECTED_QUIZ_OPTION_ID_REQUIRED_FOR_EVALUATION");
        }

        if (string.IsNullOrWhiteSpace(request.SelectedOptionText))
        {
            throw new BusinessRuleException(
                "Selected option text is required for answer evaluation.",
                "SELECTED_OPTION_TEXT_REQUIRED_FOR_EVALUATION");
        }

        if (string.IsNullOrWhiteSpace(request.CorrectAnswerText))
        {
            throw new BusinessRuleException(
                "Correct answer text is required for answer evaluation.",
                "CORRECT_ANSWER_TEXT_REQUIRED_FOR_EVALUATION");
        }
    }
}