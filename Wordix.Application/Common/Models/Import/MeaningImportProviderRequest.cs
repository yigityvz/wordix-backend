using Wordix.Application.Common.Constants;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Meaning import provider'ına gönderilen isteği temsil eder.
/// 
/// Bu model neden var?
/// - Provider'a sadece Stream vermek kısa vadede yeterli gibi görünür.
/// - Ama ileride source language, target language, maksimum satır sayısı,
///   phrase adaylarını dahil etme gibi seçeneklere ihtiyaç duyacağız.
/// - Bu yüzden parametreleri tek tek method imzasına eklemek yerine
///   request modeli altında topluyoruz.
/// </summary>
public sealed record MeaningImportProviderRequest
{
    /// <summary>
    /// Okunacak ham veri stream'idir.
    /// 
    /// Kaikki/Wiktionary tarafında bu genellikle JSONL dosyası olacak.
    /// JSONL formatında her satır ayrı bir JSON object olarak okunur.
    /// </summary>
    public Stream? SourceStream { get; init; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Biz şu an İngilizce kelime havuzunu enrich edeceğimiz için default "en" kullanıyoruz.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef anlam/çeviri dil kodudur.
    /// 
    /// Wordix için şu an hedef dil Türkçe olduğu için default "tr" kullanıyoruz.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Parser'ın en fazla kaç meaning row döndüreceğini belirtir.
    /// 
    /// Büyük Kaikki dosyalarını test ederken çok faydalıdır.
    /// Örneğin ilk smoke testte MaxRows = 100 verip parser'ı hızlı deneyebiliriz.
    /// Null ise sınır uygulanmaz.
    /// </summary>
    public int? MaxRows { get; init; }

    /// <summary>
    /// Çok kelimeli ifadelerin phrase adayı olarak işaretlenip işaretlenmeyeceğini belirler.
    /// 
    /// Örnek:
    /// - "give up"
    /// - "look after"
    /// - "as soon as possible"
    /// 
    /// Bu fazda phrase'i database'e kaydetmeyeceğiz.
    /// Sadece parser çıktısında "bu phrase olabilir" bilgisini taşıyacağız.
    /// </summary>
    public bool IncludePhraseCandidates { get; init; } = true;
}