using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;

/// <summary>
/// GetUserDictionaryItemByIdQuery için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Route üzerinden gelen UserLearningItemId boş Guid mi kontrol eder.
/// 
/// Bu validator neden var?
/// - Controller içinde elle Guid.Empty kontrolü yapmak istemiyoruz.
/// - Validation işlemleri MediatR pipeline'daki ValidationBehavior üzerinden yürümelidir.
/// - Böylece tüm validation hataları standart ErrorResponse formatında döner.
/// </summary>
public sealed class GetUserDictionaryItemByIdQueryValidator
    : AbstractValidator<GetUserDictionaryItemByIdQuery>
{
    /// <summary>
    /// Validator kuralları constructor içinde tanımlanır.
    /// FluentValidation bu kuralları GetUserDictionaryItemByIdQuery için çalıştırır.
    /// </summary>
    public GetUserDictionaryItemByIdQueryValidator()
    {
        RuleFor(query => query.UserLearningItemId)
            .NotEmpty()
            .WithMessage("User dictionary item id is required.")
            .WithErrorCode("USER_DICTIONARY_ITEM_ID_REQUIRED");
    }
}