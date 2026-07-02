using FluentValidation;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningFlags;

/// <summary>
/// GetUserLearningFlagsQuery için FluentValidation validator sınıfıdır.
/// 
/// Bu validator sadece route id boş mu diye kontrol eder.
/// 
/// Şu kontrolleri yapmaz:
/// - UserLearningItem var mı?
/// - Current user'a mı ait?
/// - Item aktif mi?
/// 
/// Bu kontroller handler tarafında repository üzerinden yapılır.
/// </summary>
public sealed class GetUserLearningFlagsQueryValidator
    : AbstractValidator<GetUserLearningFlagsQuery>
{
    public GetUserLearningFlagsQueryValidator()
    {
        RuleFor(query => query.UserLearningItemId)
            .NotEmpty()
            .WithMessage("User learning item id is required.")
            .WithErrorCode("USER_LEARNING_ITEM_ID_REQUIRED");
    }
}