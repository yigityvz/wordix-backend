using System.Text;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Tatoeba gibi dış kaynaklardan parse edilen example sentence satırlarını
/// mevcut Word/Phrase LearningItem kayıtlarıyla eşleştirip
/// Sentence, SentenceTranslation ve LearningItemExampleSentence kayıtlarına dönüştüren service'tir.
/// 
/// Bu service ne yapar?
/// - Mevcut Word/Phrase LearningItem adaylarını repository üzerinden alır.
/// - Parser'dan gelen cümlelerde bu Word/Phrase metinleri geçiyor mu kontrol eder.
/// - Eşleşen cümleleri global Sentence havuzuna ekler.
/// - Türkçe çevirileri SentenceTranslation olarak ekler.
/// - Word/Phrase LearningItem ile Sentence arasında LearningItemExampleSentence bağlantısı kurar.
/// 
/// Bu service ne yapmaz?
/// - Tatoeba TSV/CSV parse etmez.
/// - HTTP dosya upload bilmez.
/// - Controller bilmez.
/// - DbContext bilmez.
/// 
/// Parser işi Infrastructure provider'dadır.
/// Veri erişimi repository abstraction'ları üzerinden yapılır.
/// </summary>
public sealed class ExampleSentenceEnrichmentService
    : IExampleSentenceEnrichmentService
{
    /// <summary>
    /// Çok kısa kelimeler örnek cümle eşleşmesinde çok fazla false-positive üretir.
    /// Örneğin "a" veya "I" neredeyse her yerde geçebilir.
    /// Bu yüzden ilk aşamada minimum 2 karakterli adayları eşleştiriyoruz.
    /// </summary>
    private const int MinimumCandidateNormalizedLength = 2;

    /// <summary>
    /// Example sentence enrichment sırasında tek kelime olarak eşleştirilmesini istemediğimiz
    /// çok genel İngilizce stop-word listesidir.
    /// 
    /// Neden gerekli?
    /// - "the", "is", "and", "to" gibi kelimeler neredeyse her cümlede geçer.
    /// - Bunları otomatik example sentence enrichment'e dahil edersek
    ///   çok düşük kaliteli bağlantılar üretiriz.
    /// 
    /// Not:
    /// Bu liste sadece single-word adaylar için uygulanır.
    /// "have to", "used to", "go on" gibi multi-word expression adayları
    /// bu listeden dolayı otomatik elenmez.
    /// </summary>
    private static readonly HashSet<string> StopWordsExcludedFromSingleWordMatching =
        new(StringComparer.OrdinalIgnoreCase)
        {
        "a",
        "an",
        "the",

        "i",
        "you",
        "he",
        "she",
        "it",
        "we",
        "they",

        "me",
        "him",
        "her",
        "us",
        "them",

        "my",
        "your",
        "his",
        "its",
        "our",
        "their",

        "am",
        "is",
        "are",
        "was",
        "were",
        "be",
        "been",
        "being",

        "do",
        "does",
        "did",

        "have",
        "has",
        "had",

        "and",
        "or",
        "but",

        "to",
        "of",
        "in",
        "on",
        "at",
        "for",
        "from",
        "by",
        "with",
        "about",
        "as",

        "this",
        "that",
        "these",
        "those",

        "there",
        "here"
        };

    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<LearningItemExampleSentence> _exampleSentenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExampleSentenceEnrichmentService(
        ILearningItemRepository learningItemRepository,
        IRepository<Language> languageRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<LearningItemExampleSentence> exampleSentenceRepository,
        IUnitOfWork unitOfWork)
    {
        _learningItemRepository = learningItemRepository;
        _languageRepository = languageRepository;
        _sentenceRepository = sentenceRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _exampleSentenceRepository = exampleSentenceRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Example sentence enrichment ana akışıdır.
    /// </summary>
    public async Task<ExampleSentenceEnrichmentResult> EnrichAsync(
        ExampleSentenceEnrichmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = new List<ExampleSentenceEnrichmentMessage>();
        var sampleItems = new List<ExampleSentenceEnrichmentCreatedItem>();

        var maxMessages = request.MaxMessages <= 0
            ? 100
            : request.MaxMessages;

        var sampleSize = request.SampleSize <= 0
            ? 20
            : request.SampleSize;

        var rows = request.Rows
            .Where(IsValidRow)
            .GroupBy(row => new
            {
                Source = NormalizeSentenceText(row.SourceText),
                Target = NormalizeSentenceText(row.TranslatedText)
            })
            .Select(group => group.First())
            .ToArray();

        if (rows.Length == 0)
        {
            AddMessage(
                messages,
                maxMessages,
                type: "Warning",
                code: "NO_VALID_EXAMPLE_SENTENCE_ROWS",
                message: "No valid example sentence rows were provided.");

            return ExampleSentenceEnrichmentResult.Success(
                dryRun: request.DryRun,
                totalInputRows: request.Rows.Count,
                candidateLearningItemCount: 0,
                matchedRowCount: 0,
                notMatchedRowCount: request.Rows.Count,
                skippedCount: request.Rows.Count,
                wouldCreateCount: 0,
                createdSentenceCount: 0,
                createdSentenceTranslationCount: 0,
                createdExampleLinkCount: 0,
                sampleItems: sampleItems,
                messages: messages);
        }

        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);
        var targetLanguageCode = NormalizeLanguageCode(request.TargetLanguageCode);

        var sourceLanguage = await _languageRepository.FirstOrDefaultAsync(
            language =>
                language.Code == sourceLanguageCode &&
                language.IsActive,
            cancellationToken);

        if (sourceLanguage is null)
        {
            AddMessage(
                messages,
                maxMessages,
                type: "Error",
                code: "SOURCE_LANGUAGE_NOT_FOUND",
                message: $"Source language '{sourceLanguageCode}' could not be found.");

            return ExampleSentenceEnrichmentResult.Failure(
                dryRun: request.DryRun,
                totalInputRows: request.Rows.Count,
                messages: messages);
        }

        var targetLanguage = await _languageRepository.FirstOrDefaultAsync(
            language =>
                language.Code == targetLanguageCode &&
                language.IsActive,
            cancellationToken);

        if (targetLanguage is null)
        {
            AddMessage(
                messages,
                maxMessages,
                type: "Error",
                code: "TARGET_LANGUAGE_NOT_FOUND",
                message: $"Target language '{targetLanguageCode}' could not be found.");

            return ExampleSentenceEnrichmentResult.Failure(
                dryRun: request.DryRun,
                totalInputRows: request.Rows.Count,
                messages: messages);
        }

        var allowedItemTypes = ResolveAllowedItemTypes(request.AllowedItemTypes);
        var allowedContentSources = ResolveAllowedContentSources(request.AllowedContentSources);

        var candidates = await _learningItemRepository.GetExampleSentenceLearningItemCandidatesAsync(
            sourceLanguageCode: sourceLanguageCode,
            allowedItemTypes: allowedItemTypes,
            allowedContentSources: allowedContentSources,
            cancellationToken: cancellationToken);

        var candidateList = candidates
            .Where(IsUsefulCandidateForExampleMatching)
            .OrderByDescending(candidate => candidate.ItemType == LearningItemType.Phrase)
            .ThenByDescending(candidate => IsMultiWordCandidate(candidate.NormalizedText))
            .ThenByDescending(candidate => candidate.NormalizedText.Length)
            .ToArray();

        if (candidateList.Length == 0)
        {
            AddMessage(
                messages,
                maxMessages,
                type: "Warning",
                code: "NO_LEARNING_ITEM_CANDIDATES",
                message: "No active Word/Phrase LearningItem candidates were found for example sentence enrichment.");

            return ExampleSentenceEnrichmentResult.Success(
                dryRun: request.DryRun,
                totalInputRows: request.Rows.Count,
                candidateLearningItemCount: 0,
                matchedRowCount: 0,
                notMatchedRowCount: rows.Length,
                skippedCount: 0,
                wouldCreateCount: 0,
                createdSentenceCount: 0,
                createdSentenceTranslationCount: 0,
                createdExampleLinkCount: 0,
                sampleItems: sampleItems,
                messages: messages);
        }

        var normalizedSourceTexts = rows
            .Select(row => NormalizeSentenceText(row.SourceText))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct()
            .ToArray();

        var existingSentences = await _sentenceRepository.ListAsync(
            sentence =>
                sentence.LanguageId == sourceLanguage.Id &&
                normalizedSourceTexts.Contains(sentence.NormalizedText),
            cancellationToken);

        var sentenceByNormalizedText = existingSentences
            .GroupBy(sentence => sentence.NormalizedText)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        var existingSentenceIds = existingSentences
            .Select(sentence => sentence.Id)
            .Distinct()
            .ToArray();

        var existingTranslations = existingSentenceIds.Length == 0
            ? Array.Empty<SentenceTranslation>()
            : await _sentenceTranslationRepository.ListAsync(
                translation =>
                    existingSentenceIds.Contains(translation.SourceSentenceId) &&
                    translation.TargetLanguageId == targetLanguage.Id,
                cancellationToken);

        var translationBySentenceAndText = existingTranslations
            .GroupBy(translation => BuildTranslationKey(
                translation.SourceSentenceId,
                targetLanguage.Id,
                translation.NormalizedTranslatedText))
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        var translationCountBySentenceId = existingTranslations
            .GroupBy(translation => translation.SourceSentenceId)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        var candidateLearningItemIds = candidateList
            .Select(candidate => candidate.LearningItemId)
            .Distinct()
            .ToArray();

        var existingExampleLinks = await _exampleSentenceRepository.ListAsync(
            example => candidateLearningItemIds.Contains(example.LearningItemId),
            cancellationToken);

        var existingExampleLinkKeys = existingExampleLinks
            .Select(example => BuildExampleLinkKey(example.LearningItemId, example.SentenceId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var exampleCountByLearningItemId = existingExampleLinks
            .GroupBy(example => example.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        // Aynı enrichment run'ı içinde aynı LearningItem + aynı sentence tekrar planlanmasın.
        // DryRun modunda SentenceId olmayabileceği için normalized source text üzerinden de takip ediyoruz.
        var plannedExampleKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var matchedRowKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var skippedCount = 0;
        var wouldCreateCount = 0;
        var createdSentenceCount = 0;
        var createdSentenceTranslationCount = 0;
        var createdExampleLinkCount = 0;
        var maxCreatedItemsReached = false;

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedSourceText = NormalizeSentenceText(row.SourceText);
            var normalizedTranslatedText = NormalizeSentenceText(row.TranslatedText);

            var matchingCandidates = FindMatchingCandidates(
                row.SourceText,
                candidateList);

            if (matchingCandidates.Count == 0)
            {
                continue;
            }

            var rowKey = BuildRowKey(row);
            matchedRowKeys.Add(rowKey);

            foreach (var candidate in matchingCandidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (request.MaxCreatedItems.HasValue &&
                    wouldCreateCount >= request.MaxCreatedItems.Value)
                {
                    maxCreatedItemsReached = true;
                    break;
                }

                var currentExampleCount = exampleCountByLearningItemId.GetValueOrDefault(
                    candidate.LearningItemId);

                if (currentExampleCount >= request.MaxExamplesPerLearningItem)
                {
                    skippedCount++;
                    continue;
                }

                var plannedExampleKey = BuildPlannedExampleKey(
                    candidate.LearningItemId,
                    normalizedSourceText);

                if (!plannedExampleKeys.Add(plannedExampleKey))
                {
                    skippedCount++;
                    continue;
                }

                sentenceByNormalizedText.TryGetValue(
                    normalizedSourceText,
                    out var sentence);

                if (sentence is not null &&
                    existingExampleLinkKeys.Contains(BuildExampleLinkKey(
                        candidate.LearningItemId,
                        sentence.Id)))
                {
                    skippedCount++;
                    continue;
                }

                var willCreateSentence = sentence is null;
                var willCreateTranslation = true;

                SentenceTranslation? sentenceTranslation = null;

                if (sentence is not null)
                {
                    var translationKey = BuildTranslationKey(
                        sentence.Id,
                        targetLanguage.Id,
                        normalizedTranslatedText);

                    willCreateTranslation = !translationBySentenceAndText.TryGetValue(
                        translationKey,
                        out sentenceTranslation);
                }

                wouldCreateCount++;

                if (request.DryRun)
                {
                    AddSampleItemIfPossible(
                        sampleItems,
                        sampleSize,
                        CreateSampleItem(
                            candidate,
                            row,
                            sentenceId: null,
                            sentenceTranslationId: null,
                            exampleLinkId: null,
                            createdSentence: willCreateSentence,
                            createdSentenceTranslation: willCreateTranslation,
                            createdExampleLink: true));

                    exampleCountByLearningItemId[candidate.LearningItemId] =
                        currentExampleCount + 1;

                    continue;
                }

                if (sentence is null)
                {
                    sentence = new Sentence(
                        learningItemId: null,
                        languageId: sourceLanguage.Id,
                        text: row.SourceText,
                        normalizedText: normalizedSourceText,
                        externalSentenceId: row.SourceSentenceExternalId,
                        sourceProvider: row.SourceProvider,
                        license: row.License,
                        contentSource: row.ContentSource,
                        qualityStatus: row.QualityStatus);

                    await _sentenceRepository.AddAsync(
                        sentence,
                        cancellationToken);

                    sentenceByNormalizedText[normalizedSourceText] = sentence;
                    createdSentenceCount++;
                }

                if (sentenceTranslation is null)
                {
                    var translationCount = translationCountBySentenceId.GetValueOrDefault(
                        sentence.Id);

                    sentenceTranslation = new SentenceTranslation(
                        sourceSentenceId: sentence.Id,
                        targetLanguageId: targetLanguage.Id,
                        translatedText: row.TranslatedText,
                        normalizedTranslatedText: normalizedTranslatedText,
                        sourceProvider: row.SourceProvider,
                        license: row.License,
                        isPrimary: translationCount == 0,
                        displayOrder: translationCount,
                        contentSource: row.ContentSource,
                        qualityStatus: row.QualityStatus);

                    await _sentenceTranslationRepository.AddAsync(
                        sentenceTranslation,
                        cancellationToken);

                    translationBySentenceAndText[BuildTranslationKey(
                        sentence.Id,
                        targetLanguage.Id,
                        normalizedTranslatedText)] = sentenceTranslation;

                    translationCountBySentenceId[sentence.Id] = translationCount + 1;
                    createdSentenceTranslationCount++;
                }

                var exampleLink = new LearningItemExampleSentence(
                    learningItemId: candidate.LearningItemId,
                    sentenceId: sentence.Id,
                    sentenceTranslationId: sentenceTranslation.Id,
                    isPrimary: currentExampleCount == 0,
                    displayOrder: currentExampleCount);

                await _exampleSentenceRepository.AddAsync(
                    exampleLink,
                    cancellationToken);

                existingExampleLinkKeys.Add(BuildExampleLinkKey(
                    candidate.LearningItemId,
                    sentence.Id));

                exampleCountByLearningItemId[candidate.LearningItemId] =
                    currentExampleCount + 1;

                createdExampleLinkCount++;

                AddSampleItemIfPossible(
                    sampleItems,
                    sampleSize,
                    CreateSampleItem(
                        candidate,
                        row,
                        sentence.Id,
                        sentenceTranslation.Id,
                        exampleLink.Id,
                        createdSentence: willCreateSentence,
                        createdSentenceTranslation: willCreateTranslation,
                        createdExampleLink: true));
            }

            if (maxCreatedItemsReached)
            {
                break;
            }
        }

        if (maxCreatedItemsReached)
        {
            AddMessage(
                messages,
                maxMessages,
                type: "Info",
                code: "MAX_CREATED_ITEMS_REACHED",
                message: $"MaxCreatedItems limit was reached. Limit: {request.MaxCreatedItems}");
        }

        if (!request.DryRun &&
            (createdSentenceCount > 0 ||
             createdSentenceTranslationCount > 0 ||
             createdExampleLinkCount > 0))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var matchedRowCount = matchedRowKeys.Count;
        var notMatchedRowCount = rows.Length - matchedRowCount;

        return ExampleSentenceEnrichmentResult.Success(
            dryRun: request.DryRun,
            totalInputRows: request.Rows.Count,
            candidateLearningItemCount: candidateList.Length,
            matchedRowCount: matchedRowCount,
            notMatchedRowCount: notMatchedRowCount,
            skippedCount: skippedCount,
            wouldCreateCount: wouldCreateCount,
            createdSentenceCount: createdSentenceCount,
            createdSentenceTranslationCount: createdSentenceTranslationCount,
            createdExampleLinkCount: createdExampleLinkCount,
            sampleItems: sampleItems,
            messages: messages);
    }

    /// <summary>
    /// Enrichment için kullanılabilir row mu kontrol eder.
    /// </summary>
    private static bool IsValidRow(ExampleSentenceImportRow row)
    {
        return !string.IsNullOrWhiteSpace(row.SourceText) &&
               !string.IsNullOrWhiteSpace(row.TranslatedText);
    }

    /// <summary>
    /// Request'ten gelen allowed item type listesini güvenli hale getirir.
    /// 
    /// Example sentence ilişkisini ilk aşamada sadece Word/Phrase için kuruyoruz.
    /// Sentence LearningItem zaten kendi başına cümle olduğu için burada kullanılmaz.
    /// </summary>
    private static IReadOnlyCollection<LearningItemType> ResolveAllowedItemTypes(
        IReadOnlyCollection<LearningItemType> requestedItemTypes)
    {
        var allowed = requestedItemTypes
            .Where(itemType =>
                itemType is LearningItemType.Word or LearningItemType.Phrase)
            .Distinct()
            .ToArray();

        return allowed.Length == 0
            ? new[] { LearningItemType.Word, LearningItemType.Phrase }
            : allowed;
    }

    /// <summary>
    /// Enrichment yapılacak content source listesini belirler.
    /// 
    /// Varsayılan olarak sistem/import kökenli daha güvenilir içerikleri zenginleştiriyoruz.
    /// AzureTranslator/UserLookup/AutoGenerated içerikleri default enrichment havuzuna almıyoruz.
    /// </summary>
    private static IReadOnlyCollection<ContentSource> ResolveAllowedContentSources(
        IReadOnlyCollection<ContentSource> requestedContentSources)
    {
        var requested = requestedContentSources
            .Where(source => source != ContentSource.Unknown)
            .Distinct()
            .ToArray();

        if (requested.Length > 0)
        {
            return requested;
        }

        return new[]
        {
            ContentSource.Manual,
            ContentSource.CefrJ,
            ContentSource.Octanove,
            ContentSource.WiktionaryKaikki
        };
    }

    /// <summary>
    /// Verilen source sentence içinde hangi Word/Phrase adaylarının geçtiğini bulur.
    /// 
    /// Eşleştirme boundary-safe yapılır.
    /// Yani:
    /// - "he" kelimesi "the" içinde eşleşmez.
    /// - "sleep" kelimesi "I have to go to sleep." içinde eşleşir.
    /// - "give up" phrase'i "Don't give up." içinde eşleşir.
    /// </summary>
    private static IReadOnlyCollection<ExampleSentenceLearningItemCandidate> FindMatchingCandidates(
        string sourceText,
        IReadOnlyCollection<ExampleSentenceLearningItemCandidate> candidates)
    {
        var normalizedSentenceForMatching = NormalizeForMatching(sourceText);

        if (string.IsNullOrWhiteSpace(normalizedSentenceForMatching))
        {
            return Array.Empty<ExampleSentenceLearningItemCandidate>();
        }

        return candidates
            .Where(candidate =>
            {
                var normalizedCandidateForMatching = NormalizeForMatching(candidate.NormalizedText);

                if (!IsUsefulCandidateForExampleMatching(candidate))
                {
                    return false;
                }

                return normalizedSentenceForMatching.Contains(
                    normalizedCandidateForMatching,
                    StringComparison.OrdinalIgnoreCase);
            })
            .ToArray();
    }

    /// <summary>
    /// Bir Word/Phrase adayının example sentence enrichment için anlamlı olup olmadığını kontrol eder.
    /// 
    /// Burada amacımız parser'ın bulduğu her şeyi değil,
    /// öğrenme açısından faydalı olabilecek adayları eşleştirmektir.
    /// </summary>
    private static bool IsUsefulCandidateForExampleMatching(
        ExampleSentenceLearningItemCandidate candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate.NormalizedText))
        {
            return false;
        }

        var normalizedCandidate = NormalizeForMatching(candidate.NormalizedText).Trim();

        if (normalizedCandidate.Length < MinimumCandidateNormalizedLength)
        {
            return false;
        }

        // Multi-word expression/phrase adaylarını stop-word filtresine sokmuyoruz.
        // Örnek:
        // - have to
        // - used to
        // - give up
        //
        // Bunlar tek tek stop-word içerse bile ifade olarak öğrenme değeri taşıyabilir.
        if (IsMultiWordCandidate(normalizedCandidate))
        {
            return true;
        }

        // Tek kelimelik çok genel kelimeler otomatik example enrichment'e alınmaz.
        // Örnek: the, is, and, to, it.
        if (StopWordsExcludedFromSingleWordMatching.Contains(normalizedCandidate))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Aday metin birden fazla kelimeden oluşuyor mu kontrol eder.
    /// </summary>
    private static bool IsMultiWordCandidate(string? normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return false;
        }

        var normalized = NormalizeForMatching(normalizedText).Trim();

        return normalized.Contains(' ');
    }

    /// <summary>
    /// Cümleleri duplicate kontrolü için normalize eder.
    /// 
    /// Noktalama işaretlerini burada koruyoruz.
    /// Çünkü Sentence.NormalizedText DB unique indexinde de cümle metninin normalize hali tutuluyor.
    /// </summary>
    private static string NormalizeSentenceText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return NormalizeDisplayText(value)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Word/Phrase eşleştirme için metni normalize eder.
    /// 
    /// Bu method noktalama işaretlerini boşluğa çevirir ve metni baş/son boşlukla sarar.
    /// Böylece kelime sınırı kontrolü yapılabilir.
    /// </summary>
    private static string NormalizeForMatching(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.Append(' ');

        var previousWasSpace = true;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character) || character == '\'')
            {
                builder.Append(character);
                previousWasSpace = false;
                continue;
            }

            if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        if (!previousWasSpace)
        {
            builder.Append(' ');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Görünen metinlerde whitespace temizliği yapar.
    /// </summary>
    private static string NormalizeDisplayText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value
            .Trim()
            .Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Dil kodunu normalize eder.
    /// </summary>
    private static string NormalizeLanguageCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Translation duplicate kontrol key'i üretir.
    /// </summary>
    private static string BuildTranslationKey(
        Guid sentenceId,
        Guid targetLanguageId,
        string normalizedTranslatedText)
    {
        return $"{sentenceId}:{targetLanguageId}:{normalizedTranslatedText}";
    }

    /// <summary>
    /// Existing example link duplicate kontrol key'i üretir.
    /// </summary>
    private static string BuildExampleLinkKey(
        Guid learningItemId,
        Guid sentenceId)
    {
        return $"{learningItemId}:{sentenceId}";
    }

    /// <summary>
    /// Aynı enrichment run'ı içinde aynı LearningItem + sentence text tekrar planlanmasın diye key üretir.
    /// </summary>
    private static string BuildPlannedExampleKey(
        Guid learningItemId,
        string normalizedSourceText)
    {
        return $"{learningItemId}:{normalizedSourceText}";
    }

    /// <summary>
    /// Row seviyesinde eşleşme sayımı için key üretir.
    /// </summary>
    private static string BuildRowKey(ExampleSentenceImportRow row)
    {
        return $"{row.SourceSentenceExternalId}:{row.TargetSentenceExternalId}:{NormalizeSentenceText(row.SourceText)}";
    }

    /// <summary>
    /// Sample item üretir.
    /// </summary>
    private static ExampleSentenceEnrichmentCreatedItem CreateSampleItem(
        ExampleSentenceLearningItemCandidate candidate,
        ExampleSentenceImportRow row,
        Guid? sentenceId,
        Guid? sentenceTranslationId,
        Guid? exampleLinkId,
        bool createdSentence,
        bool createdSentenceTranslation,
        bool createdExampleLink)
    {
        return new ExampleSentenceEnrichmentCreatedItem
        {
            LearningItemId = candidate.LearningItemId,
            ItemType = candidate.ItemType,
            MatchedText = candidate.Text,
            NormalizedMatchedText = candidate.NormalizedText,

            SourceSentenceExternalId = row.SourceSentenceExternalId,
            TargetSentenceExternalId = row.TargetSentenceExternalId,

            SourceText = row.SourceText,
            TranslatedText = row.TranslatedText,

            SentenceId = sentenceId,
            SentenceTranslationId = sentenceTranslationId,
            LearningItemExampleSentenceId = exampleLinkId,

            CreatedSentence = createdSentence,
            CreatedSentenceTranslation = createdSentenceTranslation,
            CreatedExampleLink = createdExampleLink
        };
    }

    /// <summary>
    /// Sample listesine limit dahilinde item ekler.
    /// </summary>
    private static void AddSampleItemIfPossible(
        List<ExampleSentenceEnrichmentCreatedItem> sampleItems,
        int sampleSize,
        ExampleSentenceEnrichmentCreatedItem item)
    {
        if (sampleItems.Count < sampleSize)
        {
            sampleItems.Add(item);
        }
    }

    /// <summary>
    /// Mesaj listesine limit dahilinde mesaj ekler.
    /// </summary>
    private static void AddMessage(
        List<ExampleSentenceEnrichmentMessage> messages,
        int maxMessages,
        string type,
        string code,
        string message,
        string? sourceSentenceExternalId = null,
        string? targetSentenceExternalId = null,
        Guid? learningItemId = null)
    {
        if (messages.Count >= maxMessages)
        {
            return;
        }

        messages.Add(new ExampleSentenceEnrichmentMessage
        {
            Type = type,
            Code = code,
            Message = message,
            SourceSentenceExternalId = sourceSentenceExternalId,
            TargetSentenceExternalId = targetSentenceExternalId,
            LearningItemId = learningItemId
        });
    }
}