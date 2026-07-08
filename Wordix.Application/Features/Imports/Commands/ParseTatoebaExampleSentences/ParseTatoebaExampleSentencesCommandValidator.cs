using FluentValidation;

namespace Wordix.Application.Features.Imports.Commands.ParseTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence parser test command validation kurallarıdır.
/// 
/// Bu endpoint test/admin amaçlı olduğu için büyük dosyayı sınırsız response'a dökmek istemiyoruz.
/// MaxRows, SampleSize ve MaxMessages sınırları bunun için var.
/// </summary>
public sealed class ParseTatoebaExampleSentencesCommandValidator
    : AbstractValidator<ParseTatoebaExampleSentencesCommand>
{
    /// <summary>
    /// Parse-test endpointi için en fazla kaç eşleşmiş row üretilebileceği.
    /// 
    /// Gerçek import job fazında bu limit farklı yönetilebilir.
    /// </summary>
    private const int MaxParseTestRows = 10000;

    /// <summary>
    /// Response içinde gösterilecek maksimum sample satır sayısı.
    /// </summary>
    private const int MaxSampleSize = 100;

    /// <summary>
    /// Response içinde gösterilecek maksimum provider mesaj sayısı.
    /// </summary>
    private const int MaxMessages = 500;

    public ParseTatoebaExampleSentencesCommandValidator()
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

        RuleFor(command => command.MaxMessages)
            .GreaterThan(0)
            .WithMessage("MaxMessages 0'dan büyük olmalıdır.");

        RuleFor(command => command.MaxMessages)
            .LessThanOrEqualTo(MaxMessages)
            .WithMessage($"MaxMessages en fazla {MaxMessages} olabilir.");

        RuleFor(command => command.License)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.License))
            .WithMessage("License en fazla 500 karakter olabilir.");
    }
}