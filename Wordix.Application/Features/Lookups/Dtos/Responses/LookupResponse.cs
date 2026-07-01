namespace Wordix.Application.Features.Lookups.Dtos.Responses;

/// <summary>
/// Lookup işleminin başarılı sonucunda API'ye dönecek response modelidir.
/// 
/// Bu model frontend'e şunları anlatır:
/// - Hangi içerik bulundu/oluşturuldu?
/// - Bu içerik Word mü, Phrase mi, Sentence mı?
/// - Aranan metnin normalize edilmiş hali nedir?
/// - Sonuç database'den mi geldi, provider'dan mı?
/// - Kullanıcının dictionary'sinde zaten var mı?
/// - Word/Phrase için hangi anlamlar bulundu?
/// - Sentence için hangi çeviriler bulundu?
/// </summary>
public sealed class LookupResponse
{
    /// <summary>
    /// Lookup sonucunda bulunan veya oluşturulan LearningItem id değeridir.
    /// 
    /// Word/Phrase lookup sonucunda dolu olur.
    /// Çünkü Word/Phrase lookup provider'dan gelirse global içerik havuzuna kaydedilir.
    /// 
    /// Sentence lookup sonucunda null olabilir.
    /// Çünkü Faz 19 kararına göre sentence lookup sadece geçici translation sonucu döner.
    /// Sentence ancak kullanıcı dictionary'ye kaydetmek isterse kalıcı LearningItem olur.
    /// </summary>
    public Guid? LearningItemId { get; init; }

    /// <summary>
    /// Eğer lookup sonucu bir Word ise Word entity'sinin id değeridir.
    /// 
    /// Word lookup sonucunda dolu olur.
    /// Phrase/Sentence lookup sonucunda null olur.
    /// </summary>
    public Guid? WordId { get; init; }

    /// <summary>
    /// Eğer lookup sonucu bir Phrase ise Phrase entity'sinin id değeridir.
    /// 
    /// Phrase lookup sonucunda dolu olur.
    /// Word/Sentence lookup sonucunda null olur.
    /// 
    /// Ana öğrenme akışı yine LearningItemId üzerinden yürür.
    /// </summary>
    public Guid? PhraseId { get; init; }

    /// <summary>
    /// Eğer lookup sonucu bir Sentence ise Sentence entity'sinin id değeridir.
    /// 
    /// Sentence lookup anında kalıcı Sentence oluşturmadığımız için çoğu zaman null döner.
    /// Kullanıcı cümleyi dictionary'ye kaydederse Sentence save akışında kalıcı hale gelir.
    /// </summary>
    public Guid? SentenceId { get; init; }

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
    /// "I want to improve my English"
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş metindir.
    /// 
    /// Örnek:
    /// " Achieve " → "achieve"
    /// "I Want To Improve My English" → "i want to improve my english"
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Lookup sonucunun içerik tipidir.
    /// 
    /// Aktif desteklenen değerler:
    /// Word
    /// Phrase
    /// Sentence
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
    /// Word/Phrase için LearningItemId üzerinden kontrol edilebilir.
    /// Sentence lookup anında LearningItem oluşmadığı için false döner.
    /// Sentence dictionary'ye kaydedildiğinde ayrıca save endpointi çalışır.
    /// </summary>
    public bool IsAlreadyInUserDictionary { get; init; }

    /// <summary>
    /// Word/Phrase lookup sonucunda bulunan anlam listesidir.
    /// 
    /// Sentence lookup sonucunda boş döner.
    /// Çünkü sentence çevirileri Meaning değil, SentenceTranslation kavramıdır.
    /// </summary>
    public IReadOnlyCollection<LookupMeaningResponse> Meanings { get; init; }
        = Array.Empty<LookupMeaningResponse>();

    /// <summary>
    /// Sentence lookup sonucunda bulunan cümle çevirileridir.
    /// 
    /// Word/Phrase lookup sonucunda boş döner.
    /// </summary>
    public IReadOnlyCollection<LookupSentenceTranslationResponse> SentenceTranslations { get; init; }
        = Array.Empty<LookupSentenceTranslationResponse>();
}