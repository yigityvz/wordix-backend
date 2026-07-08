using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Tatoeba example sentence import provider sonucunu API response DTO'larına çevirir.
/// 
/// Neden mapper var?
/// - Handler'ın DTO mapping detaylarıyla şişmesini engeller.
/// - Feature bazlı explicit mapper standardımıza uyar.
/// - Provider modeli ile API response modelini birbirinden ayırır.
/// </summary>
public static class ParseTatoebaExampleSentencesMapper
{
    /// <summary>
    /// Provider sonucunu parse-test response modeline çevirir.
    /// </summary>
    public static ParseTatoebaExampleSentencesResponse ToResponse(
        ExampleSentenceImportProviderResult providerResult,
        int sampleSize)
    {
        var rows = providerResult.Rows;
        var safeSampleSize = sampleSize <= 0 ? 20 : sampleSize;

        var firstRow = rows.FirstOrDefault();

        var sampleRows = rows
            .Take(safeSampleSize)
            .Select(ToSampleResponse)
            .ToArray();

        return new ParseTatoebaExampleSentencesResponse
        {
            ProviderSucceeded = providerResult.Succeeded,
            TotalParsedRows = rows.Count,

            SourceLanguageCode = firstRow?.SourceLanguageCode ?? string.Empty,
            TargetLanguageCode = firstRow?.TargetLanguageCode ?? string.Empty,
            SourceProviderLanguageCode = firstRow?.SourceProviderLanguageCode ?? string.Empty,
            TargetProviderLanguageCode = firstRow?.TargetProviderLanguageCode ?? string.Empty,

            ErrorCount = providerResult.Errors.Count,
            SampleRows = sampleRows,
            Messages = providerResult.Errors
        };
    }

    /// <summary>
    /// Provider'ın ExampleSentenceImportRow modelini küçük sample response'a çevirir.
    /// </summary>
    private static ParseTatoebaExampleSentenceSampleResponse ToSampleResponse(
        ExampleSentenceImportRow row)
    {
        return new ParseTatoebaExampleSentenceSampleResponse
        {
            SourceSentenceExternalId = row.SourceSentenceExternalId,
            TargetSentenceExternalId = row.TargetSentenceExternalId,

            SourceText = row.SourceText,
            NormalizedSourceText = row.NormalizedSourceText,

            TranslatedText = row.TranslatedText,
            NormalizedTranslatedText = row.NormalizedTranslatedText,

            SourceProvider = row.SourceProvider,
            ExternalSourceKey = row.ExternalSourceKey,

            SourceSentenceRowNumber = row.SourceSentenceRowNumber,
            TargetSentenceRowNumber = row.TargetSentenceRowNumber,
            LinkRowNumber = row.LinkRowNumber
        };
    }
}