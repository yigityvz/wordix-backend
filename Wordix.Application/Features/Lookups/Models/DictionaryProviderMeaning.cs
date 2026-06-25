namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Dictionary provider'dan dönen tek bir anlam bilgisini temsil eder.
/// 
/// Bu model neden var?
/// - Provider implementation'ları Domain entity döndürmesin.
/// - Provider sonucu önce application model olarak gelsin.
/// - Handler daha sonra bu modeli LearningItem/Word/Meaning entity'lerine map etsin.
/// 
/// Örnek:
/// achieve → başarmak
/// improve → geliştirmek
/// perfect → mükemmel
/// </summary>
public sealed class DictionaryProviderMeaning
{
    /// <summary>
    /// Hedef dildeki çeviri/anlam bilgisidir.
    /// 
    /// Örnek:
    /// - başarmak
    /// - geliştirmek
    /// - mükemmel
    /// </summary>
    public string Translation { get; init; } = string.Empty;

    /// <summary>
    /// Kelimenin açıklaması veya tanımıdır.
    /// 
    /// İlk prototype provider'da null olabilir.
    /// İleride dış provider/import sistemiyle doldurulabilir.
    /// </summary>
    public string? Definition { get; init; }

    /// <summary>
    /// Örnek cümledir.
    /// 
    /// İlk prototype provider'da null olabilir.
    /// İleride provider/import sistemi gelişince doldurulabilir.
    /// </summary>
    public string? ExampleSentence { get; init; }

    /// <summary>
    /// Kelimenin türüdür.
    /// 
    /// Örnek:
    /// - noun
    /// - verb
    /// - adjective
    /// 
    /// İlk prototype provider'da bazı kelimeler için dolu olabilir, bazıları için null olabilir.
    /// </summary>
    public string? PartOfSpeech { get; init; }
}