using FluentValidation;

namespace Wordix.Application.Features.Decks.Queries.GetDeckById;

/// <summary>
/// GetDeckByIdQuery için FluentValidation validator sınıfıdır.
/// 
/// Bu validator ne yapar?
/// - Route'tan gelen DeckId değerinin boş Guid olup olmadığını kontrol eder.
/// 
/// Controller route id kontrolü yapmaz.
/// ValidationBehavior bu validator'ı handler'dan önce çalıştırır.
/// </summary>
public sealed class GetDeckByIdQueryValidator
    : AbstractValidator<GetDeckByIdQuery>
{
    public GetDeckByIdQueryValidator()
    {
        RuleFor(query => query.DeckId)
            .NotEmpty()
            .WithMessage("Deck id is required.")
            .WithErrorCode("DECK_ID_REQUIRED");
    }
}