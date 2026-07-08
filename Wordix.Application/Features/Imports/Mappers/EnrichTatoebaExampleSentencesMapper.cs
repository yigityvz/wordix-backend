using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Tatoeba example sentence provider + enrichment result modellerini
/// API response DTO'larına çevirir.
/// 
/// Neden mapper var?
/// - Handler'ın response mapping detaylarıyla şişmesini engeller.
/// - Feature bazlı explicit mapper standardımıza uyar.
/// - Application service result modeli ile API response DTO'sunu birbirinden ayırır.
/// </summary>
public static class EnrichTatoebaExampleSentencesMapper
{
    /// <summary>
    /// Provider ve enrichment sonucunu API response modeline çevirir.
    /// 
    /// enrichmentResult null olabilir.
    /// Örneğin provider tamamen başarısızsa enrichment hiç çalıştırılmaz.
    /// 
    /// importJobId neden nullable?
    /// - Normal endpoint akışında dolu olur.
    /// - Unit test veya eski handler çağrılarında null bırakılabilir.
    /// </summary>
    public static EnrichTatoebaExampleSentencesResponse ToResponse(
        ExampleSentenceImportProviderResult providerResult,
        ExampleSentenceEnrichmentResult? enrichmentResult,
        bool dryRun,
        Guid? importJobId = null)
    {
        return new EnrichTatoebaExampleSentencesResponse
        {
            ImportJobId = importJobId,

            ProviderSucceeded = providerResult.Succeeded,
            ProviderParsedRows = providerResult.Rows.Count,
            ProviderErrorCount = providerResult.Errors.Count,
            ProviderMessages = providerResult.Errors,

            EnrichmentSucceeded = enrichmentResult?.Succeeded ?? false,
            DryRun = enrichmentResult?.DryRun ?? dryRun,

            TotalInputRows = enrichmentResult?.TotalInputRows ?? providerResult.Rows.Count,
            CandidateLearningItemCount = enrichmentResult?.CandidateLearningItemCount ?? 0,
            MatchedRowCount = enrichmentResult?.MatchedRowCount ?? 0,
            NotMatchedRowCount = enrichmentResult?.NotMatchedRowCount ?? 0,
            SkippedCount = enrichmentResult?.SkippedCount ?? 0,
            WouldCreateCount = enrichmentResult?.WouldCreateCount ?? 0,
            CreatedSentenceCount = enrichmentResult?.CreatedSentenceCount ?? 0,
            CreatedSentenceTranslationCount = enrichmentResult?.CreatedSentenceTranslationCount ?? 0,
            CreatedExampleLinkCount = enrichmentResult?.CreatedExampleLinkCount ?? 0,

            SampleItems = enrichmentResult?.SampleItems
                .Select(ToSampleResponse)
                .ToArray()
                ?? Array.Empty<EnrichTatoebaExampleSentenceSampleResponse>(),

            EnrichmentMessages = enrichmentResult?.Messages
                .Select(ToMessageResponse)
                .ToArray()
                ?? Array.Empty<EnrichTatoebaExampleSentenceMessageResponse>()
        };
    }

    /// <summary>
    /// Enrichment sample item modelini response DTO'ya çevirir.
    /// </summary>
    private static EnrichTatoebaExampleSentenceSampleResponse ToSampleResponse(
        ExampleSentenceEnrichmentCreatedItem item)
    {
        return new EnrichTatoebaExampleSentenceSampleResponse
        {
            LearningItemId = item.LearningItemId,
            ItemType = item.ItemType.ToString(),

            MatchedText = item.MatchedText,
            NormalizedMatchedText = item.NormalizedMatchedText,

            SourceSentenceExternalId = item.SourceSentenceExternalId,
            TargetSentenceExternalId = item.TargetSentenceExternalId,

            SourceText = item.SourceText,
            TranslatedText = item.TranslatedText,

            SentenceId = item.SentenceId,
            SentenceTranslationId = item.SentenceTranslationId,
            LearningItemExampleSentenceId = item.LearningItemExampleSentenceId,

            CreatedSentence = item.CreatedSentence,
            CreatedSentenceTranslation = item.CreatedSentenceTranslation,
            CreatedExampleLink = item.CreatedExampleLink
        };
    }

    /// <summary>
    /// Enrichment message modelini response DTO'ya çevirir.
    /// </summary>
    private static EnrichTatoebaExampleSentenceMessageResponse ToMessageResponse(
        ExampleSentenceEnrichmentMessage message)
    {
        return new EnrichTatoebaExampleSentenceMessageResponse
        {
            Type = message.Type,
            Code = message.Code,
            Message = message.Message,
            SourceSentenceExternalId = message.SourceSentenceExternalId,
            TargetSentenceExternalId = message.TargetSentenceExternalId,
            LearningItemId = message.LearningItemId
        };
    }
}