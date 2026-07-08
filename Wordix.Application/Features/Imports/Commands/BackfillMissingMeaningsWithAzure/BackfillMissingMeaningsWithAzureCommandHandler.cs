using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Interfaces.Translation;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Common.Models.Translation;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.BackfillMissingMeaningsWithAzure;

/// <summary>
/// Meaning'i olmayan Word kayıtlarını Azure Translator ile dolduran handler'dır.
/// 
/// Bu handler ne yapar?
/// - Aktif Word + LearningItem kayıtlarını tarar.
/// - Hedef dilde Meaning'i olmayanları bulur.
/// - DryRun ise sadece sayar.
/// - DryRun false ise Azure Translator ile çevirip Meaning oluşturur.
/// 
/// Bu handler ne yapmaz?
/// - Word oluşturmaz.
/// - LearningItem oluşturmaz.
/// - Kullanıcı dictionary'sine kayıt atmaz.
/// </summary>
public sealed class BackfillMissingMeaningsWithAzureCommandHandler
    : IRequestHandler<BackfillMissingMeaningsWithAzureCommand, AzureMissingMeaningBackfillResponse>
{
    private const int QueryChunkSize = 1000;

    /// <summary>
    /// Meaning.MeaningText kolon limitiyle uyumlu olmalıdır.
    /// </summary>
    private const int MaxMeaningTextLength = 200;

    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly ITranslationProvider _translationProvider;
    private readonly IImportJobService _importJobService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public BackfillMissingMeaningsWithAzureCommandHandler(
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Language> languageRepository,
        ITranslationProvider translationProvider,
        IImportJobService importJobService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _learningItemRepository = learningItemRepository
            ?? throw new ArgumentNullException(nameof(learningItemRepository));

        _wordRepository = wordRepository
            ?? throw new ArgumentNullException(nameof(wordRepository));

        _meaningRepository = meaningRepository
            ?? throw new ArgumentNullException(nameof(meaningRepository));

        _languageRepository = languageRepository
            ?? throw new ArgumentNullException(nameof(languageRepository));

        _translationProvider = translationProvider
            ?? throw new ArgumentNullException(nameof(translationProvider));

        _importJobService = importJobService
            ?? throw new ArgumentNullException(nameof(importJobService));

        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<AzureMissingMeaningBackfillResponse> Handle(
        BackfillMissingMeaningsWithAzureCommand request,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>();

        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);
        var targetLanguageCode = NormalizeLanguageCode(request.TargetLanguageCode);
        var allowedContentSources = request.AllowedContentSources.ToArray();

        var targetLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == targetLanguageCode && language.IsActive,
            cancellationToken);

        if (targetLanguage is null)
        {
            return new AzureMissingMeaningBackfillResponse
            {
                DryRun = request.DryRun,
                MissingWordCount = 0,
                ProcessedCount = 0,
                WouldCreateCount = 0,
                CreatedCount = 0,
                FailedCount = 1,
                Messages = new[]
                {
                    $"Hedef dil bulunamadı veya aktif değil. Code: {targetLanguageCode}"
                }
            };
        }

        var importJob = await _importJobService.StartAsync(
            jobType: ImportJobType.MeaningEnrichment,
            sourceName: ImportConstants.ProviderNames.AzureTranslator,
            sourceVersion: null,
            sourceFileName: null,
            dryRun: request.DryRun,
            triggeredByKeycloakUserId: GetTriggeredByKeycloakUserId(),
            cancellationToken: cancellationToken);

        try
        {
            var missingWords = await LoadMissingWordsAsync(
                targetLanguage.Id,
                allowedContentSources,
                cancellationToken);

            var missingWordCount = missingWords.Count;

            var selectedWords = missingWords
                .Take(request.MaxItems)
                .ToArray();

            if (request.DryRun)
            {
                AddMessage(
                    messages,
                    $"DryRun aktif. Azure'a istek atılmadı. MissingWordCount: {missingWordCount}, ProcessedCount: {selectedWords.Length}.",
                    request.MaxMessages);

                await _importJobService.CompleteAsync(
                    importJob.Id,
                    new ImportJobCompletionRequest
                    {
                        TotalRows = missingWordCount,
                        ProcessedRows = selectedWords.Length,
                        CreatedCount = 0,
                        UpdatedCount = 0,
                        SkippedCount = Math.Max(0, missingWordCount - selectedWords.Length),
                        FailedCount = 0,
                        SummaryMessage = $"Azure missing meaning backfill dry-run completed. Missing: {missingWordCount}, WouldCreate: {selectedWords.Length}."
                    },
                    cancellationToken);

                return new AzureMissingMeaningBackfillResponse
                {
                    ImportJobId = importJob.Id,
                    DryRun = true,
                    MissingWordCount = missingWordCount,
                    ProcessedCount = selectedWords.Length,
                    WouldCreateCount = selectedWords.Length,
                    CreatedCount = 0,
                    FailedCount = 0,
                    SkippedLongTranslationCount = 0,
                    Messages = messages
                };
            }

            var createdCount = 0;
            var failedCount = 0;
            var skippedLongTranslationCount = 0;
            var pendingMeanings = new List<Meaning>();

            foreach (var wordCandidate in selectedWords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var translationInput = BuildTranslationInput(wordCandidate.WordText);

                if (string.IsNullOrWhiteSpace(translationInput))
                {
                    failedCount++;
                    AddMessage(
                        messages,
                        $"Translation input boş olduğu için atlandı. Word: {wordCandidate.WordText}",
                        request.MaxMessages);
                    continue;
                }

                var translationResult = await _translationProvider.TranslateAsync(
                    new TranslationRequest
                    {
                        Text = translationInput,
                        SourceLanguageCode = sourceLanguageCode,
                        TargetLanguageCode = targetLanguageCode,
                        Purpose = "missing-word-meaning-azure-backfill",
                        Context = "Translate this English word into a concise Turkish dictionary meaning."
                    },
                    cancellationToken);

                if (!translationResult.Succeeded ||
                    string.IsNullOrWhiteSpace(translationResult.TranslatedText))
                {
                    failedCount++;
                    AddMessage(
                        messages,
                        $"Azure translation başarısız. Word: {wordCandidate.WordText}, Error: {translationResult.ErrorMessage}",
                        request.MaxMessages);
                    continue;
                }

                var meaningText = NormalizeDisplayText(translationResult.TranslatedText);

                if (meaningText.Length > MaxMeaningTextLength)
                {
                    skippedLongTranslationCount++;
                    AddMessage(
                        messages,
                        $"Azure translation kolon limitini aştığı için atlandı. Word: {wordCandidate.WordText}, Length: {meaningText.Length}",
                        request.MaxMessages);
                    continue;
                }

                var meaning = new Meaning(
                    wordCandidate.LearningItemId,
                    targetLanguage.Id,
                    meaningText,
                    shortDefinition: null,
                    partOfSpeech: wordCandidate.PartOfSpeech,
                    category: null,
                    isPrimary: true,
                    displayOrder: 0,
                    contentSource: ContentSource.AzureTranslator,
                    qualityStatus: ContentQualityStatus.AutoGenerated,
                    sourceProvider: ImportConstants.ProviderNames.AzureTranslator,
                    license: null);

                pendingMeanings.Add(meaning);
                createdCount++;

                if (pendingMeanings.Count >= request.BatchSize)
                {
                    await SaveBatchAsync(
                        pendingMeanings,
                        cancellationToken);
                }
            }

            if (pendingMeanings.Count > 0)
            {
                await SaveBatchAsync(
                    pendingMeanings,
                    cancellationToken);
            }

            await _importJobService.CompleteAsync(
                importJob.Id,
                new ImportJobCompletionRequest
                {
                    TotalRows = missingWordCount,
                    ProcessedRows = selectedWords.Length,
                    CreatedCount = createdCount,
                    UpdatedCount = 0,
                    SkippedCount =
                        Math.Max(0, missingWordCount - selectedWords.Length) +
                        skippedLongTranslationCount,
                    FailedCount = failedCount,
                    SummaryMessage =
                        $"Azure missing meaning backfill completed. Missing: {missingWordCount}, Processed: {selectedWords.Length}, Created: {createdCount}, Failed: {failedCount}."
                },
                cancellationToken);

            AddMessage(
                messages,
                $"Azure missing meaning backfill tamamlandı. Created: {createdCount}, Failed: {failedCount}.",
                request.MaxMessages);

            return new AzureMissingMeaningBackfillResponse
            {
                ImportJobId = importJob.Id,
                DryRun = false,
                MissingWordCount = missingWordCount,
                ProcessedCount = selectedWords.Length,
                WouldCreateCount = 0,
                CreatedCount = createdCount,
                FailedCount = failedCount,
                SkippedLongTranslationCount = skippedLongTranslationCount,
                Messages = messages
            };
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
    /// Meaning'i olmayan aktif Word kayıtlarını yükler.
    /// </summary>
    private async Task<IReadOnlyCollection<MissingWordCandidate>> LoadMissingWordsAsync(
        Guid targetLanguageId,
        IReadOnlyCollection<ContentSource> allowedContentSources,
        CancellationToken cancellationToken)
    {
        var learningItems = await _learningItemRepository.ListAsync(
            item =>
                item.IsActive &&
                item.ItemType == LearningItemType.Word &&
                allowedContentSources.Contains(item.ContentSource),
            cancellationToken);

        var learningItemIds = learningItems
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        if (learningItemIds.Length == 0)
        {
            return Array.Empty<MissingWordCandidate>();
        }

        var words = new List<Word>();
        var learningItemIdsWithMeaning = new HashSet<Guid>();

        foreach (var chunk in learningItemIds.Chunk(QueryChunkSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunkArray = chunk.ToArray();

            var chunkWords = await _wordRepository.ListAsync(
                word => chunkArray.Contains(word.LearningItemId),
                cancellationToken);

            words.AddRange(chunkWords);

            var chunkMeanings = await _meaningRepository.ListAsync(
                meaning =>
                    chunkArray.Contains(meaning.LearningItemId) &&
                    meaning.TargetLanguageId == targetLanguageId,
                cancellationToken);

            foreach (var meaning in chunkMeanings)
            {
                learningItemIdsWithMeaning.Add(meaning.LearningItemId);
            }
        }

        var learningItemById = learningItems.ToDictionary(item => item.Id);

        return words
            .Where(word => !learningItemIdsWithMeaning.Contains(word.LearningItemId))
            .OrderBy(word =>
            {
                if (!learningItemById.TryGetValue(word.LearningItemId, out var item))
                {
                    return int.MaxValue;
                }

                return (int)item.CefrLevel;
            })
            .ThenBy(word => word.NormalizedText)
            .Select(word =>
            {
                learningItemById.TryGetValue(
                    word.LearningItemId,
                    out var learningItem);

                return new MissingWordCandidate
                {
                    LearningItemId = word.LearningItemId,
                    WordText = word.Text,
                    NormalizedText = word.NormalizedText,
                    PartOfSpeech = word.PartOfSpeech,
                    ContentSource = learningItem?.ContentSource ?? ContentSource.Unknown,
                    CefrLevel = learningItem?.CefrLevel ?? CefrLevel.Unknown,
                    DifficultyGroup = learningItem?.DifficultyGroup ?? DifficultyGroup.Unknown
                };
            })
            .ToArray();
    }

    private async Task SaveBatchAsync(
        List<Meaning> pendingMeanings,
        CancellationToken cancellationToken)
    {
        await _meaningRepository.AddRangeAsync(
            pendingMeanings,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        pendingMeanings.Clear();
    }

    /// <summary>
    /// Bazı imported word metinleri slash ile alternatif içerir.
    /// Örnek:
    /// airplane/aeroplane
    /// cafe/café
    /// 
    /// Azure'a mümkün olduğunca sade bir input göndermek için ilk anlamlı parçayı seçiyoruz.
    /// </summary>
    private static string BuildTranslationInput(string wordText)
    {
        var normalized = NormalizeDisplayText(wordText);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (!normalized.Contains('/'))
        {
            return normalized;
        }

        return normalized
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(part => !string.IsNullOrWhiteSpace(part))
            ?? normalized;
    }

    private string? GetTriggeredByKeycloakUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.KeycloakUserId
            : null;
    }

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
            // Asıl exception ezilmemeli.
        }
    }

    private static void AddMessage(
        List<string> messages,
        string message,
        int maxMessages)
    {
        if (messages.Count >= maxMessages)
        {
            return;
        }

        messages.Add(message);
    }

    private static string NormalizeLanguageCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string NormalizeDisplayText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private sealed record MissingWordCandidate
    {
        public Guid LearningItemId { get; init; }

        public string WordText { get; init; } = string.Empty;

        public string NormalizedText { get; init; } = string.Empty;

        public string? PartOfSpeech { get; init; }

        public ContentSource ContentSource { get; init; }

        public CefrLevel CefrLevel { get; init; }

        public DifficultyGroup DifficultyGroup { get; init; }
    }
}