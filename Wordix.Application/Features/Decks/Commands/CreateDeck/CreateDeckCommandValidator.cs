using FluentValidation;

namespace Wordix.Application.Features.Decks.Commands.CreateDeck;

/// <summary>
/// CreateDeckCommand için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Deck adının boş olup olmadığını kontrol eder.
/// - Deck adının maksimum uzunluğunu kontrol eder.
/// - Description alanının maksimum uzunluğunu kontrol eder.
/// 
/// Bu validator handler'dan önce çalışır.
/// Controller elle validation yapmaz.
/// </summary>
public sealed class CreateDeckCommandValidator
    : AbstractValidator<CreateDeckCommand>
{
    /// <summary>
    /// Deck adı için maksimum uzunluk.
    /// DeckConfiguration içinde de Name max length 100 olarak ayarlandı.
    /// Validator ve database constraint aynı mantıkta olmalı.
    /// </summary>
    private const int MaximumNameLength = 100;

    /// <summary>
    /// Deck açıklaması için maksimum uzunluk.
    /// DeckConfiguration içinde Description max length 500 olarak ayarlandı.
    /// </summary>
    private const int MaximumDescriptionLength = 500;

    public CreateDeckCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Deck name is required.")
            .WithErrorCode("DECK_NAME_REQUIRED")
            .MaximumLength(MaximumNameLength)
            .WithMessage($"Deck name cannot exceed {MaximumNameLength} characters.")
            .WithErrorCode("DECK_NAME_TOO_LONG");

        RuleFor(command => command.Description)
            .MaximumLength(MaximumDescriptionLength)
            .WithMessage($"Deck description cannot exceed {MaximumDescriptionLength} characters.")
            .WithErrorCode("DECK_DESCRIPTION_TOO_LONG");
    }
}