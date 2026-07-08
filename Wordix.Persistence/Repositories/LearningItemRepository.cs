using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Persistence;
using Wordix.Domain.Enums;
using Wordix.Persistence.Contexts;
using Wordix.Application.Common.Models.Import;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// LearningItem ve Word lookup odaklı özel repository implementasyonudur.
/// 
/// Bu sınıf EF Core kullanır.
/// Bu yüzden Persistence katmanındadır.
/// 
/// Application katmanı ILearningItemRepository interface'ini bilir,
/// bu sınıfın EF Core ile nasıl sorgu yazdığını bilmez.
/// </summary>
public class LearningItemRepository : ILearningItemRepository
{
    private readonly WordixDbContext _dbContext;

    public LearningItemRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Normalize edilmiş kelime metnine göre LearningItem + Word + Meaning verisini getirir.
    /// 
    /// Neden burada join kullanıyoruz?
    /// 
    /// Çünkü Word entity'si dil bilgisini doğrudan taşımaz.
    /// Dil bilgisi LearningItem üzerinde tutulur.
    /// Bu yüzden "English dilindeki achieve kelimesi" için Word ile LearningItem birlikte sorgulanır.
    /// 
    /// Meaning listesi ayrıca hedef dile göre çekilir.
    /// Örneğin sourceLanguageId = English, targetLanguageId = Turkish.
    /// </summary>
    public async Task<WordLookupData?> GetWordLookupDataAsync(
        string normalizedText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        if (sourceLanguageId == Guid.Empty || targetLanguageId == Guid.Empty)
        {
            return null;
        }

        var normalized = normalizedText.Trim().ToLowerInvariant();

        // Word + LearningItem birlikte sorgulanır.
        // AsNoTracking kullanıyoruz çünkü bu method sadece okuma amaçlıdır.
        var wordData = await (
            from word in _dbContext.Words.AsNoTracking()
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on word.LearningItemId equals learningItem.Id
            where word.NormalizedText == normalized
                  && learningItem.LanguageId == sourceLanguageId
                  && learningItem.ItemType == LearningItemType.Word
                  && learningItem.IsActive
            select new
            {
                LearningItem = learningItem,
                Word = word
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (wordData is null)
        {
            return null;
        }

        // Kelimenin hedef dildeki anlamlarını getiriyoruz.
        // Önce primary anlam gelsin, sonra DisplayOrder'a göre sıralansın.
        var meanings = await _dbContext.Meanings
            .AsNoTracking()
            .Where(meaning =>
                meaning.LearningItemId == wordData.LearningItem.Id &&
                meaning.TargetLanguageId == targetLanguageId)
            .OrderByDescending(meaning => meaning.IsPrimary)
            .ThenBy(meaning => meaning.DisplayOrder)
            .ToListAsync(cancellationToken);

        return new WordLookupData(
            wordData.LearningItem,
            wordData.Word,
            meanings);
    }


    /// <summary>
    /// Normalize edilmiş phrase metnine göre LearningItem + Phrase + Meaning verisini getirir.
    /// 
    /// Neden burada join kullanıyoruz?
    /// 
    /// Çünkü Phrase entity'si dil bilgisini doğrudan taşımaz.
    /// Dil bilgisi LearningItem üzerinde tutulur.
    /// Bu yüzden "English dilindeki give up phrase'i" için Phrase ile LearningItem birlikte sorgulanır.
    /// 
    /// Meaning listesi ayrıca hedef dile göre çekilir.
    /// Örneğin sourceLanguageId = English, targetLanguageId = Turkish.
    /// </summary>
    public async Task<PhraseLookupData?> GetPhraseLookupDataAsync(
        string normalizedText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return null;
        }

        if (sourceLanguageId == Guid.Empty || targetLanguageId == Guid.Empty)
        {
            return null;
        }

        var normalized = normalizedText.Trim().ToLowerInvariant();

        // Phrase + LearningItem birlikte sorgulanır.
        // AsNoTracking kullanıyoruz çünkü bu method sadece okuma amaçlıdır.
        var phraseData = await (
            from phrase in _dbContext.Phrases.AsNoTracking()
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on phrase.LearningItemId equals learningItem.Id
            where phrase.NormalizedText == normalized
                  && learningItem.LanguageId == sourceLanguageId
                  && learningItem.ItemType == LearningItemType.Phrase
                  && learningItem.IsActive
            select new
            {
                LearningItem = learningItem,
                Phrase = phrase
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (phraseData is null)
        {
            return null;
        }

        // Phrase'in hedef dildeki anlamlarını getiriyoruz.
        // Önce primary anlam gelsin, sonra DisplayOrder'a göre sıralansın.
        var meanings = await _dbContext.Meanings
            .AsNoTracking()
            .Where(meaning =>
                meaning.LearningItemId == phraseData.LearningItem.Id &&
                meaning.TargetLanguageId == targetLanguageId)
            .OrderByDescending(meaning => meaning.IsPrimary)
            .ThenBy(meaning => meaning.DisplayOrder)
            .ToListAsync(cancellationToken);

        return new PhraseLookupData(
            phraseData.LearningItem,
            phraseData.Phrase,
            meanings);
    }

    /// <summary>
    /// Belirli bir kaynak dilde normalize edilmiş kelime var mı kontrol eder.
    /// 
    /// Bu method provider/import akışında duplicate oluşmasını engellemek için kullanılabilir.
    /// Örneğin English içinde "achieve" zaten varsa tekrar Word oluşturmayız.
    /// </summary>
    public async Task<bool> WordExistsAsync(
        string normalizedText,
        Guid sourceLanguageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return false;
        }

        if (sourceLanguageId == Guid.Empty)
        {
            return false;
        }

        var normalized = normalizedText.Trim().ToLowerInvariant();

        return await (
            from word in _dbContext.Words.AsNoTracking()
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on word.LearningItemId equals learningItem.Id
            where word.NormalizedText == normalized
                  && learningItem.LanguageId == sourceLanguageId
                  && learningItem.ItemType == LearningItemType.Word
            select word.Id)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Belirli bir kaynak dilde normalize edilmiş phrase var mı kontrol eder.
    /// 
    /// Bu method provider/import akışında duplicate oluşmasını engellemek için kullanılabilir.
    /// Örneğin English içinde "give up" zaten varsa tekrar Phrase oluşturmayız.
    /// </summary>
    public async Task<bool> PhraseExistsAsync(
        string normalizedText,
        Guid sourceLanguageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return false;
        }

        if (sourceLanguageId == Guid.Empty)
        {
            return false;
        }

        var normalized = normalizedText.Trim().ToLowerInvariant();

        return await (
            from phrase in _dbContext.Phrases.AsNoTracking()
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on phrase.LearningItemId equals learningItem.Id
            where phrase.NormalizedText == normalized
                  && learningItem.LanguageId == sourceLanguageId
                  && learningItem.ItemType == LearningItemType.Phrase
            select phrase.Id)
            .AnyAsync(cancellationToken);
    }


    /// <summary>
    /// Meaning enrichment için Word + LearningItem + Language join'i yapar.
    /// 
    /// Bu method neden Persistence katmanında?
    /// - EF Core ve DbContext kullanır.
    /// - Join ve query optimizasyonu teknik veri erişim detaylarıdır.
    /// - Application katmanı sadece dönen MeaningEnrichmentWordMatch modelini görür.
    /// </summary>
    public async Task<IReadOnlyCollection<MeaningEnrichmentWordMatch>> GetMeaningEnrichmentWordMatchesAsync(
        string sourceLanguageCode,
        IReadOnlyCollection<string> normalizedTexts,
        IReadOnlyCollection<ContentSource> allowedContentSources,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguageCode) ||
            normalizedTexts.Count == 0 ||
            allowedContentSources.Count == 0)
        {
            return Array.Empty<MeaningEnrichmentWordMatch>();
        }

        var normalizedLanguageCode = sourceLanguageCode.Trim().ToLowerInvariant();

        // EF Core Contains sorgusunu SQL IN'e çevirir.
        // Service tarafında bu method küçük chunk'larla çağrılacak.
        var normalizedTextArray = normalizedTexts
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        var allowedSourceArray = allowedContentSources
            .Distinct()
            .ToArray();

        if (normalizedTextArray.Length == 0 ||
            allowedSourceArray.Length == 0)
        {
            return Array.Empty<MeaningEnrichmentWordMatch>();
        }

        var matches = await (
            from word in _dbContext.Words.AsNoTracking()
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on word.LearningItemId equals learningItem.Id
            join language in _dbContext.Languages.AsNoTracking()
                on learningItem.LanguageId equals language.Id
            where learningItem.ItemType == LearningItemType.Word
                  && learningItem.IsActive
                  && language.IsActive
                  && language.Code == normalizedLanguageCode
                  && normalizedTextArray.Contains(word.NormalizedText)
                  && allowedSourceArray.Contains(learningItem.ContentSource)
            select new MeaningEnrichmentWordMatch
            {
                LearningItemId = learningItem.Id,
                WordId = word.Id,
                NormalizedText = word.NormalizedText,
                PartOfSpeech = word.PartOfSpeech,
                LanguageId = learningItem.LanguageId
            })
            .ToListAsync(cancellationToken);

        return matches;
    }


    /// <summary>
    /// Example sentence enrichment için aktif Word/Phrase LearningItem adaylarını getirir.
    /// 
    /// Bu method neden burada?
    /// - Word ve Phrase entity'leri dil bilgisini doğrudan taşımaz.
    /// - Dil bilgisi LearningItem üzerinde tutulur.
    /// - Language.Code ile filtrelemek için LearningItem + Language join gerekir.
    /// - Word/Phrase metinleri ayrı tablolardan gelir.
    /// 
    /// Application katmanı sadece ExampleSentenceLearningItemCandidate modelini görür.
    /// EF Core join detayı Persistence katmanında kalır.
    /// </summary>
    public async Task<IReadOnlyCollection<ExampleSentenceLearningItemCandidate>> GetExampleSentenceLearningItemCandidatesAsync(
        string sourceLanguageCode,
        IReadOnlyCollection<LearningItemType> allowedItemTypes,
        IReadOnlyCollection<ContentSource> allowedContentSources,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceLanguageCode) ||
            allowedItemTypes.Count == 0 ||
            allowedContentSources.Count == 0)
        {
            return Array.Empty<ExampleSentenceLearningItemCandidate>();
        }

        var normalizedLanguageCode = sourceLanguageCode.Trim().ToLowerInvariant();

        var allowedItemTypeArray = allowedItemTypes
            .Distinct()
            .ToArray();

        var allowedSourceArray = allowedContentSources
            .Distinct()
            .ToArray();

        if (allowedItemTypeArray.Length == 0 || allowedSourceArray.Length == 0)
        {
            return Array.Empty<ExampleSentenceLearningItemCandidate>();
        }

        var candidates = new List<ExampleSentenceLearningItemCandidate>();

        // Word adayları:
        // LearningItem + Word + Language join yapıyoruz.
        if (allowedItemTypeArray.Contains(LearningItemType.Word))
        {
            var wordCandidates = await (
                from word in _dbContext.Words.AsNoTracking()
                join learningItem in _dbContext.LearningItems.AsNoTracking()
                    on word.LearningItemId equals learningItem.Id
                join language in _dbContext.Languages.AsNoTracking()
                    on learningItem.LanguageId equals language.Id
                where learningItem.ItemType == LearningItemType.Word
                      && learningItem.IsActive
                      && language.IsActive
                      && language.Code == normalizedLanguageCode
                      && allowedSourceArray.Contains(learningItem.ContentSource)
                      && !string.IsNullOrWhiteSpace(word.NormalizedText)
                select new ExampleSentenceLearningItemCandidate
                {
                    LearningItemId = learningItem.Id,
                    ItemType = learningItem.ItemType,
                    ContentId = word.Id,
                    Text = word.Text,
                    NormalizedText = word.NormalizedText,
                    LanguageId = learningItem.LanguageId,
                    LanguageCode = language.Code,
                    ContentSource = learningItem.ContentSource,
                    QualityStatus = learningItem.QualityStatus
                })
                .ToListAsync(cancellationToken);

            candidates.AddRange(wordCandidates);
        }

        // Phrase adayları:
        // Tatoeba cümlesinde phrase geçiyorsa örnek cümle bağlayabiliriz.
        if (allowedItemTypeArray.Contains(LearningItemType.Phrase))
        {
            var phraseCandidates = await (
                from phrase in _dbContext.Phrases.AsNoTracking()
                join learningItem in _dbContext.LearningItems.AsNoTracking()
                    on phrase.LearningItemId equals learningItem.Id
                join language in _dbContext.Languages.AsNoTracking()
                    on learningItem.LanguageId equals language.Id
                where learningItem.ItemType == LearningItemType.Phrase
                      && learningItem.IsActive
                      && language.IsActive
                      && language.Code == normalizedLanguageCode
                      && allowedSourceArray.Contains(learningItem.ContentSource)
                      && !string.IsNullOrWhiteSpace(phrase.NormalizedText)
                select new ExampleSentenceLearningItemCandidate
                {
                    LearningItemId = learningItem.Id,
                    ItemType = learningItem.ItemType,
                    ContentId = phrase.Id,
                    Text = phrase.Text,
                    NormalizedText = phrase.NormalizedText,
                    LanguageId = learningItem.LanguageId,
                    LanguageCode = language.Code,
                    ContentSource = learningItem.ContentSource,
                    QualityStatus = learningItem.QualityStatus
                })
                .ToListAsync(cancellationToken);

            candidates.AddRange(phraseCandidates);
        }

        return candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate.NormalizedText))
            .GroupBy(candidate => new
            {
                candidate.LearningItemId,
                candidate.ItemType
            })
            .Select(group => group.First())
            .ToArray();
    }

}