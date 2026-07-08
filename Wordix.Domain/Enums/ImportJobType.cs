namespace Wordix.Domain.Enums;

/// <summary>
/// Sistemde takip edilen import/enrichment job türlerini temsil eder.
/// 
/// ImportJobType neden gerekli?
/// - Her import işlemi aynı şey değildir.
/// - CEFR word import, Kaikki meaning enrichment ve Tatoeba example enrichment
///   farklı iş akışlarıdır.
/// - Loglama, admin ekranı, retry ve raporlama için job türünü bilmemiz gerekir.
/// </summary>
public enum ImportJobType
{
    /// <summary>
    /// Bilinmeyen veya henüz sınıflandırılmamış job türü.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// CEFR-J / Octanove gibi kelime listesi importu.
    /// </summary>
    WordListImport = 1,

    /// <summary>
    /// Kaikki/Wiktionary gibi kaynaklardan meaning parse testi.
    /// </summary>
    MeaningParse = 2,

    /// <summary>
    /// Kaikki/Wiktionary meaning verisini mevcut Word/Phrase kayıtlarına bağlama işlemi.
    /// </summary>
    MeaningEnrichment = 3,

    /// <summary>
    /// Tatoeba cümle dosyalarını parse etme testi.
    /// </summary>
    ExampleSentenceParse = 4,

    /// <summary>
    /// Tatoeba cümlelerini mevcut Word/Phrase LearningItem kayıtlarıyla eşleştirip
    /// Sentence/SentenceTranslation/LearningItemExampleSentence oluşturma işlemi.
    /// </summary>
    ExampleSentenceEnrichment = 5,

    /// <summary>
    /// Runtime provider lookup/import işlemleri.
    /// Örnek:
    /// DB miss sonrası Azure Translator ile Word/Phrase üretme.
    /// </summary>
    RuntimeProviderLookup = 6
}