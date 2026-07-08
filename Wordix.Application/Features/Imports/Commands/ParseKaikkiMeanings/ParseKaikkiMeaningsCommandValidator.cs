using FluentValidation;

namespace Wordix.Application.Features.Imports.Commands.ParseKaikkiMeanings;

/// <summary>
/// Kaikki meaning parser test command validation kuralları.
/// 
/// Bu endpoint test amaçlı olduğu için büyük dosyayı sınırsız parse ettirmiyoruz.
/// MaxRows ve SampleSize limitleri Swagger testini güvenli tutar.
/// </summary>
public sealed class ParseKaikkiMeaningsCommandValidator
    : AbstractValidator<ParseKaikkiMeaningsCommand>
{
    private const int MaxParseTestRows = 5000;
    private const int MaxSampleSize = 100;

    public ParseKaikkiMeaningsCommandValidator()
    {
        RuleFor(command => command.SourceStream)
            .NotNull()
            .WithMessage("Kaikki JSONL dosyası zorunludur.");

        RuleFor(command => command.SourceStream)
            .Must(stream => stream!.CanRead)
            .When(command => command.SourceStream is not null)
            .WithMessage("Kaikki JSONL dosyası okunabilir olmalıdır.");

        RuleFor(command => command.FileName)
            .MaximumLength(255)
            .When(command => !string.IsNullOrWhiteSpace(command.FileName))
            .WithMessage("Dosya adı en fazla 255 karakter olabilir.");

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

        RuleFor(command => command.MaxRows)
            .GreaterThan(0)
            .When(command => command.MaxRows.HasValue)
            .WithMessage("MaxRows 0'dan büyük olmalıdır.");

        RuleFor(command => command.MaxRows)
            .LessThanOrEqualTo(MaxParseTestRows)
            .When(command => command.MaxRows.HasValue)
            .WithMessage($"MaxRows en fazla {MaxParseTestRows} olabilir.");

        RuleFor(command => command.SampleSize)
            .GreaterThan(0)
            .WithMessage("SampleSize 0'dan büyük olmalıdır.");

        RuleFor(command => command.SampleSize)
            .LessThanOrEqualTo(MaxSampleSize)
            .WithMessage($"SampleSize en fazla {MaxSampleSize} olabilir.");
    }
}