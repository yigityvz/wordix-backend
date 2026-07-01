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
    /// Prototype provider için desteklenen phrase / kalıp ifadeler.
    /// 
    /// Bu veriler gerçek provider değildir.
    /// Ama lookup + database + dictionary + quiz akışını gerçek backend üzerinden test edebilmemiz için
    /// küçük bir geliştirme verisi sağlar.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<DictionaryProviderMeaning>> PrototypePhrases
        = new Dictionary<string, IReadOnlyCollection<DictionaryProviderMeaning>>(StringComparer.OrdinalIgnoreCase)
        {
            ["give up"] = new[]
            {
            new DictionaryProviderMeaning
            {
                Translation = "vazgeçmek",
                Definition = "To stop trying to do something.",
                ExampleSentence = "Do not give up on your goals.",
                PartOfSpeech = "phrasal verb"
            }
            },
            ["look after"] = new[]
            {
            new DictionaryProviderMeaning
            {
                Translation = "ilgilenmek",
                Definition = "To take care of someone or something.",
                ExampleSentence = "I look after my little brother.",
                PartOfSpeech = "phrasal verb"
            }
            },
            ["by the way"] = new[]
            {
            new DictionaryProviderMeaning
            {
                Translation = "bu arada",
                Definition = "Used to introduce a new or additional point.",
                ExampleSentence = "By the way, I finished the task.",
                PartOfSpeech = "expression"
            }
            },
            ["take care of"] = new[]
            {
            new DictionaryProviderMeaning
            {
                Translation = "ilgilenmek",
                Definition = "To care for or be responsible for someone or something.",
                ExampleSentence = "I will take care of this problem.",
                PartOfSpeech = "expression"
            }
            },

            ["find out"] = new[]
        {
            new DictionaryProviderMeaning
            {
                Translation = "öğrenip bulmak",
                Definition = "To discover information.",
                ExampleSentence = "I want to find out the truth.",
                PartOfSpeech = "phrasal verb"
            }
        },
                    ["come across"] = new[]
        {
            new DictionaryProviderMeaning
            {
                Translation = "rastlamak",
                Definition = "To find or meet something by chance.",
                ExampleSentence = "I came across an old photo.",
                PartOfSpeech = "phrasal verb"
            }
        },
                    ["set up"] = new[]
        {
            new DictionaryProviderMeaning
            {
                Translation = "kurmak",
                Definition = "To prepare or arrange something for use.",
                ExampleSentence = "We need to set up the system.",
                PartOfSpeech = "phrasal verb"
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

        // Prototype sözlüğümüzde önce kelime var mı kontrol ediyoruz.
        if (PrototypeWords.TryGetValue(normalizedText, out var wordMeanings))
        {
            return Task.FromResult(DictionaryProviderResult.FoundResult(
                normalizedText: normalizedText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                providerName: ProviderName,
                meanings: wordMeanings));
        }

        // Kelime bulunamazsa phrase/veri havuzunda arıyoruz.
        // Provider burada entity tipi döndürmez.
        // Word mü Phrase mi kararını LookupClassifier + handler verir.
        if (PrototypePhrases.TryGetValue(normalizedText, out var phraseMeanings))
        {
            return Task.FromResult(DictionaryProviderResult.FoundResult(
                normalizedText: normalizedText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                providerName: ProviderName,
                meanings: phraseMeanings));
        }

        return Task.FromResult(DictionaryProviderResult.NotFound(
            normalizedText: normalizedText,
            sourceLanguageCode: sourceLanguageCode,
            targetLanguageCode: targetLanguageCode,
            providerName: ProviderName));

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