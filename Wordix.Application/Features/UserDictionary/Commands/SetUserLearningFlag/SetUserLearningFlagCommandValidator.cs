using FluentValidation;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserDictionary.Commands.SetUserLearningFlag;

/// <summary>
/// SetUserLearningFlagCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - UserLearningItemId boş mu kontrol eder.
/// - FlagType boş mu kontrol eder.
/// - FlagType desteklenen enum değerlerinden biri mi kontrol eder.
/// 
/// Bu validator ne yapmaz?
/// - UserLearningItem gerçekten var mı kontrol etmez.
/// - UserLearningItem current user'a mı ait kontrol etmez.
/// - Aynı flag daha önce eklenmiş mi kontrol etmez.
/// 
/// Bu kontroller handler tarafında yapılır.
/// </summary>
public sealed class SetUserLearningFlagCommandValidator
    : AbstractValidator<SetUserLearningFlagCommand>
{
    public SetUserLearningFlagCommandValidator()
    {
        RuleFor(command => command.UserLearningItemId)
            .NotEmpty()
            .WithMessage("User learning item id is required.")
            .WithErrorCode("USER_LEARNING_ITEM_ID_REQUIRED");

        RuleFor(command => command.FlagType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Flag type is required.")
            .WithErrorCode("FLAG_TYPE_REQUIRED")
            .Must(IsSupportedFlagType)
            .WithMessage("Flag type is not supported. Supported values are: Favorite, Difficult, WantMorePractice, Ignored.")
            .WithErrorCode("FLAG_TYPE_NOT_SUPPORTED");
    }

    /// <summary>
    /// API'den gelen string flag değerinin UserLearningFlagType enum'una çevrilip çevrilemediğini kontrol eder.
    /// 
    /// Case-insensitive çalışır.
    /// Örnek:
    /// difficult, Difficult, DIFFICULT değerleri kabul edilir.
    /// </summary>
    private static bool IsSupportedFlagType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse<UserLearningFlagType>(
            value.Trim(),
            ignoreCase: true,
            out var parsedFlagType)
            && Enum.IsDefined(parsedFlagType);
    }
}