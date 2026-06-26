using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// StartQuizCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - QuizType değerini kontrol eder.
/// - QuizSourceType değerini kontrol eder.
/// - QuizContentMode değerini kontrol eder.
/// - QuestionCount değerinin izin verilen aralıkta olup olmadığını kontrol eder.
/// 
/// İlk prototipte sadece şu quiz kombinasyonunu destekliyoruz:
/// - QuizType: Test
/// - QuizSourceType: Dictionary
/// - QuizContentMode: WordsOnly
/// 
/// ValidationBehavior bu validator'ı handler'dan önce otomatik çalıştırır.
/// Hata varsa StartQuizCommandHandler'a hiç gidilmez.
/// ExceptionMiddleware standart 400 VALIDATION_ERROR response'u döner.
/// </summary>
public sealed class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
{
    /// <summary>
    /// İlk prototipte desteklenen quiz türü.
    /// </summary>
    private const string SupportedQuizType = "Test";

    /// <summary>
    /// İlk prototipte desteklenen quiz kaynağı.
    /// Kullanıcının kendi dictionary'sinden soru üretilecek.
    /// </summary>
    private const string SupportedQuizSourceType = "Dictionary";

    /// <summary>
    /// İlk prototipte desteklenen içerik modu.
    /// Sadece Word tabanlı sorular üretilecek.
    /// </summary>
    private const string SupportedQuizContentMode = "WordsOnly";

    /// <summary>
    /// Minimum soru sayısı.
    /// </summary>
    private const int MinimumQuestionCount = 1;

    /// <summary>
    /// Maksimum soru sayısı.
    /// İlk prototipte çok büyük quiz oluşturmayı engelliyoruz.
    /// </summary>
    private const int MaximumQuestionCount = 20;

    /// <summary>
    /// Validator kuralları constructor içinde tanımlanır.
    /// </summary>
    public StartQuizCommandValidator()
    {
        RuleFor(command => command.QuizType)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz type is required.")
            .WithErrorCode("QUIZ_TYPE_REQUIRED")
            .Must(value => IsEqualIgnoreCase(value, SupportedQuizType))
            .WithMessage($"Only '{SupportedQuizType}' quiz type is supported in the first prototype.")
            .WithErrorCode("QUIZ_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizSourceType)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz source type is required.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_REQUIRED")
            .Must(value => IsEqualIgnoreCase(value, SupportedQuizSourceType))
            .WithMessage($"Only '{SupportedQuizSourceType}' quiz source type is supported in the first prototype.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizContentMode)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz content mode is required.")
            .WithErrorCode("QUIZ_CONTENT_MODE_REQUIRED")
            .Must(value => IsEqualIgnoreCase(value, SupportedQuizContentMode))
            .WithMessage($"Only '{SupportedQuizContentMode}' quiz content mode is supported in the first prototype.")
            .WithErrorCode("QUIZ_CONTENT_MODE_NOT_SUPPORTED");

        RuleFor(command => command.QuestionCount)
            .InclusiveBetween(MinimumQuestionCount, MaximumQuestionCount)
            .WithMessage($"Question count must be between {MinimumQuestionCount} and {MaximumQuestionCount}.")
            .WithErrorCode("QUESTION_COUNT_OUT_OF_RANGE");
    }

    /// <summary>
    /// String değerleri trim ederek case-insensitive karşılaştırır.
    /// 
    /// Neden ayrı method?
    /// - Kullanıcı "test", "TEST", " Test " gibi değerler gönderirse
    ///   bunları aynı kabul etmek istiyoruz.
    /// - Karşılaştırma kuralı tek yerde dursun istiyoruz.
    /// </summary>
    private static bool IsEqualIgnoreCase(string? value, string expectedValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return string.Equals(
            value.Trim(),
            expectedValue,
            StringComparison.OrdinalIgnoreCase);
    }
}