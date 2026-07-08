using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Commands.BackfillMissingMeaningsWithAzure;
using Wordix.Application.Features.Imports.Dtos.Requests;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Azure missing meaning backfill request DTO'sunu command modeline çevirir.
/// </summary>
public static class AzureMissingMeaningBackfillMapper
{
    public static BackfillMissingMeaningsWithAzureCommand ToCommand(
        AzureMissingMeaningBackfillRequest? request)
    {
        return new BackfillMissingMeaningsWithAzureCommand
        {
            SourceLanguageCode = string.IsNullOrWhiteSpace(request?.SourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : request.SourceLanguageCode.Trim(),

            TargetLanguageCode = string.IsNullOrWhiteSpace(request?.TargetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : request.TargetLanguageCode.Trim(),

            MaxItems = request?.MaxItems ?? 100,

            DryRun = request?.DryRun ?? true,

            BatchSize = request?.BatchSize ?? 25,

            MaxMessages = request?.MaxMessages ?? 100,

            AllowedContentSources =
                request?.AllowedContentSources is { Count: > 0 }
                    ? request.AllowedContentSources
                    : new[]
                    {
                        ContentSource.CefrJ,
                        ContentSource.Octanove,
                        ContentSource.Manual
                    }
        };
    }
}