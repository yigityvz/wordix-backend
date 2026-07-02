using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// SubmitQuizAnswerCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Route'tan gelen QuizSessionId değerinin boş olup olmadığını kontrol eder.
/// - Kullanıcının en az bir cevap payload'ı gönderip göndermediğini kontrol eder.
/// - QuestionResponseTimeInMilliseconds değeri gönderilmişse makul aralıkta mı kontrol eder.
/// 
/// Bu validator ne yapmaz?
/// - QuizSession gerçekten var mı kontrol etmez.
/// - QuizSession current user'a ait mi kontrol etmez.
/// - Quiz'in Test mi Writing mi olduğunu database'den okumaz.
/// - SelectedQuizOptionId bu session'a ait mi kontrol etmez.
/// - UserAnswer doğru mu yanlış mı hesaplamaz.
/// - Progress güncellemez.
/// 
/// Bu kontroller handler + servisler tarafında yapılır.
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
    /// </summary>
    private const int MaximumQuestionResponseTimeInMilliseconds = 600_000;

    /// <summary>
    /// Writing cevap için maksimum karakter sınırı.
    /// 
    /// Şimdilik 1000 karakter yeterli.
    /// Sentence translation cevapları için de makul bir üst limittir.
    /// İleride ayar haline getirilebilir.
    /// </summary>
    private const int MaximumUserAnswerLength = 1_000;

    public SubmitQuizAnswerCommandValidator()
    {
        RuleFor(command => command.QuizSessionId)
            .NotEmpty()
            .WithMessage("Quiz session id is required.")
            .WithErrorCode("QUIZ_SESSION_ID_REQUIRED");

        RuleFor(command => command)
            .Must(ContainAnyAnswerPayload)
            .WithMessage("Either selected quiz option id or user answer is required.")
            .WithErrorCode("QUIZ_ANSWER_PAYLOAD_REQUIRED");

        RuleFor(command => command.QuizQuestionId)
            .NotEmpty()
            .When(command =>
                (!command.SelectedQuizOptionId.HasValue || command.SelectedQuizOptionId.Value == Guid.Empty)
                && !string.IsNullOrWhiteSpace(command.UserAnswer))
            .WithMessage("Quiz question id is required when answering with text.")
            .WithErrorCode("QUIZ_QUESTION_ID_REQUIRED_FOR_WRITING_ANSWER");

        RuleFor(command => command.UserAnswer)
            .MaximumLength(MaximumUserAnswerLength)
            .WithMessage($"User answer cannot exceed {MaximumUserAnswerLength} characters.")
            .WithErrorCode("USER_ANSWER_TOO_LONG");

        RuleFor(command => command.QuestionResponseTimeInMilliseconds)
            .Must(BeNullOrInAllowedRange)
            .WithMessage(
                $"Question response time must be between {MinimumQuestionResponseTimeInMilliseconds} and {MaximumQuestionResponseTimeInMilliseconds} milliseconds when provided.")
            .WithErrorCode("QUESTION_RESPONSE_TIME_OUT_OF_RANGE");
    }

    /// <summary>
    /// Kullanıcı en az bir cevap türü göndermelidir.
    /// 
    /// Test quiz için:
    /// - SelectedQuizOptionId beklenir.
    /// 
    /// Writing quiz için:
    /// - UserAnswer beklenir.
    /// 
    /// Hangi quiz tipinde hangisinin zorunlu olduğunu handler kontrol eder.
    /// Validator burada sadece boş request'i engeller.
    /// </summary>
    private static bool ContainAnyAnswerPayload(
        SubmitQuizAnswerCommand command)
    {
        var hasSelectedOption = command.SelectedQuizOptionId.HasValue
            && command.SelectedQuizOptionId.Value != Guid.Empty;

        var hasUserAnswer = !string.IsNullOrWhiteSpace(command.UserAnswer);


        return hasSelectedOption || hasUserAnswer;
    }

    /// <summary>
    /// QuestionResponseTimeInMilliseconds alanı nullable olduğu için özel kontrol yapıyoruz.
    /// 
    /// Kurallar:
    /// - null olabilir.
    /// - null değilse 1 ile 600000 ms arasında olmalıdır.
    /// </summary>
    private static bool BeNullOrInAllowedRange(
        int? questionResponseTimeInMilliseconds)
    {
        if (questionResponseTimeInMilliseconds is null)
        {
            return true;
        }

        return questionResponseTimeInMilliseconds.Value is >= MinimumQuestionResponseTimeInMilliseconds
            and <= MaximumQuestionResponseTimeInMilliseconds;
    }
}