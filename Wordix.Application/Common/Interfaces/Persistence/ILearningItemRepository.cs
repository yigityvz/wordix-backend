using Wordix.Application.Common.Models.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// LearningItem ve ona bağlı içerik detayları için özel repository sözleşmesidir.
/// 
/// Generic repository temel CRUD işlemleri için yeterlidir.
/// Ancak Wordix'te lookup gibi özel sorgular vardır:
/// - normalized word/phrase text ile arama,
/// - LearningItem + Word/Phrase + Meaning verisini birlikte getirme,
/// - aynı içerik var mı kontrol etme.
/// 
/// Bu tarz domain'e özel sorguları generic repository içine koymak yerine
/// özel repository ile ayırıyoruz.
/// </summary>
public interface ILearningItemRepository
{
    /// <summary>
    /// Normalize edilmiş kelime metnine göre Word lookup verisini getirir.
    /// 
    /// Örnek:
    /// normalizedText = "achieve"
    /// sourceLanguageId = English language Id
    /// targetLanguageId = Turkish language Id
    /// 
    /// Dönen veri:
    /// - LearningItem
    /// - Word
    /// - Türkçe Meaning listesi
    /// 
    /// Kayıt bulunamazsa null döner.
    /// Böylece lookup handler database'de yoksa provider akışına geçebilir.
    /// </summary>
    Task<WordLookupData?> GetWordLookupDataAsync(
        string normalizedText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalize edilmiş phrase metnine göre Phrase lookup verisini getirir.
    /// 
    /// Örnek:
    /// normalizedText = "give up"
    /// sourceLanguageId = English language Id
    /// targetLanguageId = Turkish language Id
    /// 
    /// Dönen veri:
    /// - LearningItem
    /// - Phrase
    /// - Türkçe Meaning listesi
    /// 
    /// Kayıt bulunamazsa null döner.
    /// Böylece lookup handler database'de yoksa provider akışına geçebilir.
    /// </summary>
    Task<PhraseLookupData?> GetPhraseLookupDataAsync(
        string normalizedText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir dilde normalize edilmiş kelime var mı kontrol eder.
    /// 
    /// Bu method duplicate kelime oluşmasını engellemek için kullanılabilir.
    /// Örneğin English içinde "achieve" zaten varsa tekrar oluşturmayız.
    /// </summary>
    Task<bool> WordExistsAsync(
        string normalizedText,
        Guid sourceLanguageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir dilde normalize edilmiş phrase var mı kontrol eder.
    /// 
    /// Bu method duplicate phrase oluşmasını engellemek için kullanılabilir.
    /// Örneğin English içinde "give up" zaten varsa tekrar oluşturmayız.
    /// </summary>
    Task<bool> PhraseExistsAsync(
        string normalizedText,
        Guid sourceLanguageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Meaning enrichment için DB'deki mevcut Word + LearningItem eşleşmelerini getirir.
    /// 
    /// Neden bu method özel repository'de?
    /// - Word, LearningItem ve Language tabloları arasında join gerekir.
    /// - Application service DbContext bilmemelidir.
    /// - Generic repository bu join senaryosu için yeterli değildir.
    /// </summary>
    Task<IReadOnlyCollection<MeaningEnrichmentWordMatch>> GetMeaningEnrichmentWordMatchesAsync(
        string sourceLanguageCode,
        IReadOnlyCollection<string> normalizedTexts,
        IReadOnlyCollection<ContentSource> allowedContentSources,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Example sentence enrichment için mevcut aktif Word/Phrase LearningItem adaylarını getirir.
    /// 
    /// Bu method Tatoeba gibi dış cümle kaynaklarından gelen örnek cümleleri
    /// mevcut Word/Phrase içeriklerle eşleştirmek için kullanılır.
    /// 
    /// Neden özel repository methodu?
    /// - LearningItem + Word/Phrase + Language join gerekir.
    /// - Application katmanı DbContext bilmemelidir.
    /// - Generic repository bu join senaryosu için yeterli değildir.
    /// </summary>
    Task<IReadOnlyCollection<ExampleSentenceLearningItemCandidate>> GetExampleSentenceLearningItemCandidatesAsync(
        string sourceLanguageCode,
        IReadOnlyCollection<LearningItemType> allowedItemTypes,
        IReadOnlyCollection<ContentSource> allowedContentSources,
        CancellationToken cancellationToken = default);

}