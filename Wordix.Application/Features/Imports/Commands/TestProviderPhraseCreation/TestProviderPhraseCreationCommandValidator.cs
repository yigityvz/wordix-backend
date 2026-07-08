using FluentValidation;

namespace Wordix.Application.Features.Imports.Commands.TestProviderPhraseCreation;

/// <summary>
/// Provider-created phrase test command validation kuralları.
/// 
/// Gerçek database insert yapılacağı için metinleri sınırlıyoruz.
/// </summary>
public sealed class TestProviderPhraseCreationCommandValidator
    : AbstractValidator<TestProviderPhraseCreationCommand>
{
    private const int MaxPhraseLength = 300;
    private const int MaxMeaningLength = 500;

    public TestProviderPhraseCreationCommandValidator()
    {
        RuleFor(command => command.Text)
            .NotEmpty()
            .WithMessage("Phrase metni zorunludur.")
            .MaximumLength(MaxPhraseLength)
            .WithMessage($"Phrase metni en fazla {MaxPhraseLength} karakter olabilir.");

        RuleFor(command => command.MeaningText)
            .NotEmpty()
            .WithMessage("Meaning metni zorunludur.")
            .MaximumLength(MaxMeaningLength)
            .WithMessage($"Meaning metni en fazla {MaxMeaningLength} karakter olabilir.");

        RuleFor(command => command.SourceLanguageCode)
            .NotEmpty()
            .WithMessage("Kaynak dil kodu zorunludur.")
            .MaximumLength(10)
            .WithMessage("Kaynak dil kodu en fazla 10 karakter olabilir.");

        RuleFor(command => command.TargetLanguageCode)
            .NotEmpty()
            .WithMessage("Hedef dil kodu zorunludur.")
            .MaximumLength(10)
            .WithMessage("Hedef dil kodu en fazla 10 karakter olabilir.");
    }
}