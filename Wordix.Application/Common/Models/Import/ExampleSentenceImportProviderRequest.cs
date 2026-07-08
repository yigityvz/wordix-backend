using Wordix.Application.Common.Constants;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence import provider'ına gönderilen isteği temsil eder.
/// 
/// Bu model özellikle Tatoeba gibi dış cümle kaynaklarını parse etmek için kullanılır.
/// 
/// Tatoeba için neden birden fazla stream var?
/// - SourceSentencesStream: İngilizce cümle dosyası.
/// - TargetSentencesStream: Türkçe cümle dosyası.
/// - LinksStream: Hangi cümlenin hangi cümleyle çeviri ilişkisi olduğunu gösteren bağlantı dosyası.
/// 
/// Bu model Application katmanındadır.
/// Çünkü Application katmanı dış kaynağın teknik detayını bilmeden
/// "bana örnek cümle import satırları üret" ihtiyacını tanımlar.
/// </summary>
public sealed record ExampleSentenceImportProviderRequest
{
    /// <summary>
    /// Kaynak dildeki cümleleri içeren stream'dir.
    /// 
    /// Tatoeba örneği:
    /// eng_sentences.tsv
    /// 
    /// Beklenen satır formatı:
    /// sentenceId<TAB>languageCode<TAB>sentenceText
    /// </summary>
    public Stream? SourceSentencesStream { get; init; }

    /// <summary>
    /// Hedef dildeki cümleleri içeren stream'dir.
    /// 
    /// Tatoeba örneği:
    /// tur_sentences.tsv
    /// 
    /// Beklenen satır formatı:
    /// sentenceId<TAB>languageCode<TAB>sentenceText
    /// </summary>
    public Stream? TargetSentencesStream { get; init; }

    /// <summary>
    /// Kaynak ve hedef sentence id değerlerini bağlayan stream'dir.
    /// 
    /// Tatoeba örneği:
    /// links.csv
    /// 
    /// Beklenen satır formatı:
    /// sentenceId<TAB>linkedSentenceId
    /// 
    /// Not:
    /// Link dosyasında yön garanti olmayabilir.
    /// Yani bazen EnglishId -> TurkishId,
    /// bazen TurkishId -> EnglishId gelebilir.
    /// Provider iki yönü de kontrol edecektir.
    /// </summary>
    public Stream? LinksStream { get; init; }

    /// <summary>
    /// Wordix içindeki kaynak dil kodudur.
    /// 
    /// Wordix database tarafında genelde ISO kısa kod kullanıyoruz:
    /// en, tr gibi.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Wordix içindeki hedef dil kodudur.
    /// 
    /// Wordix için şu an Türkçe hedef dil olduğu için default tr.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Tatoeba dosyasında kaynak dili temsil eden provider dil kodudur.
    /// 
    /// Tatoeba'da İngilizce genelde "eng" olarak gelir.
    /// Wordix'te ise aynı dil "en" olarak tutulur.
    /// 
    /// Bu yüzden Wordix dil kodu ile provider dil kodunu ayrı tutuyoruz.
    /// </summary>
    public string SourceProviderLanguageCode { get; init; } = "eng";

    /// <summary>
    /// Tatoeba dosyasında hedef dili temsil eden provider dil kodudur.
    /// 
    /// Tatoeba'da Türkçe genelde "tur" olarak gelir.
    /// Wordix'te ise aynı dil "tr" olarak tutulur.
    /// </summary>
    public string TargetProviderLanguageCode { get; init; } = "tur";

    /// <summary>
    /// Parser'ın en fazla kaç eşleşmiş example sentence row döndüreceğini belirtir.
    /// 
    /// Büyük Tatoeba dosyalarında tüm veriyi parse etmek zaman alabilir.
    /// Parse-test endpointlerinde bu alanı düşük tutarak hızlı smoke test yapabiliriz.
    /// 
    /// Null ise sınır uygulanmaz.
    /// </summary>
    public int? MaxRows { get; init; }

    /// <summary>
    /// Response veya result içine en fazla kaç mesaj/hata alınacağını belirler.
    /// 
    /// Büyük dosyalarda binlerce bozuk satır olabilir.
    /// Hepsini response'a koymak istemeyiz.
    /// </summary>
    public int MaxMessages { get; init; } = 100;

    /// <summary>
    /// Kaynak lisans bilgisidir.
    /// 
    /// Tatoeba verisi kullanırken attribution/license bilgisini ileride DB'de saklamak isteyebiliriz.
    /// Bu alanı provider row'larına taşırız.
    /// </summary>
    public string? License { get; init; }
}