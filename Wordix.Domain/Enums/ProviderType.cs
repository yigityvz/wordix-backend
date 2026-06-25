namespace Wordix.Domain.Enums;

/// <summary>
/// Dış servis/provider tipini temsil eder.
/// 
/// Wordix ileride farklı kaynaklardan veri çekebilir:
/// - Dictionary provider
/// - Translation provider
/// - Example sentence provider
/// - AI provider
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// Kelime/phrase anlamı sağlayan provider.
    /// Örnek: Wiktionary, Kaikki
    /// </summary>
    Dictionary = 1,

    /// <summary>
    /// Çeviri sağlayan provider.
    /// Örnek: LibreTranslate
    /// </summary>
    Translation = 2,

    /// <summary>
    /// Örnek cümle sağlayan provider.
    /// Örnek: Tatoeba
    /// </summary>
    ExampleSentence = 3,

    /// <summary>
    /// AI tabanlı içerik/örnek/öneri sağlayan provider.
    /// </summary>
    Ai = 4,

    /// <summary>
    /// Büyük veri import süreçlerinde kullanılan kaynak.
    /// </summary>
    Import = 5
}