using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Commands.SaveRecommendedItemToDictionary;

/// <summary>
/// SaveRecommendedItemToDictionaryCommand için validation kurallarıdır.
/// 
/// Validator sadece request formatını kontrol eder.
/// Recommendation item gerçekten var mı, current user'a ait mi gibi kontroller handler içinde yapılır.
/// </summary>
public sealed class SaveRecommendedItemToDictionaryCommandValidator
    : AbstractValidator<SaveRecommendedItemToDictionaryCommand>
{
    public SaveRecommendedItemToDictionaryCommandValidator()
    {
        RuleFor(command => command.QuizRecommendationItemId)
            .NotEmpty()
            .WithMessage("Quiz recommendation item id is required.")
            .WithErrorCode("QUIZ_RECOMMENDATION_ITEM_ID_REQUIRED");
    }
}