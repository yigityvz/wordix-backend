using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Kaikki meaning import provider sonucunu API response DTO'larına çevirir.
/// 
/// Neden mapper var?
/// - Handler'ın DTO mapping detaylarıyla şişmesini engeller.
/// - Feature bazlı explicit mapper standardımıza uyar.
/// - Provider modeli ile API response modelini birbirinden ayırır.
/// </summary>
public static class ParseKaikkiMeaningsMapper
{
    /// <summary>
    /// Provider sonucunu parse-test response modeline çevirir.
    /// </summary>
    public static ParseKaikkiMeaningsResponse ToResponse(
        MeaningImportProviderResult providerResult,
        int sampleSize)
    {
        var rows = providerResult.Rows;
        var safeSampleSize = sampleSize <= 0 ? 20 : sampleSize;

        var sampleRows = rows
            .Take(safeSampleSize)
            .Select(ToSampleResponse)
            .ToArray();

        return new ParseKaikkiMeaningsResponse
        {
            ProviderSucceeded = providerResult.Succeeded,
            TotalParsedRows = rows.Count,
            WordMeaningCount = rows.Count(row => !row.IsPhraseCandidate),
            PhraseCandidateCount = rows.Count(row => row.IsPhraseCandidate),
            ErrorCount = providerResult.Errors.Count,
            SampleRows = sampleRows,
            Messages = providerResult.Errors
        };
    }

    /// <summary>
    /// Provider'ın MeaningImportRow modelini küçük sample response'a çevirir.
    /// </summary>
    private static ParseKaikkiMeaningSampleResponse ToSampleResponse(
        MeaningImportRow row)
    {
        return new ParseKaikkiMeaningSampleResponse
        {
            SourceText = row.SourceText,
            NormalizedSourceText = row.NormalizedSourceText,
            MeaningText = row.MeaningText,
            NormalizedMeaningText = row.NormalizedMeaningText,
            ShortDefinition = row.ShortDefinition,
            PartOfSpeech = row.PartOfSpeech,
            IsPhraseCandidate = row.IsPhraseCandidate,
            SourceRowNumber = row.SourceRowNumber,
            TranslationIndex = row.TranslationIndex,
            ExternalSourceKey = row.ExternalSourceKey
        };
    }
}