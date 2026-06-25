using Wordix.Application.Features.Lookups.Models;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Database'de bulunmayan lookup içerikleri için provider sözleşmesidir.
/// 
/// Bu interface neden Application katmanında?
/// - Lookup handler'ın ihtiyacı olan abstraction budur.
/// - Handler provider'ın gerçek implementation detayını bilmez.
/// - Implementation Infrastructure katmanında olabilir.
/// 
/// Faz 13'te PrototypeDictionaryProvider kullanılacak.
/// Faz 24'te gerçek import/provider mimarisi detaylandırılacak.
/// </summary>
public interface IDictionaryProvider
{
    /// <summary>
    /// Normalize edilmiş text için provider üzerinden anlam arar.
    /// 
    /// Önemli:
    /// Bu method entity döndürmez.
    /// Sadece provider result modeli döndürür.
    /// Entity oluşturma işi handler/domain tarafında yapılır.
    /// </summary>
    /// <param name="normalizedText">Normalize edilmiş lookup text.</param>
    /// <param name="sourceLanguageCode">Kaynak dil kodu. Örnek: en</param>
    /// <param name="targetLanguageCode">Hedef dil kodu. Örnek: tr</param>
    /// <param name="cancellationToken">Async operasyon iptal token'ı.</param>
    /// <returns>Provider lookup sonucu.</returns>
    Task<DictionaryProviderResult> FindAsync(
        string normalizedText,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken = default);
}