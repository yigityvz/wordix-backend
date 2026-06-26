using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;

/// <summary>
/// SaveLearningItemCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - LearningItemId boş Guid mi kontrol eder.
/// - SelectedMeaningId gönderildiyse boş Guid mi kontrol eder.
/// - SourceLookupHistoryId gönderildiyse boş Guid mi kontrol eder.
/// 
/// Bu validator handler'dan önce çalışır.
/// Faz 12'de yazdığımız ValidationBehavior bu validator'ı otomatik yakalar.
/// Hata varsa SaveLearningItemCommandHandler'a hiç gidilmez.
/// ExceptionMiddleware standart 400 VALIDATION_ERROR response'u döner.
/// </summary>
public sealed class SaveLearningItemCommandValidator
    : AbstractValidator<SaveLearningItemCommand>
{
    /// <summary>
    /// Validator kuralları constructor içinde tanımlanır.
    /// FluentValidation bu kuralları SaveLearningItemCommand için çalıştırır.
    /// </summary>
    public SaveLearningItemCommandValidator()
    {
        RuleFor(command => command.LearningItemId)
            .NotEmpty()
            .WithMessage("Learning item id is required.")
            .WithErrorCode("LEARNING_ITEM_ID_REQUIRED");

        RuleFor(command => command.SelectedMeaningId)
            .Must(BeNullOrNotEmptyGuid)
            .WithMessage("Selected meaning id must be a valid id when provided.")
            .WithErrorCode("SELECTED_MEANING_ID_INVALID");

        RuleFor(command => command.SourceLookupHistoryId)
            .Must(BeNullOrNotEmptyGuid)
            .WithMessage("Source lookup history id must be a valid id when provided.")
            .WithErrorCode("SOURCE_LOOKUP_HISTORY_ID_INVALID");
    }

    /// <summary>
    /// Nullable Guid alanlarının validasyonunu yapar.
    /// 
    /// Kural:
    /// - null olabilir.
    /// - ama değer gönderildiyse Guid.Empty olamaz.
    /// 
    /// Neden?
    /// SelectedMeaningId ve SourceLookupHistoryId opsiyonel alanlardır.
    /// Bu yüzden null değerleri geçerli kabul ediyoruz.
    /// Ancak "00000000-0000-0000-0000-000000000000" gerçek bir id değildir.
    /// </summary>
    private static bool BeNullOrNotEmptyGuid(Guid? value)
    {
        return value is null || value.Value != Guid.Empty;
    }
}