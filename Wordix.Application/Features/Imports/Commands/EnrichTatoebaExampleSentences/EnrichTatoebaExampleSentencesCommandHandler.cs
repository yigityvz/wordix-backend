using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;
using Wordix.Application.Features.Imports.Services;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.EnrichTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence enrichment command handler'ıdır.
/// 
/// Bu handler ne yapar?
/// - ImportJob kaydı oluşturur.
/// - Tatoeba parser provider'ını çalıştırır.
/// - Parser başarılıysa enrichment service'i çalıştırır.
/// - Sonucu response DTO'ya çevirir.
/// - ImportJob'u Completed veya Failed durumuna geçirir.
/// 
/// Bu handler ne yapmaz?
/// - TSV/CSV parse etmez.
/// - Word/Phrase eşleştirme algoritmasını kendi içinde yazmaz.
/// - DbContext bilmez.
/// - HTTP form-data bilmez.
/// 
/// Handler sadece use-case akışını orkestre eder.
/// </summary>
public sealed class EnrichTatoebaExampleSentencesCommandHandler
    : IRequestHandler<EnrichTatoebaExampleSentencesCommand, EnrichTatoebaExampleSentencesResponse>
{
    private readonly IExampleSentenceImportProvider _exampleSentenceImportProvider;
    private readonly IExampleSentenceEnrichmentService _exampleSentenceEnrichmentService;
    private readonly IImportJobService _importJobService;
    private readonly ICurrentUserService _currentUserService;

    public EnrichTatoebaExampleSentencesCommandHandler(
        IExampleSentenceImportProvider exampleSentenceImportProvider,
        IExampleSentenceEnrichmentService exampleSentenceEnrichmentService,
        IImportJobService importJobService,
        ICurrentUserService currentUserService)
    {
        _exampleSentenceImportProvider = exampleSentenceImportProvider
            ?? throw new ArgumentNullException(nameof(exampleSentenceImportProvider));

        _exampleSentenceEnrichmentService = exampleSentenceEnrichmentService
            ?? throw new ArgumentNullException(nameof(exampleSentenceEnrichmentService));

        _importJobService = importJobService
            ?? throw new ArgumentNullException(nameof(importJobService));

        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Parser + enrichment akışını çalıştırır.
    /// </summary>
    public async Task<EnrichTatoebaExampleSentencesResponse> Handle(
        EnrichTatoebaExampleSentencesCommand request,
        CancellationToken cancellationToken)
    {
        var importJob = await _importJobService.StartAsync(
            jobType: ImportJobType.ExampleSentenceEnrichment,
            sourceName: ImportConstants.ProviderNames.Tatoeba,
            sourceVersion: null,
            sourceFileName: BuildSourceFileNameSummary(
                request.SourceFileName,
                request.TargetFileName,
                request.LinksFileName),
            dryRun: request.DryRun,
            triggeredByKeycloakUserId: GetTriggeredByKeycloakUserId(),
            cancellationToken: cancellationToken);

        try
        {
            // 1. Önce Tatoeba provider request modelini hazırlıyoruz.
            var providerRequest = new ExampleSentenceImportProviderRequest
            {
                SourceSentencesStream = request.SourceSentencesStream,
                TargetSentencesStream = request.TargetSentencesStream,
                LinksStream = request.LinksStream,

                SourceLanguageCode = request.SourceLanguageCode,
                TargetLanguageCode = request.TargetLanguageCode,

                SourceProviderLanguageCode = request.SourceProviderLanguageCode,
                TargetProviderLanguageCode = request.TargetProviderLanguageCode,

                MaxRows = request.ParserMaxRows,
                MaxMessages = request.MaxMessages,
                License = request.License
            };

            // 2. Tatoeba parser'ı çalıştırıyoruz.
            var providerResult = await _exampleSentenceImportProvider.LoadAsync(
                providerRequest,
                cancellationToken);

            // Provider tamamen başarısızsa enrichment çalıştırmıyoruz.
            // Örneğin üç dosyadan biri yoksa veya okunamıyorsa burada dururuz.
            if (!providerResult.Succeeded)
            {
                await _importJobService.FailAsync(
                    importJob.Id,
                    new ImportJobFailureRequest
                    {
                        ErrorMessage = BuildProviderFailureMessage(providerResult.Errors),
                        TotalRows = providerResult.Rows.Count,
                        ProcessedRows = 0,
                        CreatedCount = 0,
                        UpdatedCount = 0,
                        SkippedCount = 0,
                        FailedCount = providerResult.Errors.Count
                    },
                    cancellationToken);

                return EnrichTatoebaExampleSentencesMapper.ToResponse(
                    providerResult,
                    enrichmentResult: null,
                    dryRun: request.DryRun,
                    importJobId: importJob.Id);
            }

            // Parser başarılı ama hiç row üretmemişse enrichment service'i çalıştırmak anlamsızdır.
            // Bu durumda job teknik olarak başarılı tamamlandı kabul edilir.
            if (providerResult.Rows.Count == 0)
            {
                var emptyEnrichmentResult = ExampleSentenceEnrichmentResult.Success(
                    dryRun: request.DryRun,
                    totalInputRows: 0,
                    candidateLearningItemCount: 0,
                    matchedRowCount: 0,
                    notMatchedRowCount: 0,
                    skippedCount: 0,
                    wouldCreateCount: 0,
                    createdSentenceCount: 0,
                    createdSentenceTranslationCount: 0,
                    createdExampleLinkCount: 0,
                    sampleItems: Array.Empty<ExampleSentenceEnrichmentCreatedItem>(),
                    messages: Array.Empty<ExampleSentenceEnrichmentMessage>());

                await _importJobService.CompleteAsync(
                    importJob.Id,
                    new ImportJobCompletionRequest
                    {
                        TotalRows = 0,
                        ProcessedRows = 0,
                        CreatedCount = 0,
                        UpdatedCount = 0,
                        SkippedCount = 0,
                        FailedCount = 0,
                        SummaryMessage = BuildCompletionSummaryMessage(
                            request.DryRun,
                            providerParsedRows: 0,
                            createdSentenceCount: 0,
                            createdSentenceTranslationCount: 0,
                            createdExampleLinkCount: 0,
                            wouldCreateCount: 0,
                            failedCount: 0)
                    },
                    cancellationToken);

                return EnrichTatoebaExampleSentencesMapper.ToResponse(
                    providerResult,
                    emptyEnrichmentResult,
                    request.DryRun,
                    importJob.Id);
            }

            // 3. Parser'dan gelen rowları enrichment service'e gönderiyoruz.
            var enrichmentRequest = new ExampleSentenceEnrichmentRequest
            {
                Rows = providerResult.Rows,

                SourceLanguageCode = request.SourceLanguageCode,
                TargetLanguageCode = request.TargetLanguageCode,

                AllowedItemTypes = request.AllowedItemTypes,
                AllowedContentSources = request.AllowedContentSources,

                DryRun = request.DryRun,
                MaxExamplesPerLearningItem = request.MaxExamplesPerLearningItem,
                MaxCreatedItems = request.MaxCreatedItems,
                MaxMessages = request.MaxMessages,
                SampleSize = request.SampleSize
            };

            var enrichmentResult = await _exampleSentenceEnrichmentService.EnrichAsync(
                enrichmentRequest,
                cancellationToken);

            await _importJobService.CompleteAsync(
                importJob.Id,
                new ImportJobCompletionRequest
                {
                    TotalRows = providerResult.Rows.Count,
                    ProcessedRows = enrichmentResult.TotalInputRows,

                    // ImportJobs.CreatedCount tek kolon olduğu için burada ana iş sonucunu temsil eden
                    // CreatedExampleLinkCount değerini yazıyoruz.
                    // Detaylı sentence/translation sayıları response içinde zaten dönüyor.
                    CreatedCount = enrichmentResult.CreatedExampleLinkCount,

                    UpdatedCount = 0,
                    SkippedCount = enrichmentResult.SkippedCount,
                    FailedCount = 0,
                    SummaryMessage = BuildCompletionSummaryMessage(
                        request.DryRun,
                        providerResult.Rows.Count,
                        enrichmentResult.CreatedSentenceCount,
                        enrichmentResult.CreatedSentenceTranslationCount,
                        enrichmentResult.CreatedExampleLinkCount,
                        enrichmentResult.WouldCreateCount,
                        failedCount: 0)
                },
                cancellationToken);

            // 4. Provider + enrichment sonucunu API response DTO'ya çeviriyoruz.
            return EnrichTatoebaExampleSentencesMapper.ToResponse(
                providerResult,
                enrichmentResult,
                request.DryRun,
                importJob.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await TryFailImportJobAsync(
                importJob.Id,
                exception,
                cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Import job'a yazılacak kullanıcı id değerini güvenli şekilde döndürür.
    /// 
    /// Admin endpointleri normalde authenticated çalışır.
    /// Ama handler testlerinde current user olmayabilir.
    /// Bu yüzden burada exception fırlatmak yerine null dönebilen güvenli property kullanıyoruz.
    /// </summary>
    private string? GetTriggeredByKeycloakUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.KeycloakUserId
            : null;
    }

    /// <summary>
    /// Tatoeba enrichment üç dosya aldığı için ImportJob.SourceFileName alanına kısa bir özet yazıyoruz.
    /// 
    /// SourceFileName kolonu 255 karakter olduğu için sonucu limitliyoruz.
    /// </summary>
    private static string? BuildSourceFileNameSummary(
        string? sourceFileName,
        string? targetFileName,
        string? linksFileName)
    {
        var parts = new[]
            {
                sourceFileName,
                targetFileName,
                linksFileName
            }
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .Select(fileName => fileName!.Trim())
            .ToArray();

        if (parts.Length == 0)
        {
            return null;
        }

        var summary = string.Join(" | ", parts);

        return summary.Length <= 255
            ? summary
            : summary[..255];
    }

    /// <summary>
    /// Provider tamamen başarısız olduğunda ImportJob.ErrorMessage için güvenli mesaj üretir.
    /// </summary>
    private static string BuildProviderFailureMessage(
        IReadOnlyCollection<string> errors)
    {
        if (errors.Count == 0)
        {
            return "Tatoeba example sentence import provider failed.";
        }

        var message = string.Join(" | ", errors.Take(5));

        return message.Length <= 1000
            ? message
            : message[..1000];
    }

    /// <summary>
    /// Completed job için okunabilir summary mesajı üretir.
    /// </summary>
    private static string BuildCompletionSummaryMessage(
        bool dryRun,
        int providerParsedRows,
        int createdSentenceCount,
        int createdSentenceTranslationCount,
        int createdExampleLinkCount,
        int wouldCreateCount,
        int failedCount)
    {
        var message =
            $"Tatoeba example sentence enrichment completed. DryRun: {dryRun}, " +
            $"ProviderParsedRows: {providerParsedRows}, " +
            $"CreatedSentences: {createdSentenceCount}, " +
            $"CreatedTranslations: {createdSentenceTranslationCount}, " +
            $"CreatedExampleLinks: {createdExampleLinkCount}, " +
            $"WouldCreate: {wouldCreateCount}, Failed: {failedCount}.";

        return message.Length <= 1000
            ? message
            : message[..1000];
    }

    /// <summary>
    /// Beklenmeyen exception durumunda ImportJob kaydını Failed yapmaya çalışır.
    /// 
    /// Bu method best-effort çalışır:
    /// - Asıl exception'ı ezmez.
    /// - FailAsync hata verirse onu yutar.
    /// - Ana handler orijinal exception'ı tekrar fırlatır.
    /// </summary>
    private async Task TryFailImportJobAsync(
        Guid importJobId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var errorMessage = exception.Message;

            if (errorMessage.Length > 1000)
            {
                errorMessage = errorMessage[..1000];
            }

            await _importJobService.FailAsync(
                importJobId,
                new ImportJobFailureRequest
                {
                    ErrorMessage = errorMessage,
                    TotalRows = 0,
                    ProcessedRows = 0,
                    CreatedCount = 0,
                    UpdatedCount = 0,
                    SkippedCount = 0,
                    FailedCount = 1
                },
                cancellationToken);
        }
        catch
        {
            // Job fail update hatası, asıl enrichment exception'ını ezmemeli.
        }
    }
}