namespace Wordix.Domain.Enums;

/// <summary>
/// Phrase içeriklerinin türünü temsil eder.
/// 
/// Wordix'te Phrase sadece "iki kelimeden oluşan metin" değildir.
/// Phrasal verb, idiom, expression veya collocation gibi farklı kalıp ifade türleri olabilir.
/// 
/// Bu enum sayesinde ileride:
/// - Kullanıcı sadece phrasal verb çalışabilir.
/// - Admin içerikleri türüne göre filtreleyebilir.
/// - Quiz sistemi phrase türüne göre soru üretebilir.
/// - Analytics hangi phrase türlerinde zorlanıldığını gösterebilir.
/// </summary>
public enum PhraseType
{
    /// <summary>
    /// Phrase türü bilinmiyor veya henüz sınıflandırılmamış.
    /// 
    /// Prototype/provider/import aşamalarında tür bilgisi yoksa bu değer kullanılabilir.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Phrasal verb türündeki ifadeler.
    /// 
    /// Örnek:
    /// give up, look after, turn on
    /// </summary>
    PhrasalVerb = 1,

    /// <summary>
    /// Deyimsel ifadeler.
    /// 
    /// Örnek:
    /// piece of cake, break the ice
    /// </summary>
    Idiom = 2,

    /// <summary>
    /// Genel kalıp ifadeler.
    /// 
    /// Örnek:
    /// by the way, in my opinion, as soon as possible
    /// </summary>
    Expression = 3,

    /// <summary>
    /// Birlikte sık kullanılan kelime grupları.
    /// 
    /// Örnek:
    /// make a decision, take responsibility, heavy rain
    /// </summary>
    Collocation = 4
}