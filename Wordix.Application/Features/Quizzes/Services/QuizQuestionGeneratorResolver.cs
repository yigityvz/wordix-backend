using Wordix.Application.Common.Exceptions;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// QuizType değerine göre doğru question generator implementation'ını seçer.
/// 
/// Faz 21 itibarıyla:
/// - Test quiz için MultipleChoiceTranslationQuestionGenerator
/// - Writing quiz için WrittenTranslationQuestionGenerator
/// 
/// Mixed QuizType şimdilik aktif desteklenmez.
/// İleride Test + Writing karışık oturum istenirse ayrı tasarlanacaktır.
/// </summary>
public sealed class QuizQuestionGeneratorResolver
    : IQuizQuestionGeneratorResolver
{
    private readonly MultipleChoiceTranslationQuestionGenerator _multipleChoiceGenerator;
    private readonly WrittenTranslationQuestionGenerator _writtenTranslationGenerator;

    public QuizQuestionGeneratorResolver(
        MultipleChoiceTranslationQuestionGenerator multipleChoiceGenerator,
        WrittenTranslationQuestionGenerator writtenTranslationGenerator)
    {
        _multipleChoiceGenerator = multipleChoiceGenerator;
        _writtenTranslationGenerator = writtenTranslationGenerator;
    }

    /// <summary>
    /// QuizType değerine göre uygun generator'ı döner.
    /// </summary>
    public IQuizQuestionGenerator Resolve(
        QuizType quizType)
    {
        return quizType switch
        {
            QuizType.Test => _multipleChoiceGenerator,
            QuizType.Writing => _writtenTranslationGenerator,

            QuizType.Mixed => throw new BusinessRuleException(
                "Mixed quiz type is not supported yet.",
                "QUIZ_TYPE_NOT_SUPPORTED"),

            _ => throw new BusinessRuleException(
                "Quiz type is not supported.",
                "QUIZ_TYPE_NOT_SUPPORTED")
        };
    }
}