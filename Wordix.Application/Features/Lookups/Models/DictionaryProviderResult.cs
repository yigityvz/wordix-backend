namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Dictionary provider lookup sonucunu temsil eder.
/// 
/// Bu model provider'dan gelen sonucu application katmanına taşır.
/// Provider sonucu doğrudan entity değildir.
/// Handler bu sonucu alıp gerekirse LearningItem, Word ve Meaning entity'lerine dönüştürür.
/// </summary>
public sealed class DictionaryProviderResult
{
    /// <summary>
    /// Provider'ın bu kelime için sonuç bulup bulmadığını gösterir.
    /// 
    /// true:
    /// Provider anlam buldu.
    /// 
    /// false:
    /// Provider bu kelime için sonuç bulamadı.
    /// </summary>
    public bool Found { get; init; }

    /// <summary>
    /// Provider'ın baktığı normalize edilmiş text değeridir.
    /// 
    /// Örnek:
    /// achieve
    /// improve
    /// perfect
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
    /// - ImportProvider
    /// 
    /// Faz 13'te PrototypeProvider kullanacağız.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Provider'dan gelen anlam listesidir.
    /// 
    /// Bir kelimenin birden fazla anlamı olabilir.
    /// </summary>
    public IReadOnlyCollection<DictionaryProviderMeaning> Meanings { get; init; }
        = Array.Empty<DictionaryProviderMeaning>();

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
            Meanings = Array.Empty<DictionaryProviderMeaning>()
        };
    }

    /// <summary>
    /// Sonuç bulunduğunda standart found result üretir.
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
            Meanings = meanings
        };
    }
}