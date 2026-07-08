using FluentValidation;

namespace Wordix.Application.Features.Imports.Commands.TestAzureTranslation;

/// <summary>
/// Azure translation test command validation kuralları.
/// 
/// Bu endpoint gerçek Azure çağrısı yaptığı için metin uzunluğunu sınırlıyoruz.
/// Böylece yanlışlıkla büyük metin gönderip kota tüketmeyiz.
/// </summary>
public sealed class TestAzureTranslationCommandValidator
    : AbstractValidator<TestAzureTranslationCommand>
{
    private const int MaxTextLength = 1000;

    public TestAzureTranslationCommandValidator()
    {
        RuleFor(command => command.Text)
            .NotEmpty()
            .WithMessage("Çevrilecek metin zorunludur.")
            .MaximumLength(MaxTextLength)
            .WithMessage($"Çevrilecek metin en fazla {MaxTextLength} karakter olabilir.");

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

        RuleFor(command => command.Purpose)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.Purpose))
            .WithMessage("Purpose en fazla 100 karakter olabilir.");
    }
}