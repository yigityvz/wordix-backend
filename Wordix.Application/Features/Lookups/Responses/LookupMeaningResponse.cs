namespace Wordix.Application.Features.Lookups.Responses;

/// <summary>
/// Lookup sonucunda dönen tek bir anlam bilgisini temsil eder.
/// 
/// Bir kelimenin birden fazla anlamı olabilir.
/// Örneğin "run":
/// - koşmak
/// - işletmek
/// - çalıştırmak
/// 
/// Bu yüzden LookupResponse içinde meanings koleksiyonu bulunur.
/// </summary>
public sealed class LookupMeaningResponse
{
    /// <summary>
    /// Meaning entity'sinin database id değeridir.
    /// 
    /// Eğer anlam database'den geliyorsa dolu olur.
    /// Yeni provider sonucu oluşturulup kaydedildiyse yine kayıt sonrası dolu olur.
    /// </summary>
    public Guid MeaningId { get; init; }

    /// <summary>
    /// Hedef dildeki anlam/çeviri bilgisidir.
    /// 
    /// Örnek:
    /// achieve → başarmak
    /// improve → geliştirmek
    /// perfect → mükemmel
    /// </summary>
    public string Translation { get; init; } = string.Empty;

    /// <summary>
    /// Kelimenin açıklaması veya tanımıdır.
    /// 
    /// İlk prototipte null olabilir.
    /// İleride provider/import sistemi gelişince doldurulabilir.
    /// </summary>
    public string? Definition { get; init; }

    /// <summary>
    /// Örnek cümledir.
    /// 
    /// İlk prototipte null olabilir.
    /// İleride AI/provider/import sistemiyle zenginleştirilebilir.
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
    /// İlk prototipte null olabilir.
    /// </summary>
    public string? PartOfSpeech { get; init; }
}