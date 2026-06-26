using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Queries.GetQuizSummary;

/// <summary>
/// GetQuizSummaryQuery için validator sınıfıdır.
/// 
/// Sadece input formatını doğrular.
/// QuizSession gerçekten var mı, current user'a ait mi gibi kontroller handler'da yapılır.
/// </summary>
public sealed class GetQuizSummaryQueryValidator
    : AbstractValidator<GetQuizSummaryQuery>
{
    public GetQuizSummaryQueryValidator()
    {
        RuleFor(query => query.QuizSessionId)
            .NotEmpty()
            .WithMessage("Quiz session id is required.")
            .WithErrorCode("QUIZ_SESSION_ID_REQUIRED");
    }
}