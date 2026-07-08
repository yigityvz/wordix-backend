namespace Wordix.Domain.Enums;

/// <summary>
/// Bir içerik parçasının gerçek veri kaynağını temsil eder.
/// 
/// LearningItemSourceType şunu söyler:
/// - Bu kayıt sisteme hangi akışla geldi?
///   Örnek: Import, UserLookup, Provider
/// 
/// ContentSource ise şunu söyler:
/// - Bu verinin gerçek kaynağı ne?
///   Örnek: CefrJ, WiktionaryKaikki, Tatoeba, LibreTranslate
/// 
/// Bu ayrım neden önemli?
/// Çünkü "Import" bir süreçtir, "CefrJ" ise gerçek veri kaynağıdır.
/// </summary>
public enum ContentSource
{
    /// <summary>
    /// Kaynak bilinmiyor veya henüz set edilmemiş.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Veri admin veya geliştirici tarafından manuel girildi.
    /// </summary>
    Manual = 1,

    /// <summary>
    /// CEFR-J/Open Language Profiles kelime listesinden geldi.
    /// Genellikle kelime + CEFR seviyesi için kullanılır.
    /// </summary>
    CefrJ = 2,

    /// <summary>
    /// Wiktionary verisinin machine-readable çıktısı olan Kaikki kaynağından geldi.
    /// Genellikle kelime/phrase anlamları için kullanılır.
    /// </summary>
    WiktionaryKaikki = 3,

    /// <summary>
    /// Tatoeba sentence/example kaynağından geldi.
    /// Genellikle örnek cümle ve cümle çevirileri için kullanılır.
    /// </summary>
    Tatoeba = 4,

    /// <summary>
    /// LibreTranslate çeviri provider'ından otomatik üretildi.
    /// Genellikle fallback çeviri için kullanılır.
    /// </summary>
    LibreTranslate = 5,

    /// <summary>
    /// Kullanıcının yazdığı inputtan geldi.
    /// Örnek: kullanıcının çevirdiği cümleyi dictionary'ye kaydetmesi.
    /// </summary>
    UserInput = 6,

    /// <summary>
    /// Sistem tarafından basit fallback olarak üretildi.
    /// Örnek: hiçbir anlam bulunamazsa meaningText = word text yapılması.
    /// </summary>
    Fallback = 7,

    /// <summary>
    /// Sistem tarafından otomatik örnek cümle/placeholder üretildi.
    /// AI veya template tabanlı üretimlerde kullanılabilir.
    /// </summary>
    SystemGenerated = 8,

    /// <summary>
    /// Octanove C1/C2 vocabulary profile kaynağından geldi.
    /// 
    /// Bu kaynak özellikle ileri seviye C1-C2 kelimeleri desteklemek için kullanılabilir.
    /// </summary>
    Octanove = 9,

    /// <summary>
    /// Azure Translator provider'ından otomatik üretildi.
    /// 
    /// Ne için kullanacağız?
    /// - DB'de bulunmayan Word lookup sonucu Azure'dan çevrilirse
    ///   oluşan Meaning/LearningItem metadata'sında kullanacağız.
    /// - DB'de bulunmayan Phrase lookup sonucu Azure'dan çevrilirse
    ///   oluşan Meaning/LearningItem metadata'sında kullanacağız.
    /// 
    /// Sentence lookup için Azure kullanılabilir,
    /// fakat bu faz kararımıza göre sentence sonucu DB'ye kaydedilmeyecektir.
    /// </summary>
    AzureTranslator = 10,

    /// <summary>
    /// FreeDict English-Turkish sözlük kaynağından geldi.
    /// 
    /// Ne için kullanacağız?
    /// - CEFR-J / Octanove ile import edilmiş İngilizce Word kayıtlarına
    ///   Türkçe Meaning eklemek için kullanacağız.
    /// 
    /// Not:
    /// FreeDict verisi otomatik import edilmiş açık sözlük verisidir.
    /// Bu yüzden meaning tarafında QualityStatus = Imported kullanılmalıdır.
    /// </summary>
    FreeDict = 11
}