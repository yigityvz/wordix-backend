using FluentValidation;
using Wordix.Application.Common.Constants;

namespace Wordix.Application.Features.Imports.Commands.EnrichKaikkiMeanings;

/// <summary>
/// Kaikki meaning enrichment command validation kuralları.
/// 
/// Büyük dosya ve gerçek insert işlemi yapılabileceği için
/// limitleri burada kontrollü tutuyoruz.
/// </summary>
public sealed class EnrichKaikkiMeaningsCommandValidator
    : AbstractValidator<EnrichKaikkiMeaningsCommand>
{
    private const int MaxParserRows = 200000;
    private const int MaxMessages = 1000;

    public EnrichKaikkiMeaningsCommandValidator()
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
            .LessThanOrEqualTo(MaxParserRows)
            .When(command => command.MaxRows.HasValue)
            .WithMessage($"MaxRows en fazla {MaxParserRows} olabilir.");

        RuleFor(command => command.BatchSize)
            .GreaterThan(0)
            .WithMessage("BatchSize 0'dan büyük olmalıdır.");

        RuleFor(command => command.BatchSize)
            .LessThanOrEqualTo(ImportConstants.MaxImportBatchSize)
            .WithMessage($"BatchSize en fazla {ImportConstants.MaxImportBatchSize} olabilir.");

        RuleFor(command => command.MaxMessages)
            .GreaterThan(0)
            .WithMessage("MaxMessages 0'dan büyük olmalıdır.");

        RuleFor(command => command.MaxMessages)
            .LessThanOrEqualTo(MaxMessages)
            .WithMessage($"MaxMessages en fazla {MaxMessages} olabilir.");
    }
}