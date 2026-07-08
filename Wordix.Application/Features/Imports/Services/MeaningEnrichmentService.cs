using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Kaikki/Wiktionary parser'dan gelen meaning satırlarını
/// database'deki mevcut Word/LearningItem kayıtlarına bağlayan application service.
/// 
/// Bu service ne yapar?
/// - Parser'dan gelen MeaningImportRow listesini alır.
/// - DB'de karşılığı olan Word/LearningItem kayıtlarını bulur.
/// - Mevcut meaning varsa tekrar eklemez.
/// - DryRun aktifse sadece sayım yapar.
/// - DryRun false ise Meaning entity oluşturur ve batch save yapar.
/// 
/// Bu service neden Application katmanında?
/// - Enrichment bir use-case davranışıdır.
/// - DbContext bilmez.
/// - Repository interface'leri ve UnitOfWork üzerinden çalışır.
/// </summary>
public sealed class MeaningEnrichmentService : IMeaningEnrichmentService
{
    /// <summary>
    /// SQL Server IN parametre limitine yaklaşmamak için sorguları küçük parçalara bölüyoruz.
    /// </summary>
    private const int QueryChunkSize = 1000;

    /// <summary>
    /// Meaning.MeaningText kolon limitine uymak için provider'dan gelen anlam metinlerini sınırlarız.
    /// 
    /// Not:
    /// Bu değer MeaningConfiguration içindeki HasMaxLength değeriyle uyumlu tutulmalıdır.
    /// </summary>
    private const int MaxMeaningTextLength = 200;

    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MeaningEnrichmentService(
        ILearningItemRepository learningItemRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Language> languageRepository,
        IUnitOfWork unitOfWork)
    {
        _learningItemRepository = learningItemRepository;
        _meaningRepository = meaningRepository;
        _languageRepository = languageRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Meaning enrichment ana akışıdır.
    /// </summary>
    public async Task<MeaningEnrichmentResult> EnrichAsync(
        MeaningEnrichmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<string>();

        var totalInputRows = request.MeaningRows.Count;
        var failedCount = 0;
        var notMatchedCount = 0;
        var matchedWordCount = 0;
        var skippedPhraseCandidateCount = 0;
        var skippedExistingMeaningCount = 0;
        var skippedDuplicateInputCount = 0;
        var wouldCreateCount = 0;
        var createdCount = 0;

        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);
        var targetLanguageCode = NormalizeLanguageCode(request.TargetLanguageCode);
        var batchSize = ResolveBatchSize(request.BatchSize);

        if (request.MeaningRows.Count == 0)
        {
            AddMessage(messages, "Meaning enrichment için input row bulunamadı.", request.MaxMessages);

            return BuildResult(
                request.DryRun,
                totalInputRows,
                matchedWordCount,
                notMatchedCount,
                skippedPhraseCandidateCount,
                skippedExistingMeaningCount,
                skippedDuplicateInputCount,
                wouldCreateCount,
                createdCount,
                failedCount,
                messages);
        }

        var targetLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == targetLanguageCode && language.IsActive,
            cancellationToken);

        if (targetLanguage is null)
        {
            AddMessage(
                messages,
                $"Hedef dil bulunamadı veya aktif değil. Code: {targetLanguageCode}",
                request.MaxMessages);

            failedCount = request.MeaningRows.Count;

            return BuildResult(
                request.DryRun,
                totalInputRows,
                matchedWordCount,
                notMatchedCount,
                skippedPhraseCandidateCount,
                skippedExistingMeaningCount,
                skippedDuplicateInputCount,
                wouldCreateCount,
                createdCount,
                failedCount,
                messages);
        }

        var normalizedRows = NormalizeAndFilterRows(
            request,
            sourceLanguageCode,
            targetLanguageCode,
            messages,
            ref failedCount,
            ref skippedPhraseCandidateCount,
            ref skippedDuplicateInputCount);

        if (normalizedRows.Count == 0)
        {
            AddMessage(messages, "Filtrelerden sonra enrich edilebilir meaning row kalmadı.", request.MaxMessages);

            return BuildResult(
                request.DryRun,
                totalInputRows,
                matchedWordCount,
                notMatchedCount,
                skippedPhraseCandidateCount,
                skippedExistingMeaningCount,
                skippedDuplicateInputCount,
                wouldCreateCount,
                createdCount,
                failedCount,
                messages);
        }

        var sourceTexts = normalizedRows
            .Select(row => row.NormalizedSourceText)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var wordMatches = await LoadWordMatchesAsync(
            sourceLanguageCode,
            sourceTexts,
            request.AllowedLearningItemSources,
            cancellationToken);

        var wordMatchByNormalizedText = wordMatches
            .GroupBy(match => match.NormalizedText, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        var matchedLearningItemIds = wordMatches
            .Select(match => match.LearningItemId)
            .Distinct()
            .ToArray();

        var existingMeaningKeys = await LoadExistingMeaningKeysAsync(
            matchedLearningItemIds,
            targetLanguage.Id,
            cancellationToken);

        var pendingMeanings = new List<Meaning>();

        // Aynı enrichment çalışması içinde yeni eklenen meaningleri de takip ediyoruz.
        // Böylece aynı batch içinde duplicate oluşmasını engelliyoruz.
        var pendingMeaningKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var displayOrderByLearningItemId = await LoadNextDisplayOrdersAsync(
            matchedLearningItemIds,
            targetLanguage.Id,
            cancellationToken);

        foreach (var row in normalizedRows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!wordMatchByNormalizedText.TryGetValue(
                    row.NormalizedSourceText,
                    out var wordMatch))
            {
                notMatchedCount++;

                AddMessage(
                    messages,
                    $"DB'de eşleşen imported word bulunamadı. SourceText: {row.SourceText}",
                    request.MaxMessages);

                continue;
            }

            matchedWordCount++;

            var meaningKey = BuildMeaningKey(
                wordMatch.LearningItemId,
                targetLanguage.Id,
                row.NormalizedMeaningText);

            if (existingMeaningKeys.Contains(meaningKey) ||
                pendingMeaningKeys.Contains(meaningKey))
            {
                skippedExistingMeaningCount++;

                continue;
            }

            if (request.DryRun)
            {
                wouldCreateCount++;
                pendingMeaningKeys.Add(meaningKey);
                continue;
            }

            var nextDisplayOrder = GetAndIncreaseDisplayOrder(
                displayOrderByLearningItemId,
                wordMatch.LearningItemId);

            var isPrimary = nextDisplayOrder == 0;

            var meaning = new Meaning(
                wordMatch.LearningItemId,
                targetLanguage.Id,
                row.MeaningText,
                row.ShortDefinition,
                row.PartOfSpeech,
                row.Category,
                isPrimary,
                nextDisplayOrder,
                row.ContentSource,
                row.QualityStatus,
                row.SourceProvider,
                row.License);

            pendingMeanings.Add(meaning);
            pendingMeaningKeys.Add(meaningKey);
            createdCount++;

            if (pendingMeanings.Count >= batchSize)
            {
                await SaveBatchAsync(pendingMeanings, cancellationToken);
            }
        }

        if (!request.DryRun && pendingMeanings.Count > 0)
        {
            await SaveBatchAsync(pendingMeanings, cancellationToken);
        }

        AddMessage(
            messages,
            $"Meaning enrichment tamamlandı. DryRun: {request.DryRun}. Created: {createdCount}. WouldCreate: {wouldCreateCount}.",
            request.MaxMessages);

        return BuildResult(
            request.DryRun,
            totalInputRows,
            matchedWordCount,
            notMatchedCount,
            skippedPhraseCandidateCount,
            skippedExistingMeaningCount,
            skippedDuplicateInputCount,
            wouldCreateCount,
            createdCount,
            failedCount,
            messages);
    }

    /// <summary>
    /// Input row'ları normalize eder ve bu fazda kullanılmayacak satırları ayıklar.
    /// </summary>
    private static IReadOnlyCollection<MeaningImportRow> NormalizeAndFilterRows(
        MeaningEnrichmentRequest request,
        string sourceLanguageCode,
        string targetLanguageCode,
        List<string> messages,
        ref int failedCount,
        ref int skippedPhraseCandidateCount,
        ref int skippedDuplicateInputCount)
    {
        var result = new List<MeaningImportRow>();
        var seenInputKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in request.MeaningRows)
        {
            var normalizedSourceLanguageCode = NormalizeLanguageCode(row.SourceLanguageCode);
            var normalizedTargetLanguageCode = NormalizeLanguageCode(row.TargetLanguageCode);
            var normalizedSourceText = NormalizeText(row.NormalizedSourceText);
            var normalizedMeaningText = NormalizeText(row.NormalizedMeaningText);

            if (!string.Equals(
                    normalizedSourceLanguageCode,
                    sourceLanguageCode,
                    StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(
                    normalizedTargetLanguageCode,
                    targetLanguageCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!request.IncludePhraseCandidates && row.IsPhraseCandidate)
            {
                skippedPhraseCandidateCount++;
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.SourceText) ||
                string.IsNullOrWhiteSpace(normalizedSourceText) ||
                string.IsNullOrWhiteSpace(row.MeaningText) ||
                string.IsNullOrWhiteSpace(normalizedMeaningText))
            {
                failedCount++;
                AddMessage(
                    messages,
                    $"Geçersiz meaning row atlandı. SourceText: {row.SourceText}, MeaningText: {row.MeaningText}",
                    request.MaxMessages);
                continue;
            }

            if (row.MeaningText.Trim().Length > MaxMeaningTextLength)
            {
                failedCount++;

                AddMessage(
                    messages,
                    $"MeaningText kolon limitini aştığı için row atlandı. SourceText: {row.SourceText}, Length: {row.MeaningText.Trim().Length}",
                    request.MaxMessages);

                continue;
            }

            var inputKey = string.Join(
                "|",
                normalizedSourceText,
                NormalizeOptionalText(row.PartOfSpeech) ?? string.Empty,
                normalizedMeaningText,
                normalizedTargetLanguageCode);

            if (!seenInputKeys.Add(inputKey))
            {
                skippedDuplicateInputCount++;
                continue;
            }

            result.Add(row with
            {
                NormalizedSourceText = normalizedSourceText,
                NormalizedMeaningText = normalizedMeaningText,
                SourceLanguageCode = sourceLanguageCode,
                TargetLanguageCode = targetLanguageCode
            });
        }

        return result;
    }

    /// <summary>
    /// Word eşleşmelerini chunk'lı şekilde repository'den çeker.
    /// 
    /// Neden chunk?
    /// - SQL Server'da çok büyük IN sorguları parametre limitine takılabilir.
    /// - 8000+ kelime için tek sorgu yerine kontrollü parçalara bölmek daha güvenlidir.
    /// </summary>
    private async Task<IReadOnlyCollection<MeaningEnrichmentWordMatch>> LoadWordMatchesAsync(
        string sourceLanguageCode,
        IReadOnlyCollection<string> sourceTexts,
        IReadOnlyCollection<ContentSource> allowedSources,
        CancellationToken cancellationToken)
    {
        var allMatches = new List<MeaningEnrichmentWordMatch>();

        foreach (var chunk in sourceTexts.Chunk(QueryChunkSize))
        {
            var matches = await _learningItemRepository.GetMeaningEnrichmentWordMatchesAsync(
                sourceLanguageCode,
                chunk,
                allowedSources,
                cancellationToken);

            allMatches.AddRange(matches);
        }

        return allMatches;
    }

    /// <summary>
    /// Mevcut meaning kayıtlarının key'lerini yükler.
    /// 
    /// Böylece aynı meaning daha önce eklenmişse tekrar eklemeyiz.
    /// </summary>
    private async Task<HashSet<string>> LoadExistingMeaningKeysAsync(
        IReadOnlyCollection<Guid> learningItemIds,
        Guid targetLanguageId,
        CancellationToken cancellationToken)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var chunk in learningItemIds.Chunk(QueryChunkSize))
        {
            var chunkArray = chunk.ToArray();

            var existingMeanings = await _meaningRepository.ListAsync(
                meaning => chunkArray.Contains(meaning.LearningItemId) &&
                           meaning.TargetLanguageId == targetLanguageId,
                cancellationToken);

            foreach (var meaning in existingMeanings)
            {
                var key = BuildMeaningKey(
                    meaning.LearningItemId,
                    meaning.TargetLanguageId,
                    NormalizeText(meaning.MeaningText));

                result.Add(key);
            }
        }

        return result;
    }

    /// <summary>
    /// Her LearningItem için sıradaki DisplayOrder değerini hesaplar.
    /// 
    /// Eğer bir item'ın hiç meaning'i yoksa sıradaki display order 0 olur.
    /// Bu durumda eklenecek ilk meaning primary yapılır.
    /// </summary>
    private async Task<Dictionary<Guid, int>> LoadNextDisplayOrdersAsync(
        IReadOnlyCollection<Guid> learningItemIds,
        Guid targetLanguageId,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, int>();

        foreach (var learningItemId in learningItemIds)
        {
            result[learningItemId] = 0;
        }

        foreach (var chunk in learningItemIds.Chunk(QueryChunkSize))
        {
            var chunkArray = chunk.ToArray();

            var existingMeanings = await _meaningRepository.ListAsync(
                meaning => chunkArray.Contains(meaning.LearningItemId) &&
                           meaning.TargetLanguageId == targetLanguageId,
                cancellationToken);

            foreach (var group in existingMeanings.GroupBy(meaning => meaning.LearningItemId))
            {
                result[group.Key] = group.Max(meaning => meaning.DisplayOrder) + 1;
            }
        }

        return result;
    }

    /// <summary>
    /// İlgili LearningItem için display order değerini döner ve bir artırır.
    /// </summary>
    private static int GetAndIncreaseDisplayOrder(
        Dictionary<Guid, int> displayOrderByLearningItemId,
        Guid learningItemId)
    {
        if (!displayOrderByLearningItemId.TryGetValue(
                learningItemId,
                out var currentDisplayOrder))
        {
            currentDisplayOrder = 0;
        }

        displayOrderByLearningItemId[learningItemId] = currentDisplayOrder + 1;

        return currentDisplayOrder;
    }

    /// <summary>
    /// Meaning batch'ini kaydeder.
    /// </summary>
    private async Task SaveBatchAsync(
        List<Meaning> pendingMeanings,
        CancellationToken cancellationToken)
    {
        await _meaningRepository.AddRangeAsync(pendingMeanings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        pendingMeanings.Clear();
    }

    /// <summary>
    /// Batch size değerini güvenli hale getirir.
    /// </summary>
    private static int ResolveBatchSize(int batchSize)
    {
        if (batchSize <= 0)
        {
            return ImportConstants.DefaultBatchSize;
        }

        return Math.Min(batchSize, ImportConstants.MaxImportBatchSize);
    }

    /// <summary>
    /// Meaning duplicate kontrol key'i üretir.
    /// </summary>
    private static string BuildMeaningKey(
        Guid learningItemId,
        Guid targetLanguageId,
        string normalizedMeaningText)
    {
        return string.Join(
            "|",
            learningItemId,
            targetLanguageId,
            normalizedMeaningText);
    }

    /// <summary>
    /// Result modelini tek yerden üretir.
    /// </summary>
    private static MeaningEnrichmentResult BuildResult(
        bool dryRun,
        int totalInputRows,
        int matchedWordCount,
        int notMatchedCount,
        int skippedPhraseCandidateCount,
        int skippedExistingMeaningCount,
        int skippedDuplicateInputCount,
        int wouldCreateCount,
        int createdCount,
        int failedCount,
        IReadOnlyCollection<string> messages)
    {
        return new MeaningEnrichmentResult
        {
            DryRun = dryRun,
            TotalInputRows = totalInputRows,
            MatchedWordCount = matchedWordCount,
            NotMatchedCount = notMatchedCount,
            SkippedPhraseCandidateCount = skippedPhraseCandidateCount,
            SkippedExistingMeaningCount = skippedExistingMeaningCount,
            SkippedDuplicateInputCount = skippedDuplicateInputCount,
            WouldCreateCount = wouldCreateCount,
            CreatedCount = createdCount,
            FailedCount = failedCount,
            Messages = messages
        };
    }

    /// <summary>
    /// Mesaj sayısını sınırlayarak response'un aşırı büyümesini engeller.
    /// </summary>
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

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}