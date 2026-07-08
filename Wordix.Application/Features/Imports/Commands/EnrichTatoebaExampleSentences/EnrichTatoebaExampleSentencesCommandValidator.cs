using FluentValidation;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.EnrichTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence enrichment command validation kurallarıdır.
/// 
/// Bu endpoint admin/import amaçlıdır.
/// Yine de limitleri kontrol ediyoruz.
/// Çünkü büyük dosyalarla yanlışlıkla çok fazla insert yapılmasını istemiyoruz.
/// </summary>
public sealed class EnrichTatoebaExampleSentencesCommandValidator
    : AbstractValidator<EnrichTatoebaExampleSentencesCommand>
{
    private const int MaxParserRows = 50000;
    private const int MaxExamplesPerItem = 10;
    private const int MaxCreatedItems = 10000;
    private const int MaxSampleSize = 100;
    private const int MaxMessages = 500;

    public EnrichTatoebaExampleSentencesCommandValidator()
    {
        RuleFor(command => command.SourceSentencesStream)
            .NotNull()
            .WithMessage("Source sentences dosyası zorunludur.");

        RuleFor(command => command.SourceSentencesStream)
            .Must(stream => stream!.CanRead)
            .When(command => command.SourceSentencesStream is not null)
            .WithMessage("Source sentences dosyası okunabilir olmalıdır.");

        RuleFor(command => command.TargetSentencesStream)
            .NotNull()
            .WithMessage("Target sentences dosyası zorunludur.");

        RuleFor(command => command.TargetSentencesStream)
            .Must(stream => stream!.CanRead)
            .When(command => command.TargetSentencesStream is not null)
            .WithMessage("Target sentences dosyası okunabilir olmalıdır.");

        RuleFor(command => command.LinksStream)
            .NotNull()
            .WithMessage("Links dosyası zorunludur.");

        RuleFor(command => command.LinksStream)
            .Must(stream => stream!.CanRead)
            .When(command => command.LinksStream is not null)
            .WithMessage("Links dosyası okunabilir olmalıdır.");

        RuleFor(command => command.SourceFileName)
            .MaximumLength(255)
            .When(command => !string.IsNullOrWhiteSpace(command.SourceFileName))
            .WithMessage("Source file name en fazla 255 karakter olabilir.");

        RuleFor(command => command.TargetFileName)
            .MaximumLength(255)
            .When(command => !string.IsNullOrWhiteSpace(command.TargetFileName))
            .WithMessage("Target file name en fazla 255 karakter olabilir.");

        RuleFor(command => command.LinksFileName)
            .MaximumLength(255)
            .When(command => !string.IsNullOrWhiteSpace(command.LinksFileName))
            .WithMessage("Links file name en fazla 255 karakter olabilir.");

        RuleFor(command => command.SourceLanguageCode)
            .NotEmpty()
            .WithMessage("Source language code zorunludur.")
            .MaximumLength(10)
            .WithMessage("Source language code en fazla 10 karakter olabilir.");

        RuleFor(command => command.TargetLanguageCode)
            .NotEmpty()
            .WithMessage("Target language code zorunludur.")
            .MaximumLength(10)
            .WithMessage("Target language code en fazla 10 karakter olabilir.");

        RuleFor(command => command.SourceProviderLanguageCode)
            .NotEmpty()
            .WithMessage("Source provider language code zorunludur.")
            .MaximumLength(10)
            .WithMessage("Source provider language code en fazla 10 karakter olabilir.");

        RuleFor(command => command.TargetProviderLanguageCode)
            .NotEmpty()
            .WithMessage("Target provider language code zorunludur.")
            .MaximumLength(10)
            .WithMessage("Target provider language code en fazla 10 karakter olabilir.");

        RuleFor(command => command.ParserMaxRows)
            .GreaterThan(0)
            .When(command => command.ParserMaxRows.HasValue)
            .WithMessage("ParserMaxRows 0'dan büyük olmalıdır.");

        RuleFor(command => command.ParserMaxRows)
            .LessThanOrEqualTo(MaxParserRows)
            .When(command => command.ParserMaxRows.HasValue)
            .WithMessage($"ParserMaxRows en fazla {MaxParserRows} olabilir.");

        RuleFor(command => command.MaxExamplesPerLearningItem)
            .InclusiveBetween(1, MaxExamplesPerItem)
            .WithMessage($"MaxExamplesPerLearningItem 1 ile {MaxExamplesPerItem} arasında olmalıdır.");

        RuleFor(command => command.MaxCreatedItems)
            .GreaterThan(0)
            .When(command => command.MaxCreatedItems.HasValue)
            .WithMessage("MaxCreatedItems 0'dan büyük olmalıdır.");

        RuleFor(command => command.MaxCreatedItems)
            .LessThanOrEqualTo(MaxCreatedItems)
            .When(command => command.MaxCreatedItems.HasValue)
            .WithMessage($"MaxCreatedItems en fazla {MaxCreatedItems} olabilir.");

        RuleFor(command => command.AllowedItemTypes)
            .NotNull()
            .WithMessage("AllowedItemTypes null olamaz.");

        RuleFor(command => command.AllowedItemTypes)
            .Must(types =>
                types is not null &&
                types.All(type => type is LearningItemType.Word or LearningItemType.Phrase))
            .WithMessage("AllowedItemTypes sadece Word ve Phrase değerlerini içerebilir.");

        RuleFor(command => command.AllowedContentSources)
            .NotNull()
            .WithMessage("AllowedContentSources null olamaz.");

        RuleFor(command => command.SampleSize)
            .InclusiveBetween(1, MaxSampleSize)
            .WithMessage($"SampleSize 1 ile {MaxSampleSize} arasında olmalıdır.");

        RuleFor(command => command.MaxMessages)
            .InclusiveBetween(1, MaxMessages)
            .WithMessage($"MaxMessages 1 ile {MaxMessages} arasında olmalıdır.");

        RuleFor(command => command.License)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.License))
            .WithMessage("License en fazla 500 karakter olabilir.");
    }
}