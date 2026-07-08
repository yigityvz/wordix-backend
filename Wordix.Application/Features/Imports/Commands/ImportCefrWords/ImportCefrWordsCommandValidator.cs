using FluentValidation;
using Wordix.Application.Common.Constants;

namespace Wordix.Application.Features.Imports.Commands.ImportCefrWords;

/// <summary>
/// ImportCefrWordsCommand için validasyon kurallarını içerir.
/// </summary>
public sealed class ImportCefrWordsCommandValidator
    : AbstractValidator<ImportCefrWordsCommand>
{
    public ImportCefrWordsCommandValidator()
    {
        RuleFor(command => command.SourceStream)
            .NotNull()
            .WithMessage("CEFR import dosyası zorunludur.");

        RuleFor(command => command.SourceStream)
            .Must(stream => stream!.CanRead)
            .When(command => command.SourceStream is not null)
            .WithMessage("CEFR import dosyası okunabilir olmalıdır.");

        RuleFor(command => command.FileName)
            .MaximumLength(255)
            .When(command => !string.IsNullOrWhiteSpace(command.FileName))
            .WithMessage("Dosya adı en fazla 255 karakter olabilir.");

        RuleFor(command => command.ImportSource)
            .NotEmpty()
            .WithMessage("Import source zorunludur.")
            .Must(BeSupportedImportSource)
            .WithMessage("Import source sadece 'cefrj' veya 'octanove' olabilir.");

        RuleFor(command => command.SourceVersion)
            .MaximumLength(50)
            .When(command => !string.IsNullOrWhiteSpace(command.SourceVersion))
            .WithMessage("Source version en fazla 50 karakter olabilir.");

        RuleFor(command => command.SourceLanguageCode)
            .NotEmpty()
            .WithMessage("Kaynak dil kodu zorunludur.")
            .MaximumLength(10)
            .WithMessage("Kaynak dil kodu en fazla 10 karakter olabilir.");

        RuleFor(command => command.BatchSize)
            .GreaterThan(0)
            .When(command => command.BatchSize.HasValue)
            .WithMessage("Batch size 0'dan büyük olmalıdır.");

        RuleFor(command => command.BatchSize)
            .LessThanOrEqualTo(ImportConstants.MaxImportBatchSize)
            .When(command => command.BatchSize.HasValue)
            .WithMessage($"Batch size en fazla {ImportConstants.MaxImportBatchSize} olabilir.");
    }

    /// <summary>
    /// Endpoint'ten gelen import source değerinin desteklenip desteklenmediğini kontrol eder.
    /// 
    /// Kullanıcı "cefr-j", "CEFRJ", "octanove" gibi yazarsa normalize edip kontrol ederiz.
    /// </summary>
    private static bool BeSupportedImportSource(string value)
    {
        var normalizedSource = NormalizeImportSource(value);

        return normalizedSource is
            ImportConstants.ImportSources.CefrJ or
            ImportConstants.ImportSources.Octanove;
    }

    private static string NormalizeImportSource(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }
}