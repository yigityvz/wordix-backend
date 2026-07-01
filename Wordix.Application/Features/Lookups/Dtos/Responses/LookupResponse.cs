namespace Wordix.Application.Features.Lookups.Dtos.Responses;

/// <summary>
/// Lookup işleminin başarılı sonucunda API'ye dönecek response modelidir.
/// 
/// Bu model frontend'e şunları anlatır:
/// - Hangi LearningItem bulundu/oluşturuldu?
/// - Bu LearningItem bir Word mü, Phrase mi, Sentence mı?
/// - Aranan metnin normalize edilmiş hali nedir?
/// - Sonuç database'den mi geldi, provider'dan mı?
/// - Kullanıcının dictionary'sinde zaten var mı?
/// - Hangi anlamlar bulundu?
/// </summary>
public sealed class LookupResponse
{
    /// <summary>
    /// Lookup sonucunda bulunan veya oluşturulan LearningItem id değeridir.
    /// 
    /// Wordix'te Word/Phrase/Sentence gibi öğrenilebilir her içerik LearningItem çatısı altında tutulur.
    /// Wordix'te Word/Phrase/Sentence gibi öğrenilebilir her içerik LearningItem çatısı altında tutulur.
    /// Faz 18 itibarıyla Word ve Phrase aktif olarak desteklenir..
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer lookup sonucu bir Word ise Word entity'sinin id değeridir.
    /// Word lookup sonucunda dolu olur.
    /// Phrase lookup sonucunda null olur.
    /// </summary>
    public Guid? WordId { get; init; }


    /// <summary>
    /// Eğer lookup sonucu bir Phrase ise Phrase entity'sinin id değeridir.
    /// 
    /// Word lookup sonucunda null olur.
    /// Phrase lookup sonucunda dolu olur.
    /// 
    /// Bu alan frontend'in içerik detay tipini ayırt etmesini kolaylaştırır.
    /// Ana öğrenme akışı yine LearningItemId üzerinden yürür.
    /// </summary>
    public Guid? PhraseId { get; init; }


    /// <summary>
    /// Oluşturulan LookupHistory kaydının id değeridir.
    /// 
    /// Lookup işlemi sadece veri okuma değildir.
    /// Kullanıcının arama geçmişi LookupHistory olarak kaydedilir.
    /// </summary>
    public Guid LookupHistoryId { get; init; }

    /// <summary>
    /// Kullanıcının gönderdiği orijinal metindir.
    /// 
    /// Örnek:
    /// " Achieve "
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş metindir.
    /// 
    /// Örnek:
    /// " Achieve " → "achieve"
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Lookup sonucunun içerik tipidir.
    /// Aktif desteklenen değerler:
    /// Word
    /// Phrase
    /// 
    /// Sentence enum/model tarafında düşünülmüştür ama Faz 19'a kadar aktif değildir.
    /// </summary>
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Lookup sonucunun nereden geldiğini gösterir.
    /// 
    /// Örnek:
    /// - Database
    /// - PrototypeProvider
    /// 
    /// Bu bilgi debugging ve ürün davranışı takibi için faydalıdır.
    /// </summary>
    public string LookupSource { get; init; } = string.Empty;

    /// <summary>
    /// Bu learning item kullanıcının dictionary'sinde zaten var mı bilgisidir.
    /// 
    /// Lookup yapmak, otomatik olarak dictionary'ye kaydetmek anlamına gelmez.
    /// Kullanıcı isterse sonraki fazda SaveToDictionary endpointiyle kaydeder.
    /// </summary>
    public bool IsAlreadyInUserDictionary { get; init; }

    /// <summary>
    /// Lookup sonucunda bulunan anlam listesidir.
    /// 
    /// Bir kelimenin birden fazla anlamı olabileceği için koleksiyon olarak tutulur.
    /// </summary>
    public IReadOnlyCollection<LookupMeaningResponse> Meanings { get; init; }
        = Array.Empty<LookupMeaningResponse>();
}
