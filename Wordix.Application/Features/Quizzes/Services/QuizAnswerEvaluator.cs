using System.Text;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabını değerlendiren servis implementation'ıdır.
/// 
/// Faz 21 itibarıyla iki cevap türünü destekler:
/// 
/// Test quiz:
/// - Kullanıcının seçtiği option'ın IsCorrect değeri true ise cevap Correct kabul edilir.
/// 
/// Writing quiz:
/// - Kullanıcının yazdığı cevap CorrectAnswerText ile normalize edilerek karşılaştırılır.
/// - Büyük/küçük harf farkı yok sayılır.
/// - Noktalama farkı yok sayılır.
/// - Fazla boşluklar yok sayılır.
/// - Eksik kelime, yanlış kelime, devrik cümle veya kısmi cevap kabul edilmez.
/// 
/// Bu class ne yapmaz?
/// - DbContext kullanmaz.
/// - Repository kullanmaz.
/// - QuizAnswer entity oluşturmaz.
/// - UserLearningProgress güncellemez.
/// - LearningProgressHistory oluşturmaz.
/// - AI/NLP/semantic similarity yapmaz.
/// </summary>
public sealed class QuizAnswerEvaluator : IQuizAnswerEvaluator
{
    public QuizAnswerEvaluationResult Evaluate(
        QuizAnswerEvaluationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateCommonRequest(request);

        return request.QuizType switch
        {
            QuizType.Test => EvaluateMultipleChoiceAnswer(request),
            QuizType.Writing => EvaluateWrittenAnswer(request),

            _ => throw new BusinessRuleException(
                "Quiz type is not supported for answer evaluation.",
                "QUIZ_TYPE_NOT_SUPPORTED_FOR_EVALUATION")
        };
    }

    /// <summary>
    /// Test quiz cevabını değerlendirir.
    /// </summary>
    private static QuizAnswerEvaluationResult EvaluateMultipleChoiceAnswer(
        QuizAnswerEvaluationRequest request)
    {
        if (!request.SelectedQuizOptionId.HasValue || request.SelectedQuizOptionId.Value == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Selected quiz option id is required for test quiz answer evaluation.",
                "SELECTED_QUIZ_OPTION_ID_REQUIRED_FOR_TEST_EVALUATION");
        }

        if (string.IsNullOrWhiteSpace(request.SelectedOptionText))
        {
            throw new BusinessRuleException(
                "Selected option text is required for test quiz answer evaluation.",
                "SELECTED_OPTION_TEXT_REQUIRED_FOR_TEST_EVALUATION");
        }

        if (!request.SelectedOptionIsCorrect.HasValue)
        {
            throw new BusinessRuleException(
                "Selected option correctness is required for test quiz answer evaluation.",
                "SELECTED_OPTION_CORRECTNESS_REQUIRED_FOR_TEST_EVALUATION");
        }

        var answerResult = request.SelectedOptionIsCorrect.Value
            ? AnswerResult.Correct
            : AnswerResult.Incorrect;

        return new QuizAnswerEvaluationResult
        {
            QuizSessionId = request.QuizSessionId,
            QuizQuestionId = request.QuizQuestionId,
            SelectedQuizOptionId = request.SelectedQuizOptionId,
            SelectedOptionText = request.SelectedOptionText.Trim(),
            UserAnswerText = string.Empty,
            CorrectAnswerText = request.CorrectAnswerText.Trim(),
            AnswerResult = answerResult,
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds
        };
    }

    /// <summary>
    /// Writing quiz cevabını değerlendirir.
    /// 
    /// Karar:
    /// - Kısmi doğru cevabı kabul etmiyoruz.
    /// - Kullanıcının öğrenmesi için tam cevap bekliyoruz.
    /// - Sadece casing, noktalama ve fazla boşluk gibi biçimsel farkları tolere ediyoruz.
    /// </summary>
    private static QuizAnswerEvaluationResult EvaluateWrittenAnswer(
        QuizAnswerEvaluationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserAnswerText))
        {
            throw new BusinessRuleException(
                "User answer is required for writing quiz answer evaluation.",
                "USER_ANSWER_REQUIRED_FOR_WRITING_EVALUATION");
        }

        var normalizedUserAnswer = NormalizeAnswerText(request.UserAnswerText);
        var normalizedCorrectAnswer = NormalizeAnswerText(request.CorrectAnswerText);

        var answerResult = ResolveWrittenAnswerResult(
            normalizedUserAnswer,
            normalizedCorrectAnswer);

        return new QuizAnswerEvaluationResult
        {
            QuizSessionId = request.QuizSessionId,
            QuizQuestionId = request.QuizQuestionId,
            SelectedQuizOptionId = null,
            SelectedOptionText = string.Empty,
            UserAnswerText = request.UserAnswerText.Trim(),
            CorrectAnswerText = request.CorrectAnswerText.Trim(),
            AnswerResult = answerResult,
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds
        };
    }

    /// <summary>
    /// Ortak evaluation validation.
    /// </summary>
    private static void ValidateCommonRequest(
        QuizAnswerEvaluationRequest request)
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

        if (string.IsNullOrWhiteSpace(request.CorrectAnswerText))
        {
            throw new BusinessRuleException(
                "Correct answer text is required for answer evaluation.",
                "CORRECT_ANSWER_TEXT_REQUIRED_FOR_EVALUATION");
        }
    }

    /// <summary>
    /// Writing cevabının sonucunu belirler.
    /// 
    /// Burada bilinçli olarak partial correct üretmiyoruz.
    /// Çünkü writing quiz öğrenme amacıyla tam yazım/çeviri pratiği ölçer.
    /// Eksik veya kısmi cevap kullanıcıyı yanıltmamak için Incorrect kabul edilir.
    /// </summary>
    private static AnswerResult ResolveWrittenAnswerResult(
        string normalizedUserAnswer,
        string normalizedCorrectAnswer)
    {
        if (string.IsNullOrWhiteSpace(normalizedUserAnswer))
        {
            return AnswerResult.Skipped;
        }

        return string.Equals(
            normalizedUserAnswer,
            normalizedCorrectAnswer,
            StringComparison.Ordinal)
            ? AnswerResult.Correct
            : AnswerResult.Incorrect;
    }

    /// <summary>
    /// Cevap metinlerini deterministic şekilde normalize eder.
    /// 
    /// Bu normalize işlemi:
    /// - trim yapar,
    /// - küçük harfe çevirir,
    /// - noktalama karakterlerini boşluk kabul eder,
    /// - fazla boşlukları teke indirir.
    /// 
    /// Bilinçli olarak kelime benzerliği, harf hatası veya eş anlamlı kontrolü yapmayız.
    /// Çünkü bu fazda kullanıcıdan tam doğru yazım/çeviri bekliyoruz.
    /// </summary>
    private static string NormalizeAnswerText(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            builder.Append(char.IsPunctuation(character)
                ? ' '
                : character);
        }

        return string.Join(
            " ",
            builder
                .ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}