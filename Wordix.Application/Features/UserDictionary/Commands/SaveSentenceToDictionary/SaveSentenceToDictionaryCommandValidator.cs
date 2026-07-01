using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveSentenceToDictionary;

/// <summary>
/// SaveSentenceToDictionaryCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator handler'dan önce çalışır.
/// Controller elle validation yapmaz.
/// </summary>
public sealed class SaveSentenceToDictionaryCommandValidator
    : AbstractValidator<SaveSentenceToDictionaryCommand>
{
    public SaveSentenceToDictionaryCommandValidator()
    {
        RuleFor(command => command.SourceText)
            .NotEmpty()
            .WithMessage("Source sentence text is required.")
            .WithErrorCode("SOURCE_TEXT_REQUIRED")
            .MaximumLength(1000)
            .WithMessage("Source sentence text cannot exceed 1000 characters.")
            .WithErrorCode("SOURCE_TEXT_TOO_LONG");

        RuleFor(command => command.TranslatedText)
            .NotEmpty()
            .WithMessage("Translated sentence text is required.")
            .WithErrorCode("TRANSLATED_TEXT_REQUIRED")
            .MaximumLength(1000)
            .WithMessage("Translated sentence text cannot exceed 1000 characters.")
            .WithErrorCode("TRANSLATED_TEXT_TOO_LONG");

        RuleFor(command => command.SourceLanguageCode)
            .NotEmpty()
            .WithMessage("Source language code is required.")
            .WithErrorCode("SOURCE_LANGUAGE_CODE_REQUIRED")
            .MaximumLength(10)
            .WithMessage("Source language code cannot exceed 10 characters.")
            .WithErrorCode("SOURCE_LANGUAGE_CODE_TOO_LONG");

        RuleFor(command => command.TargetLanguageCode)
            .NotEmpty()
            .WithMessage("Target language code is required.")
            .WithErrorCode("TARGET_LANGUAGE_CODE_REQUIRED")
            .MaximumLength(10)
            .WithMessage("Target language code cannot exceed 10 characters.")
            .WithErrorCode("TARGET_LANGUAGE_CODE_TOO_LONG");

        RuleFor(command => command.SourceLookupHistoryId)
            .Must(BeNullOrNotEmptyGuid)
            .WithMessage("Source lookup history id must be a valid id when provided.")
            .WithErrorCode("SOURCE_LOOKUP_HISTORY_ID_INVALID");
    }

    /// <summary>
    /// Nullable Guid alanı null olabilir.
    /// Ama değer gönderildiyse Guid.Empty olamaz.
    /// </summary>
    private static bool BeNullOrNotEmptyGuid(Guid? value)
    {
        return value is null || value.Value != Guid.Empty;
    }
}