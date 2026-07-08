using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;
using Wordix.Application.Features.Imports.Services;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.EnrichFreeDictMeanings;

/// <summary>
/// FreeDict English-Turkish meaning enrichment command handler'ı.
/// 
/// Akış:
/// 1. ImportJob kaydı oluşturur.
/// 2. TEI XML dosyasını FreeDict provider ile parse eder.
/// 3. Parser başarısızsa ImportJob'u Failed yapar ve enrichment çalıştırmadan response döner.
/// 4. Parser başarılıysa MeaningEnrichmentService'i çağırır.
/// 5. Sonucu API response DTO'ya çevirir.
/// 6. ImportJob'u Completed yapar.
/// </summary>
public sealed class EnrichFreeDictMeaningsCommandHandler
    : IRequestHandler<EnrichFreeDictMeaningsCommand, EnrichFreeDictMeaningsResponse>
{
    private readonly IFreeDictMeaningImportProvider _freeDictMeaningImportProvider;
    private readonly IMeaningEnrichmentService _meaningEnrichmentService;
    private readonly IImportJobService _importJobService;
    private readonly ICurrentUserService _currentUserService;

    public EnrichFreeDictMeaningsCommandHandler(
        IFreeDictMeaningImportProvider freeDictMeaningImportProvider,
        IMeaningEnrichmentService meaningEnrichmentService,
        IImportJobService importJobService,
        ICurrentUserService currentUserService)
    {
        _freeDictMeaningImportProvider = freeDictMeaningImportProvider
            ?? throw new ArgumentNullException(nameof(freeDictMeaningImportProvider));

        _meaningEnrichmentService = meaningEnrichmentService
            ?? throw new ArgumentNullException(nameof(meaningEnrichmentService));

        _importJobService = importJobService
            ?? throw new ArgumentNullException(nameof(importJobService));

        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// FreeDict meaning enrichment akışını yürütür.
    /// </summary>
    public async Task<EnrichFreeDictMeaningsResponse> Handle(
        EnrichFreeDictMeaningsCommand request,
        CancellationToken cancellationToken)
    {
        var importJob = await _importJobService.StartAsync(
            jobType: ImportJobType.MeaningEnrichment,
            sourceName: ImportConstants.ProviderNames.FreeDict,
            sourceVersion: null,
            sourceFileName: request.FileName,
            dryRun: request.DryRun,
            triggeredByKeycloakUserId: GetTriggeredByKeycloakUserId(),
            cancellationToken: cancellationToken);

        try
        {
            var providerRequest = new MeaningImportProviderRequest
            {
                SourceStream = request.SourceStream,
                SourceLanguageCode = request.SourceLanguageCode,
                TargetLanguageCode = request.TargetLanguageCode,
                MaxRows = request.MaxRows,
                IncludePhraseCandidates = request.IncludePhraseCandidates
            };

            var providerResult = await _freeDictMeaningImportProvider.LoadAsync(
                providerRequest,
                cancellationToken);

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

                return EnrichFreeDictMeaningsMapper.ToProviderFailureResponse(
                    providerResult,
                    request.DryRun,
                    request.MaxMessages,
                    importJob.Id);
            }

            var enrichmentRequest = new MeaningEnrichmentRequest
            {
                MeaningRows = providerResult.Rows,
                SourceLanguageCode = request.SourceLanguageCode,
                TargetLanguageCode = request.TargetLanguageCode,
                IncludePhraseCandidates = request.IncludePhraseCandidates,
                DryRun = request.DryRun,
                BatchSize = request.BatchSize,
                MaxMessages = request.MaxMessages,
                AllowedLearningItemSources = request.AllowedLearningItemSources
            };

            var enrichmentResult = await _meaningEnrichmentService.EnrichAsync(
                enrichmentRequest,
                cancellationToken);

            await _importJobService.CompleteAsync(
                importJob.Id,
                new ImportJobCompletionRequest
                {
                    TotalRows = providerResult.Rows.Count,
                    ProcessedRows = enrichmentResult.TotalInputRows,
                    CreatedCount = enrichmentResult.CreatedCount,
                    UpdatedCount = 0,
                    SkippedCount =
                        enrichmentResult.NotMatchedCount +
                        enrichmentResult.SkippedPhraseCandidateCount +
                        enrichmentResult.SkippedExistingMeaningCount +
                        enrichmentResult.SkippedDuplicateInputCount,
                    FailedCount = enrichmentResult.FailedCount,
                    SummaryMessage = BuildCompletionSummaryMessage(
                        request.DryRun,
                        providerResult.Rows.Count,
                        enrichmentResult.CreatedCount,
                        enrichmentResult.WouldCreateCount,
                        enrichmentResult.FailedCount)
                },
                cancellationToken);

            return EnrichFreeDictMeaningsMapper.ToResponse(
                providerResult,
                enrichmentResult,
                request.MaxMessages,
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
    /// </summary>
    private string? GetTriggeredByKeycloakUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.KeycloakUserId
            : null;
    }

    /// <summary>
    /// Provider tamamen başarısız olduğunda ImportJob.ErrorMessage için güvenli mesaj üretir.
    /// </summary>
    private static string BuildProviderFailureMessage(
        IReadOnlyCollection<string> errors)
    {
        if (errors.Count == 0)
        {
            return "FreeDict meaning import provider failed.";
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
        int createdCount,
        int wouldCreateCount,
        int failedCount)
    {
        var message =
            $"FreeDict meaning enrichment completed. DryRun: {dryRun}, " +
            $"ProviderParsedRows: {providerParsedRows}, Created: {createdCount}, " +
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