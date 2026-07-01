namespace Wordix.Application.Features.Lookups.Dtos.Responses;

/// <summary>
/// Sentence lookup sonucunda dönen tek bir cümle çevirisini temsil eder.
/// 
/// Bu response neden LookupMeaningResponse'tan ayrı?
/// - LookupMeaningResponse Word/Phrase anlamları içindir.
/// - Sentence translation tam cümle çevirisidir.
/// - Cümle çevirisini "meaning" gibi göstermek domain kavramlarını karıştırır.
/// 
/// Faz 19 kararı:
/// Sentence lookup anında database'e kalıcı SentenceTranslation yazılmaz.
/// Bu yüzden SentenceTranslationId çoğu zaman null döner.
/// Kullanıcı cümleyi dictionary'ye kaydederse kalıcı SentenceTranslation save akışında oluşur.
/// </summary>
public sealed class LookupSentenceTranslationResponse
{
    /// <summary>
    /// Kalıcı SentenceTranslation entity id değeridir.
    /// 
    /// Sentence lookup anında kalıcı kayıt oluşturmadığımız için null olabilir.
    /// Dictionary'ye kaydedilmiş bir sentence ileride database'den dönerse dolu olabilir.
    /// </summary>
    public Guid? SentenceTranslationId { get; init; }

    /// <summary>
    /// Hedef dildeki tam cümle çevirisidir.
    /// 
    /// Örnek:
    /// İngilizcemi geliştirmek istiyorum.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Çeviriyi üreten provider adıdır.
    /// 
    /// Örnek:
    /// PrototypeProvider
    /// </summary>
    public string? SourceProvider { get; init; }

    /// <summary>
    /// Çeviri kaynağının lisans bilgisidir.
    /// 
    /// Prototype aşamasında null olabilir.
    /// Faz 24 provider/import sisteminde önemli hale gelecektir.
    /// </summary>
    public string? License { get; init; }
}