using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Persistence;
using Wordix.Domain.Enums;
using Wordix.Persistence.Contexts;

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
}