using FluentValidation;

namespace Wordix.Application.Features.Decks.Commands.AddItemToDeck;

/// <summary>
/// AddItemToDeckCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Route'tan gelen DeckId boş mu kontrol eder.
/// - Body'den gelen UserLearningItemId boş mu kontrol eder.
/// 
/// Controller elle validation yapmaz.
/// ValidationBehavior bu validator'ı handler'dan önce çalıştırır.
/// </summary>
public sealed class AddItemToDeckCommandValidator
    : AbstractValidator<AddItemToDeckCommand>
{
    public AddItemToDeckCommandValidator()
    {
        RuleFor(command => command.DeckId)
            .NotEmpty()
            .WithMessage("Deck id is required.")
            .WithErrorCode("DECK_ID_REQUIRED");

        RuleFor(command => command.UserLearningItemId)
            .NotEmpty()
            .WithMessage("User learning item id is required.")
            .WithErrorCode("USER_LEARNING_ITEM_ID_REQUIRED");
    }
}