using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// FreeDict parser sonucu ve meaning enrichment sonucunu API response modeline çevirir.
/// 
/// Neden ayrı mapper var?
/// - Handler'ın response mapping detaylarıyla şişmesini engeller.
/// - Kaikki ve FreeDict response modellerini ayrı tutar.
/// - Feature bazlı explicit mapper standardımıza uyar.
/// </summary>
public static class EnrichFreeDictMeaningsMapper
{
    /// <summary>
    /// Parser ve enrichment sonuçlarını tek response modeline dönüştürür.
    /// </summary>
    public static EnrichFreeDictMeaningsResponse ToResponse(
        MeaningImportProviderResult providerResult,
        MeaningEnrichmentResult enrichmentResult,
        int maxMessages,
        Guid? importJobId = null)
    {
        var messages = providerResult.Errors
            .Concat(enrichmentResult.Messages)
            .Take(maxMessages)
            .ToArray();

        return new EnrichFreeDictMeaningsResponse
        {
            ImportJobId = importJobId,

            ProviderSucceeded = providerResult.Succeeded,
            ProviderParsedRows = providerResult.Rows.Count,
            ProviderErrorCount = providerResult.Errors.Count,

            DryRun = enrichmentResult.DryRun,
            TotalInputRows = enrichmentResult.TotalInputRows,
            MatchedWordCount = enrichmentResult.MatchedWordCount,
            NotMatchedCount = enrichmentResult.NotMatchedCount,
            SkippedPhraseCandidateCount = enrichmentResult.SkippedPhraseCandidateCount,
            SkippedExistingMeaningCount = enrichmentResult.SkippedExistingMeaningCount,
            SkippedDuplicateInputCount = enrichmentResult.SkippedDuplicateInputCount,
            WouldCreateCount = enrichmentResult.WouldCreateCount,
            CreatedCount = enrichmentResult.CreatedCount,
            FailedCount = enrichmentResult.FailedCount,

            Messages = messages
        };
    }

    /// <summary>
    /// Provider başarısız olduğunda enrichment çalıştırmadan response üretir.
    /// </summary>
    public static EnrichFreeDictMeaningsResponse ToProviderFailureResponse(
        MeaningImportProviderResult providerResult,
        bool dryRun,
        int maxMessages,
        Guid? importJobId = null)
    {
        return new EnrichFreeDictMeaningsResponse
        {
            ImportJobId = importJobId,

            ProviderSucceeded = providerResult.Succeeded,
            ProviderParsedRows = providerResult.Rows.Count,
            ProviderErrorCount = providerResult.Errors.Count,

            DryRun = dryRun,
            TotalInputRows = 0,
            MatchedWordCount = 0,
            NotMatchedCount = 0,
            SkippedPhraseCandidateCount = 0,
            SkippedExistingMeaningCount = 0,
            SkippedDuplicateInputCount = 0,
            WouldCreateCount = 0,
            CreatedCount = 0,
            FailedCount = providerResult.Errors.Count,

            Messages = providerResult.Errors
                .Take(maxMessages)
                .ToArray()
        };
    }
}