using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Dış meaning provider'lardan gelen bir anlam satırını Wordix'in anlayacağı standart modele çevirir.
/// 
/// Bu model Kaikki/Wiktionary parser'ı için yazılıyor,
/// ama ileride farklı meaning kaynakları da aynı modeli döndürebilir.
/// 
/// Örnek:
/// SourceText = "abandon"
/// MeaningText = "terk etmek"
/// TargetLanguageCode = "tr"
/// ContentSource = WiktionaryKaikki
/// QualityStatus = Imported
/// </summary>
public sealed record MeaningImportRow
{
    /// <summary>
    /// Kaynak kelime veya ifadedir.
    /// 
    /// Örnek:
    /// - abandon
    /// - give up
    /// - take care of
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak kelime/ifadenin normalize edilmiş halidir.
    /// 
    /// Duplicate kontrolü ve DB eşleştirme bu alan üzerinden yapılacak.
    /// 
    /// Örnek:
    /// " Abandon " => "abandon"
    /// </summary>
    public string NormalizedSourceText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Şu an CEFR word pool İngilizce olduğu için genelde "en" olacak.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef anlam dilidir.
    /// 
    /// Wordix için şu an Türkçe anlam istediğimizden "tr" olacak.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Provider'dan gelen anlam metnidir.
    /// 
    /// Örnek:
    /// "terk etmek"
    /// "vazgeçmek"
    /// "devam etmek"
    /// </summary>
    public string MeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Anlam metninin normalize edilmiş halidir.
    /// 
    /// Aynı kelime için aynı Türkçe anlam tekrar gelirse duplicate temizliği bu alanla yapılabilir.
    /// </summary>
    public string NormalizedMeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Kısa açıklama veya gloss bilgisidir.
    /// 
    /// Kaikki/Wiktionary içinde sense gloss bilgisi varsa burada taşıyabiliriz.
    /// 
    /// Örnek:
    /// "to leave behind or give up completely"
    /// </summary>
    public string? ShortDefinition { get; init; }

    /// <summary>
    /// Kelimenin türüdür.
    /// 
    /// Örnek:
    /// noun, verb, adjective, adverb
    /// </summary>
    public string? PartOfSpeech { get; init; }

    /// <summary>
    /// Anlam kategorisi veya semantic group için kullanılabilir.
    /// 
    /// Şu an zorunlu değil.
    /// İleride konu bazlı anlam ayrımı yapmak istersek faydalı olabilir.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Bu satırın phrase/phrasal verb/idiom adayı olup olmadığını belirtir.
    /// 
    /// Örnek:
    /// SourceText = "give up"
    /// IsPhraseCandidate = true
    /// 
    /// Bu fazda phrase'i DB'ye kaydetmeyeceğiz.
    /// Ama parser çıktısında bu bilgiyi taşımak ileride phrase import fazını kolaylaştırır.
    /// </summary>
    public bool IsPhraseCandidate { get; init; }

    /// <summary>
    /// İçeriğin hangi kaynaktan geldiğini belirtir.
    /// 
    /// Kaikki parser için default WiktionaryKaikki kullanıyoruz.
    /// </summary>
    public ContentSource ContentSource { get; init; } = ContentSource.WiktionaryKaikki;

    /// <summary>
    /// İçeriğin kalite durumunu belirtir.
    /// 
    /// Kaikki'den gelen anlamları şu aşamada Verified değil, Imported kabul ediyoruz.
    /// Çünkü otomatik import edilmiş sözlük verisi olacak.
    /// </summary>
    public ContentQualityStatus QualityStatus { get; init; } = ContentQualityStatus.Imported;

    /// <summary>
    /// Provider adıdır.
    /// 
    /// Örnek:
    /// WiktionaryKaikki
    /// </summary>
    public string SourceProvider { get; init; } = ImportConstants.ProviderNames.WiktionaryKaikki;

    /// <summary>
    /// Kaynak lisans bilgisidir.
    /// 
    /// Kaikki/Wiktionary verisini kullanırken attribution/license bilgisini ileride saklamak için ayrıldı.
    /// </summary>
    public string? License { get; init; }

    /// <summary>
    /// Dış kaynakta bu meaning satırını takip etmek için kullanılan anahtardır.
    /// 
    /// Örnek:
    /// kaikki:line-123:sense-1:translation-2:abandon
    /// </summary>
    public string? ExternalSourceKey { get; init; }

    /// <summary>
    /// Ham dosyada bu kaydın kaçıncı satırdan geldiğini belirtir.
    /// 
    /// Hata ayıklama ve import logging için çok önemlidir.
    /// </summary>
    public int SourceRowNumber { get; init; }

    /// <summary>
    /// Kaikki entry içindeki sense index bilgisidir.
    /// 
    /// Aynı kelimenin birden fazla sense'i olabilir.
    /// Örneğin "bank" kelimesi finans kurumu veya nehir kenarı anlamına gelebilir.
    /// </summary>
    public int? SenseIndex { get; init; }

    /// <summary>
    /// Sense içindeki translation index bilgisidir.
    /// 
    /// Aynı sense altında birden fazla Türkçe translation olabilir.
    /// </summary>
    public int? TranslationIndex { get; init; }
}