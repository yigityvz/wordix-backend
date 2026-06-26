using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// SubmitQuizAnswerCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Route'tan gelen QuizSessionId değerinin boş olup olmadığını kontrol eder.
/// - Body'den gelen SelectedQuizOptionId değerinin boş olup olmadığını kontrol eder.
/// - QuestionResponseTimeInMilliseconds değeri gönderilmişse makul aralıkta mı kontrol eder.
/// 
/// Bu validator ne yapmaz?
/// - QuizSession gerçekten var mı kontrol etmez.
/// - QuizSession current user'a ait mi kontrol etmez.
/// - QuizOption gerçekten var mı kontrol etmez.
/// - QuizOption bu session'daki bir question'a mı ait kontrol etmez.
/// - Cevap doğru mu yanlış mı hesaplamaz.
/// - Progress güncellemez.
/// 
/// Bunlar business/data kontrolleridir ve handler + servisler tarafında yapılacaktır.
/// </summary>
public sealed class SubmitQuizAnswerCommandValidator
    : AbstractValidator<SubmitQuizAnswerCommand>
{
    /// <summary>
    /// Minimum soru cevaplama süresi.
    /// 
    /// 0 veya negatif süre mantıklı değildir.
    /// Ancak alan nullable olduğu için kullanıcı hiç süre göndermeyebilir.
    /// </summary>
    private const int MinimumQuestionResponseTimeInMilliseconds = 1;

    /// <summary>
    /// Maksimum soru cevaplama süresi.
    /// 
    /// İlk prototipte tek soru için 10 dakika üstünü makul kabul etmiyoruz.
    /// 10 dakika = 600.000 ms
    /// 
    /// Bu süre quiz geneli değil, tek bir soru içindir.
    /// İleride quiz tipine veya soru zorluğuna göre değiştirilebilir.
    /// </summary>
    private const int MaximumQuestionResponseTimeInMilliseconds = 600_000;

    /// <summary>
    /// Validator kuralları constructor içinde tanımlanır.
    /// </summary>
    public SubmitQuizAnswerCommandValidator()
    {
        RuleFor(command => command.QuizSessionId)
            .NotEmpty()
            .WithMessage("Quiz session id is required.")
            .WithErrorCode("QUIZ_SESSION_ID_REQUIRED");

        RuleFor(command => command.SelectedQuizOptionId)
            .NotEmpty()
            .WithMessage("Selected quiz option id is required.")
            .WithErrorCode("SELECTED_QUIZ_OPTION_ID_REQUIRED");

        RuleFor(command => command.QuestionResponseTimeInMilliseconds)
            .Must(BeNullOrInAllowedRange)
            .WithMessage(
                $"Question response time must be between {MinimumQuestionResponseTimeInMilliseconds} and {MaximumQuestionResponseTimeInMilliseconds} milliseconds when provided.")
            .WithErrorCode("QUESTION_RESPONSE_TIME_OUT_OF_RANGE");
    }

    /// <summary>
    /// QuestionResponseTimeInMilliseconds alanı nullable olduğu için özel kontrol yapıyoruz.
    /// 
    /// Kurallar:
    /// - null olabilir.
    /// - null değilse 1 ile 600000 ms arasında olmalıdır.
    /// </summary>
    private static bool BeNullOrInAllowedRange(int? questionResponseTimeInMilliseconds)
    {
        if (questionResponseTimeInMilliseconds is null)
        {
            return true;
        }

        return questionResponseTimeInMilliseconds.Value is >= MinimumQuestionResponseTimeInMilliseconds
            and <= MaximumQuestionResponseTimeInMilliseconds;
    }
}