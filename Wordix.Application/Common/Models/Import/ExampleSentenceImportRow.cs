using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Dış example sentence provider'larından gelen tek bir örnek cümle eşleşmesini
/// Wordix'in anlayacağı standart modele çevirir.
/// 
/// Tatoeba örneği:
/// SourceText = "I have to go to sleep."
/// TranslatedText = "Yatmaya gitmek zorundayım."
/// 
/// Bu model DB entity değildir.
/// Parser ve enrichment servisleri arasında taşınan Application modelidir.
/// </summary>
public sealed record ExampleSentenceImportRow
{
    /// <summary>
    /// Dış kaynakta kaynak cümlenin id değeridir.
    /// 
    /// Tatoeba örneği:
    /// 1277
    /// 
    /// String tutuyoruz çünkü dış kaynak id formatı ileride farklı provider'larda değişebilir.
    /// </summary>
    public string SourceSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Dış kaynakta hedef/çeviri cümlenin id değeridir.
    /// 
    /// Tatoeba örneği:
    /// 1166752
    /// </summary>
    public string TargetSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dildeki örnek cümle metnidir.
    /// 
    /// Örnek:
    /// I have to go to sleep.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak cümlenin normalize edilmiş halidir.
    /// 
    /// Duplicate kontrolü, eşleştirme ve arama işlemlerinde kullanılabilir.
    /// </summary>
    public string NormalizedSourceText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dildeki çeviri cümle metnidir.
    /// 
    /// Örnek:
    /// Yatmaya gitmek zorundayım.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Çeviri cümlenin normalize edilmiş halidir.
    /// Duplicate kontrolü için kullanılabilir.
    /// </summary>
    public string NormalizedTranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Wordix içindeki kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Wordix içindeki hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Provider dosyasında kaynak cümlenin dil kodudur.
    /// 
    /// Tatoeba örneği:
    /// eng
    /// </summary>
    public string SourceProviderLanguageCode { get; init; } = "eng";

    /// <summary>
    /// Provider dosyasında hedef cümlenin dil kodudur.
    /// 
    /// Tatoeba örneği:
    /// tur
    /// </summary>
    public string TargetProviderLanguageCode { get; init; } = "tur";

    /// <summary>
    /// İçeriğin hangi kaynaktan geldiğini belirtir.
    /// 
    /// Tatoeba parser için Tatoeba.
    /// </summary>
    public ContentSource ContentSource { get; init; } = ContentSource.Tatoeba;

    /// <summary>
    /// İçeriğin kalite durumudur.
    /// 
    /// Tatoeba'dan gelen cümleleri otomatik parse ettiğimiz için Verified değil,
    /// Imported kabul ediyoruz.
    /// </summary>
    public ContentQualityStatus QualityStatus { get; init; } = ContentQualityStatus.Imported;

    /// <summary>
    /// Provider adıdır.
    /// 
    /// Örnek:
    /// Tatoeba
    /// </summary>
    public string SourceProvider { get; init; } = ImportConstants.ProviderNames.Tatoeba;

    /// <summary>
    /// Kaynak lisans bilgisidir.
    /// 
    /// Import edilen verinin attribution/license bilgisini ileride DB'ye taşımak için ayrıldı.
    /// </summary>
    public string? License { get; init; }

    /// <summary>
    /// Dış kaynakta bu eşleşmeyi takip etmek için kullanılan anahtardır.
    /// 
    /// Örnek:
    /// tatoeba:1277:1166752
    /// </summary>
    public string? ExternalSourceKey { get; init; }

    /// <summary>
    /// Source sentences dosyasında kaynak cümlenin geldiği satır numarasıdır.
    /// Debug ve import loglama için kullanılır.
    /// </summary>
    public int SourceSentenceRowNumber { get; init; }

    /// <summary>
    /// Target sentences dosyasında hedef cümlenin geldiği satır numarasıdır.
    /// Debug ve import loglama için kullanılır.
    /// </summary>
    public int TargetSentenceRowNumber { get; init; }

    /// <summary>
    /// Links dosyasında bu eşleşmenin geldiği satır numarasıdır.
    /// Hatalı linkleri teşhis etmek için önemlidir.
    /// </summary>
    public int LinkRowNumber { get; init; }
}