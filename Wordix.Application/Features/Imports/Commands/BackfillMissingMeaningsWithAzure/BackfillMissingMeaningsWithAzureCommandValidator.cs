using FluentValidation;
using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.BackfillMissingMeaningsWithAzure;

/// <summary>
/// Azure missing meaning backfill command validation kurallarıdır.
/// 
/// Azure ücret/kota doğurabilecek provider olduğu için MaxItems kontrollü tutulur.
/// </summary>
public sealed class BackfillMissingMeaningsWithAzureCommandValidator
    : AbstractValidator<BackfillMissingMeaningsWithAzureCommand>
{
    private const int MaxAzureBackfillItemsPerRun = 5000;
    private const int MaxMessages = 1000;

    public BackfillMissingMeaningsWithAzureCommandValidator()
    {
        RuleFor(command => command.SourceLanguageCode)
            .NotEmpty()
            .WithMessage("SourceLanguageCode zorunludur.")
            .MaximumLength(10)
            .WithMessage("SourceLanguageCode en fazla 10 karakter olabilir.");

        RuleFor(command => command.TargetLanguageCode)
            .NotEmpty()
            .WithMessage("TargetLanguageCode zorunludur.")
            .MaximumLength(10)
            .WithMessage("TargetLanguageCode en fazla 10 karakter olabilir.");

        RuleFor(command => command.MaxItems)
            .InclusiveBetween(1, MaxAzureBackfillItemsPerRun)
            .WithMessage($"MaxItems 1 ile {MaxAzureBackfillItemsPerRun} arasında olmalıdır.");

        RuleFor(command => command.BatchSize)
            .InclusiveBetween(1, ImportConstants.MaxImportBatchSize)
            .WithMessage($"BatchSize 1 ile {ImportConstants.MaxImportBatchSize} arasında olmalıdır.");

        RuleFor(command => command.MaxMessages)
            .InclusiveBetween(1, MaxMessages)
            .WithMessage($"MaxMessages 1 ile {MaxMessages} arasında olmalıdır.");

        RuleFor(command => command.AllowedContentSources)
            .NotNull()
            .WithMessage("AllowedContentSources null olamaz.");

        RuleFor(command => command.AllowedContentSources)
            .Must(sources =>
                sources is not null &&
                sources.Count > 0 &&
                sources.All(source =>
                    source is ContentSource.CefrJ
                        or ContentSource.Octanove
                        or ContentSource.Manual))
            .WithMessage("AllowedContentSources sadece CefrJ, Octanove veya Manual içerebilir.");
    }
}