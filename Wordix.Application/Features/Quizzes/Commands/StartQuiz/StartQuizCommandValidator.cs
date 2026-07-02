using FluentValidation;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// StartQuizCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - QuizType değerini kontrol eder.
/// - QuizSourceType değerini kontrol eder.
/// - QuizContentMode değerini kontrol eder.
/// - QuizType + QuizContentMode kombinasyonunu kontrol eder.
/// - QuestionCount değerinin izin verilen aralıkta olup olmadığını kontrol eder.
/// - Deck quiz başlatılıyorsa DeckId zorunluluğunu kontrol eder.
/// 
/// Faz 21 itibarıyla desteklenen kombinasyon:
/// 
/// Test quiz:
/// - WordsOnly
/// - PhrasesOnly
/// - Mixed
/// - SentencesOnly desteklenmez.
/// 
/// Writing quiz:
/// - WordsOnly
/// - PhrasesOnly
/// - Mixed
/// - SentencesOnly
/// 
/// Önemli:
/// Validator database'e gitmez.
/// Deck var mı, current user'a ait mi, dictionary/deck içinde yeterli item var mı gibi kontroller
/// StartQuizCommandHandler içinde yapılır.
/// </summary>
public sealed class StartQuizCommandValidator : AbstractValidator<StartQuizCommand>
{
    /// <summary>
    /// Çoktan seçmeli quiz tipi.
    /// </summary>
    private const string TestQuizType = "Test";

    /// <summary>
    /// Kullanıcının cevabı kendisinin yazdığı quiz tipi.
    /// </summary>
    private const string WritingQuizType = "Writing";

    /// <summary>
    /// Kullanıcının kendi dictionary'sinden quiz başlatması için kullanılan kaynak tipi.
    /// </summary>
    private const string SupportedQuizSourceType = "UserDictionary";

    /// <summary>
    /// Eski/prototip dokümantasyonunda kullanılan alias değerdir.
    /// 
    /// Neden tutuyoruz?
    /// - Önceki Swagger örneklerinde "Dictionary" yazıyordu.
    /// - Frontend veya manuel testler hâlâ "Dictionary" gönderebilir.
    /// - Validator bunu kabul eder, handler tarafı da UserDictionary'ye çevirir.
    /// </summary>
    private const string DictionaryAliasQuizSourceType = "Dictionary";

    /// <summary>
    /// Deck kaynaklı quiz source değeri.
    /// </summary>
    private const string DeckQuizSourceType = "Deck";

    /// <summary>
    /// Sadece word itemlarıyla quiz başlatır.
    /// </summary>
    private const string WordsOnlyContentMode = "WordsOnly";

    /// <summary>
    /// Sadece phrase itemlarıyla quiz başlatır.
    /// </summary>
    private const string PhrasesOnlyContentMode = "PhrasesOnly";

    /// <summary>
    /// Test quiz için Word + Phrase.
    /// Writing quiz için Word + Phrase + Sentence.
    /// 
    /// Bu ayrımı validator değil, handler içindeki ResolveAllowedLearningItemTypes yapar.
    /// Validator sadece Mixed değerinin geçerli bir content mode olduğunu bilir.
    /// </summary>
    private const string MixedContentMode = "Mixed";

    /// <summary>
    /// Sadece sentence itemlarıyla quiz başlatır.
    /// 
    /// Faz 21 kararı:
    /// SentencesOnly sadece Writing quiz için desteklenir.
    /// Test quiz için desteklenmez.
    /// </summary>
    private const string SentencesOnlyContentMode = "SentencesOnly";

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
            .NotEmpty()
            .WithMessage("Quiz type is required.")
            .WithErrorCode("QUIZ_TYPE_REQUIRED")
            .Must(IsSupportedQuizType)
            .WithMessage($"Only '{TestQuizType}' and '{WritingQuizType}' quiz types are supported.")
            .WithErrorCode("QUIZ_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizSourceType)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz source type is required.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_REQUIRED")
            .Must(IsSupportedQuizSourceType)
            .WithMessage($"Only '{SupportedQuizSourceType}', '{DictionaryAliasQuizSourceType}' and '{DeckQuizSourceType}' quiz source types are supported.")
            .WithErrorCode("QUIZ_SOURCE_TYPE_NOT_SUPPORTED");

        RuleFor(command => command.QuizContentMode)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Quiz content mode is required.")
            .WithErrorCode("QUIZ_CONTENT_MODE_REQUIRED")
            .Must(IsSupportedQuizContentMode)
            .WithMessage($"Only '{WordsOnlyContentMode}', '{PhrasesOnlyContentMode}', '{MixedContentMode}' and '{SentencesOnlyContentMode}' quiz content modes are supported.")
            .WithErrorCode("QUIZ_CONTENT_MODE_NOT_SUPPORTED");

        RuleFor(command => command)
            .Must(IsValidQuizTypeAndContentModeCombination)
            .WithMessage("SentencesOnly content mode is supported only for Writing quiz type.")
            .WithErrorCode("SENTENCES_ONLY_REQUIRES_WRITING_QUIZ");

        RuleFor(command => command.QuestionCount)
            .InclusiveBetween(MinimumQuestionCount, MaximumQuestionCount)
            .WithMessage($"Question count must be between {MinimumQuestionCount} and {MaximumQuestionCount}.")
            .WithErrorCode("QUESTION_COUNT_OUT_OF_RANGE");

        RuleFor(command => command.DeckId)
            .NotEmpty()
            .When(command => IsEqualIgnoreCase(command.QuizSourceType, DeckQuizSourceType))
            .WithMessage("Deck id is required when quiz source type is Deck.")
            .WithErrorCode("DECK_ID_REQUIRED_FOR_DECK_QUIZ");
    }

    /// <summary>
    /// QuizType değerinin desteklenip desteklenmediğini kontrol eder.
    /// </summary>
    private static bool IsSupportedQuizType(string? value)
    {
        return IsEqualIgnoreCase(value, TestQuizType)
               || IsEqualIgnoreCase(value, WritingQuizType);
    }

    /// <summary>
    /// QuizSourceType değerinin desteklenip desteklenmediğini kontrol eder.
    /// 
    /// Desteklenenler:
    /// - UserDictionary
    /// - Dictionary alias
    /// - Deck
    /// </summary>
    private static bool IsSupportedQuizSourceType(string? value)
    {
        return IsEqualIgnoreCase(value, SupportedQuizSourceType)
               || IsEqualIgnoreCase(value, DictionaryAliasQuizSourceType)
               || IsEqualIgnoreCase(value, DeckQuizSourceType);
    }

    /// <summary>
    /// QuizContentMode değerinin desteklenip desteklenmediğini kontrol eder.
    /// 
    /// Faz 21 itibarıyla SentencesOnly de content mode olarak tanınır.
    /// Ancak SentencesOnly sadece Writing quiz için geçerlidir.
    /// Bu kombinasyon kuralı IsValidQuizTypeAndContentModeCombination methodunda kontrol edilir.
    /// </summary>
    private static bool IsSupportedQuizContentMode(string? value)
    {
        return IsEqualIgnoreCase(value, WordsOnlyContentMode)
               || IsEqualIgnoreCase(value, PhrasesOnlyContentMode)
               || IsEqualIgnoreCase(value, MixedContentMode)
               || IsEqualIgnoreCase(value, SentencesOnlyContentMode);
    }

    /// <summary>
    /// QuizType + QuizContentMode kombinasyonunun geçerli olup olmadığını kontrol eder.
    /// 
    /// Kural:
    /// - SentencesOnly sadece Writing quiz ile kullanılabilir.
    /// - Test + SentencesOnly desteklenmez.
    /// 
    /// Eğer QuizType veya QuizContentMode zaten geçersizse burada ekstra hata üretmemeye çalışırız.
    /// O alanların kendi validation kuralları zaten uygun hata mesajını döner.
    /// </summary>
    private static bool IsValidQuizTypeAndContentModeCombination(
        StartQuizCommand command)
    {
        if (!IsEqualIgnoreCase(command.QuizContentMode, SentencesOnlyContentMode))
        {
            return true;
        }

        if (!IsSupportedQuizType(command.QuizType))
        {
            return true;
        }

        return IsEqualIgnoreCase(command.QuizType, WritingQuizType);
    }

    /// <summary>
    /// String değerleri trim ederek case-insensitive karşılaştırır.
    /// 
    /// Neden ayrı method?
    /// - Kullanıcı "test", "TEST", " Test " gibi değerler gönderirse bunları aynı kabul etmek istiyoruz.
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