namespace Wordix.Domain.Enums;

/// <summary>
/// Dış provider çağrısının sonucunu temsil eder.
/// 
/// Örnek provider:
/// - Azure Translator
/// - Tatoeba import provider
/// - Kaikki/Wiktionary parser
/// </summary>
public enum ProviderRequestStatus
{
    /// <summary>
    /// Durum bilinmiyor.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Çağrı başarıyla tamamlandı.
    /// </summary>
    Succeeded = 1,

    /// <summary>
    /// Provider çağrısı hata verdi.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Provider çağrısı yapılmadı, sonuç cache'den döndü.
    /// </summary>
    ServedFromCache = 3,

    /// <summary>
    /// Provider timeout verdi.
    /// </summary>
    Timeout = 4,

    /// <summary>
    /// Provider rate-limit veya kota sınırına takıldı.
    /// </summary>
    RateLimited = 5
}