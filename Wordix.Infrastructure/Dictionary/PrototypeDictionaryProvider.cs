using Wordix.Application.Features.Lookups.Models;
using Wordix.Application.Features.Lookups.Services;

namespace Wordix.Infrastructure.Dictionary;

/// <summary>
/// Faz 13 için kullanılan basit prototype dictionary provider implementation'ıdır.
/// 
/// Bu provider ne yapar?
/// - Dış API çağırmaz.
/// - Import sistemi kurmaz.
/// - Sadece birkaç sabit kelime için provider sonucu döndürür.
/// 
/// Neden Infrastructure katmanında?
/// - IDictionaryProvider interface'i Application'dadır.
/// - Provider implementation teknik/dış kaynak adaptörü olarak Infrastructure'da tutulur.
/// - Faz 24'te gerçek provider/import sistemi geldiğinde bu class değiştirilebilir veya kaldırılabilir.
/// 
/// Önemli:
/// Bu mock kullanıcı veya fake authentication değildir.
/// Auth, API, DB akışı gerçek kalır.
/// Sadece dış dictionary provider henüz hazır olmadığı için prototype data adaptörü kullanıyoruz.
/// </summary>
public sealed class PrototypeDictionaryProvider : IDictionaryProvider
{
    private const string ProviderName = "PrototypeProvider";

    /// <summary>
    /// Prototype provider için desteklenen kelimeler.
    /// 
    /// Key:
    /// Normalize edilmiş text.
    /// 
    /// Value:
    /// Provider'ın döndüreceği meaning listesi.
    /// 
    /// Bu listeyi büyük tutmuyoruz çünkü Faz 24'te gerçek provider/import sistemi ayrıca tasarlanacak.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<DictionaryProviderMeaning>> PrototypeWords
        = new Dictionary<string, IReadOnlyCollection<DictionaryProviderMeaning>>(StringComparer.OrdinalIgnoreCase)
        {
            ["study"] = new[]
            {
                new DictionaryProviderMeaning
                {
                    Translation = "çalışmak",
                    Definition = "To learn about a subject by reading, practicing, or going to school.",
                    ExampleSentence = "I study English every day.",
                    PartOfSpeech = "verb"
                }
            },
            ["learn"] = new[]
            {
                new DictionaryProviderMeaning
                {
                    Translation = "öğrenmek",
                    Definition = "To get knowledge or skill in a new subject or activity.",
                    ExampleSentence = "I want to learn new words.",
                    PartOfSpeech = "verb"
                }
            },
            ["remember"] = new[]
            {
                new DictionaryProviderMeaning
                {
                    Translation = "hatırlamak",
                    Definition = "To bring a fact or experience back into your mind.",
                    ExampleSentence = "I remember this word now.",
                    PartOfSpeech = "verb"
                }
            }
        };

    /// <summary>
    /// Normalize edilmiş kelime için prototype provider içinde anlam arar.
    /// </summary>
    public Task<DictionaryProviderResult> FindAsync(
        string normalizedText,
        string sourceLanguageCode,
        string targetLanguageCode,
        CancellationToken cancellationToken = default)
    {
        // Faz 13 prototype provider sadece en → tr lookup destekler.
        // Farklı dil çifti gelirse sonuç bulunamadı kabul ediyoruz.
        if (!IsSupportedLanguagePair(sourceLanguageCode, targetLanguageCode))
        {
            return Task.FromResult(DictionaryProviderResult.NotFound(
                normalizedText: normalizedText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                providerName: ProviderName));
        }

        // Prototype sözlüğümüzde kelime var mı kontrol ediyoruz.
        if (!PrototypeWords.TryGetValue(normalizedText, out var meanings))
        {
            return Task.FromResult(DictionaryProviderResult.NotFound(
                normalizedText: normalizedText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                providerName: ProviderName));
        }

        // Kelime bulunduysa provider result dönüyoruz.
        return Task.FromResult(DictionaryProviderResult.FoundResult(
            normalizedText: normalizedText,
            sourceLanguageCode: sourceLanguageCode,
            targetLanguageCode: targetLanguageCode,
            providerName: ProviderName,
            meanings: meanings));
    }

    /// <summary>
    /// Prototype provider'ın desteklediği dil çiftini kontrol eder.
    /// 
    /// İlk prototip:
    /// en → tr
    /// </summary>
    private static bool IsSupportedLanguagePair(
        string sourceLanguageCode,
        string targetLanguageCode)
    {
        return string.Equals(sourceLanguageCode, "en", StringComparison.OrdinalIgnoreCase)
            && string.Equals(targetLanguageCode, "tr", StringComparison.OrdinalIgnoreCase);
    }
}