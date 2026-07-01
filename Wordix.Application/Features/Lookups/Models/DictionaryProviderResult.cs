namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Dictionary/translation provider lookup sonucunu temsil eder.
/// 
/// Bu model provider'dan gelen sonucu application katmanına taşır.
/// Provider sonucu doğrudan entity değildir.
/// Handler bu sonucu alıp gerekirse entity'lere dönüştürür.
/// 
/// Faz 19 kararı:
/// - Word/Phrase için provider sonucu Meanings koleksiyonunu doldurur.
/// - Sentence için provider sonucu SentenceTranslations koleksiyonunu doldurur.
/// - Sentence lookup anında LearningItem/Sentence/SentenceTranslation entity oluşturulmaz.
///   Kalıcı kayıt sadece kullanıcı dictionary'ye kaydetmek isterse oluşturulur.
/// </summary>
public sealed class DictionaryProviderResult
{
    /// <summary>
    /// Provider'ın bu lookup için sonuç bulup bulmadığını gösterir.
    /// 
    /// Word/Phrase için:
    /// Meanings doluysa true olur.
    /// 
    /// Sentence için:
    /// SentenceTranslations doluysa true olur.
    /// </summary>
    public bool Found { get; init; }

    /// <summary>
    /// Provider'ın baktığı normalize edilmiş text değeridir.
    /// 
    /// Örnek:
    /// achieve
    /// give up
    /// i want to improve my english
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

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
    /// Sonucu üreten provider adıdır.
    /// 
    /// Örnek:
    /// - PrototypeProvider
    /// - ExternalDictionaryApi
    /// - TranslationApi
    /// - ImportProvider
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Provider'dan gelen Word/Phrase anlam listesidir.
    /// 
    /// Word/Phrase lookup için kullanılır.
    /// Sentence lookup için boş kalır.
    /// </summary>
    public IReadOnlyCollection<DictionaryProviderMeaning> Meanings { get; init; }
        = Array.Empty<DictionaryProviderMeaning>();

    /// <summary>
    /// Provider'dan gelen sentence translation listesidir.
    /// 
    /// Sentence lookup için kullanılır.
    /// Word/Phrase lookup için boş kalır.
    /// 
    /// Bu alanı ayrı tutuyoruz çünkü sentence çevirisi,
    /// Word/Phrase anlamı ile aynı domain kavramı değildir.
    /// </summary>
    public IReadOnlyCollection<DictionaryProviderSentenceTranslation> SentenceTranslations { get; init; }
        = Array.Empty<DictionaryProviderSentenceTranslation>();

    /// <summary>
    /// Sonuç bulunamadığında standart empty result üretir.
    /// 
    /// Bu factory method neden var?
    /// - Her yerde new DictionaryProviderResult { Found = false, ... } yazmamak için.
    /// - Bulunamadı sonucunun standart olmasını sağlamak için.
    /// </summary>
    public static DictionaryProviderResult NotFound(
        string normalizedText,
        string sourceLanguageCode,
        string targetLanguageCode,
        string providerName)
    {
        return new DictionaryProviderResult
        {
            Found = false,
            NormalizedText = normalizedText,
            SourceLanguageCode = sourceLanguageCode,
            TargetLanguageCode = targetLanguageCode,
            ProviderName = providerName,
            Meanings = Array.Empty<DictionaryProviderMeaning>(),
            SentenceTranslations = Array.Empty<DictionaryProviderSentenceTranslation>()
        };
    }

    /// <summary>
    /// Word/Phrase sonucu bulunduğunda standart found result üretir.
    /// 
    /// Bu method mevcut Word/Phrase akışını bozmamak için korunur.
    /// Sentence için SentenceFoundResult kullanılmalıdır.
    /// </summary>
    public static DictionaryProviderResult FoundResult(
        string normalizedText,
        string sourceLanguageCode,
        string targetLanguageCode,
        string providerName,
        IReadOnlyCollection<DictionaryProviderMeaning> meanings)
    {
        return new DictionaryProviderResult
        {
            Found = true,
            NormalizedText = normalizedText,
            SourceLanguageCode = sourceLanguageCode,
            TargetLanguageCode = targetLanguageCode,
            ProviderName = providerName,
            Meanings = meanings,
            SentenceTranslations = Array.Empty<DictionaryProviderSentenceTranslation>()
        };
    }

    /// <summary>
    /// Sentence translation sonucu bulunduğunda standart found result üretir.
    /// 
    /// Sentence lookup, Word/Phrase lookup'tan farklıdır:
    /// - Meanings oluşturmaz.
    /// - SentenceTranslations döner.
    /// - Lookup anında database'e Sentence/SentenceTranslation yazılmaz.
    /// </summary>
    public static DictionaryProviderResult SentenceFoundResult(
        string normalizedText,
        string sourceLanguageCode,
        string targetLanguageCode,
        string providerName,
        IReadOnlyCollection<DictionaryProviderSentenceTranslation> sentenceTranslations)
    {
        return new DictionaryProviderResult
        {
            Found = true,
            NormalizedText = normalizedText,
            SourceLanguageCode = sourceLanguageCode,
            TargetLanguageCode = targetLanguageCode,
            ProviderName = providerName,
            Meanings = Array.Empty<DictionaryProviderMeaning>(),
            SentenceTranslations = sentenceTranslations
        };
    }
}