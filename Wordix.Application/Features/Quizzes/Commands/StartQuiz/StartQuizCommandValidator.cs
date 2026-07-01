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
/// Faz 18 itibarıyla desteklenen kombinasyon:
/// - QuizType: Test
/// - QuizSourceType: UserDictionary
/// - QuizContentMode: WordsOnly, PhrasesOnly, Mixed
/// 
/// SentencesOnly enumda vardır ama Sentence desteği Faz 19'a bırakıldığı için
/// şu anda validation aşamasında engellenir.
/// 
/// ValidationBehavior bu validator'ı handler'dan önce otomatik çalıştırır.
/// Hata varsa StartQuizCommandHandler'a hiç gidilmez.
/// ExceptionMiddleware standart 400 VALIDATION_ERROR response'u döner.
/// </summary>
public sealed class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
{
    /// <summary>
    /// Faz 18'de desteklenen quiz türü.
    /// </summary>
    private const string SupportedQuizType = "Test";

    /// <summary>
    /// Faz 18'de desteklenen gerçek domain enum kaynak değeri.
    /// 
    /// Domain enum tarafında değer:
    /// QuizSourceType.UserDictionary
    /// </summary>
    private const string SupportedQuizSourceType = "UserDictionary";

    /// <summary>
    /// Eski/prototip dokümantasyonunda kullanılan alias değerdir.
    /// 
    /// Neden tutuyoruz?
    /// - Önceki Swagger örneklerinde "Dictionary" yazıyordu.
    /// - Frontend veya manuel testler hâlâ "Dictionary" gönderebilir.
    /// - Validator bunu kabul edebilir ama handler tarafında da alias parse desteği olmalıdır.
    /// </summary>
    private const string DictionaryAliasQuizSourceType = "Dictionary";

    /// <summary>
    /// Minimum soru sayısı.
    /// </summary>
    private const int MinimumQuestionCount = 1;

    /// <summary>
    /// Maksimum soru sayısı.
    /// Çok büyük quiz oluşturmayı engellemek için sınır koyuyoruz.
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
            .WithMessage($"Only '{SupportedQuizType}' quiz type is supported.")
            .WithErrorCode("QUIZ_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizSourceType)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz source type is required.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_REQUIRED")
            .Must(IsSupportedQuizSourceType)
            .WithMessage($"Only '{SupportedQuizSourceType}' quiz source type is supported.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizContentMode)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz content mode is required.")
            .WithErrorCode("QUIZ_CONTENT_MODE_REQUIRED")
            .Must(IsSupportedQuizContentMode)
            .WithMessage("Only 'WordsOnly', 'PhrasesOnly' and 'Mixed' quiz content modes are supported.")
            .WithErrorCode("QUIZ_CONTENT_MODE_NOT_SUPPORTED");

        RuleFor(command => command.QuestionCount)
            .InclusiveBetween(MinimumQuestionCount, MaximumQuestionCount)
            .WithMessage($"Question count must be between {MinimumQuestionCount} and {MaximumQuestionCount}.")
            .WithErrorCode("QUESTION_COUNT_OUT_OF_RANGE");
    }

    /// <summary>
    /// QuizSourceType değerinin desteklenip desteklenmediğini kontrol eder.
    /// 
    /// Ana desteklenen değer:
    /// UserDictionary
    /// 
    /// Geriye uyumluluk alias değeri:
    /// Dictionary
    /// 
    /// Not:
    /// Dictionary alias'ı validator'dan geçerse handler tarafında da UserDictionary'ye çevrilmelidir.
    /// </summary>
    private static bool IsSupportedQuizSourceType(string? value)
    {
        return IsEqualIgnoreCase(value, SupportedQuizSourceType)
               || IsEqualIgnoreCase(value, DictionaryAliasQuizSourceType);
    }

    /// <summary>
    /// QuizContentMode değerinin Faz 18'de desteklenip desteklenmediğini kontrol eder.
    /// 
    /// Desteklenenler:
    /// - WordsOnly
    /// - PhrasesOnly
    /// - Mixed
    /// 
    /// Şimdilik desteklenmeyen:
    /// - SentencesOnly
    /// </summary>
    private static bool IsSupportedQuizContentMode(string? value)
    {
        return IsEqualIgnoreCase(value, "WordsOnly")
               || IsEqualIgnoreCase(value, "PhrasesOnly")
               || IsEqualIgnoreCase(value, "Mixed");
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