using Wordix.Application.Common.Models.Persistence;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// LearningItem ve ona bağlı içerik detayları için özel repository sözleşmesidir.
/// 
/// Generic repository temel CRUD işlemleri için yeterlidir.
/// Ancak Wordix'te lookup gibi özel sorgular vardır:
/// - normalized word text ile arama,
/// - LearningItem + Word + Meaning verisini birlikte getirme,
/// - aynı kelime var mı kontrol etme.
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
    /// Belirli bir dilde normalize edilmiş kelime var mı kontrol eder.
    /// 
    /// Bu method duplicate kelime oluşmasını engellemek için kullanılabilir.
    /// Örneğin English içinde "achieve" zaten varsa tekrar oluşturmayız.
    /// </summary>
    Task<bool> WordExistsAsync(
        string normalizedText,
        Guid sourceLanguageId,
        CancellationToken cancellationToken = default);
}