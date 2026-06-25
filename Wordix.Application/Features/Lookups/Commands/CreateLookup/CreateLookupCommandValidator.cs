using FluentValidation;

namespace Wordix.Application.Features.Lookups.Commands.CreateLookup;

/// <summary>
/// CreateLookupCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Lookup text boş mu kontrol eder.
/// - Lookup text uzunluğu makul mü kontrol eder.
/// - SourceLanguageCode ve TargetLanguageCode boş mu kontrol eder.
/// - Dil kodları beklenen formatta mı kontrol eder.
/// - Source ve target language aynı mı kontrol eder.
/// 
/// Bu validator handler'dan önce çalışır.
/// ValidationBehavior bu validator'ı otomatik yakalar.
/// Hata varsa handler'a hiç gidilmez.
/// ExceptionMiddleware 400 VALIDATION_ERROR response'u döner.
/// </summary>
public sealed class CreateLookupCommandValidator : AbstractValidator<CreateLookupCommand>
{
    /// <summary>
    /// Lookup text için izin verdiğimiz maksimum karakter sayısı.
    /// 
    /// İlk prototipte single-word lookup destekliyoruz.
    /// Ama kullanıcı yanlışlıkla uzun cümle girerse de sistem gereksiz yere büyük input işlemek zorunda kalmasın.
    /// </summary>
    private const int MaximumTextLength = 250;

    /// <summary>
    /// Dil kodu için izin verilen maksimum uzunluk.
    /// 
    /// Örnek geçerli değerler:
    /// - en
    /// - tr
    /// - en-US
    /// - pt-BR
    /// 
    /// İlk prototipte en/tr kullanacağız ama ileride bölgesel dil kodlarına da hazır kalıyoruz.
    /// </summary>
    private const int MaximumLanguageCodeLength = 10;

    /// <summary>
    /// Dil kodu formatını kontrol eden regex pattern.
    /// 
    /// Desteklenen örnekler:
    /// - en
    /// - tr
    /// - de
    /// - en-US
    /// - pt-BR
    /// </summary>
    private const string LanguageCodePattern = "^[a-zA-Z]{2,3}(-[a-zA-Z]{2})?$";

    /// <summary>
    /// Validator kuralları constructor içinde tanımlanır.
    /// FluentValidation bu kuralları CreateLookupCommand için çalıştırır.
    /// </summary>
    public CreateLookupCommandValidator()
    {
        RuleFor(command => command.Text)
            .Must(text => !string.IsNullOrWhiteSpace(text))
            .WithMessage("Lookup text is required.")
            .WithErrorCode("LOOKUP_TEXT_REQUIRED")
            .MaximumLength(MaximumTextLength)
            .WithMessage($"Lookup text cannot exceed {MaximumTextLength} characters.")
            .WithErrorCode("LOOKUP_TEXT_MAX_LENGTH");

        RuleFor(command => command.SourceLanguageCode)
            .Must(languageCode => !string.IsNullOrWhiteSpace(languageCode))
            .WithMessage("Source language code is required.")
            .WithErrorCode("SOURCE_LANGUAGE_CODE_REQUIRED")
            .MaximumLength(MaximumLanguageCodeLength)
            .WithMessage($"Source language code cannot exceed {MaximumLanguageCodeLength} characters.")
            .WithErrorCode("SOURCE_LANGUAGE_CODE_MAX_LENGTH")
            .Matches(LanguageCodePattern)
            .WithMessage("Source language code format is invalid.")
            .WithErrorCode("SOURCE_LANGUAGE_CODE_INVALID");

        RuleFor(command => command.TargetLanguageCode)
            .Must(languageCode => !string.IsNullOrWhiteSpace(languageCode))
            .WithMessage("Target language code is required.")
            .WithErrorCode("TARGET_LANGUAGE_CODE_REQUIRED")
            .MaximumLength(MaximumLanguageCodeLength)
            .WithMessage($"Target language code cannot exceed {MaximumLanguageCodeLength} characters.")
            .WithErrorCode("TARGET_LANGUAGE_CODE_MAX_LENGTH")
            .Matches(LanguageCodePattern)
            .WithMessage("Target language code format is invalid.")
            .WithErrorCode("TARGET_LANGUAGE_CODE_INVALID");

        RuleFor(command => command)
            .Must(command => !AreLanguageCodesSame(
                command.SourceLanguageCode,
                command.TargetLanguageCode))
            .WithMessage("Source language code and target language code cannot be the same.")
            .WithErrorCode("LOOKUP_LANGUAGE_PAIR_INVALID");
    }

    /// <summary>
    /// Source ve target language code değerleri aynı mı kontrol eder.
    /// 
    /// Neden ayrı method?
    /// - RuleFor içinde karmaşık string karşılaştırması yazmamak için.
    /// - Trim ve case-insensitive karşılaştırmayı merkezi yapmak için.
    /// </summary>
    private static bool AreLanguageCodesSame(
        string? sourceLanguageCode,
        string? targetLanguageCode)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguageCode)
            || string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            return false;
        }

        return string.Equals(
            sourceLanguageCode.Trim(),
            targetLanguageCode.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}