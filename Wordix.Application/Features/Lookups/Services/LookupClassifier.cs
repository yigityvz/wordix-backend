using Wordix.Application.Features.Lookups.Models;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Lookup input'unu basit kurallarla Word/Phrase/Sentence olarak sınıflandırır.
/// 
/// İlk prototipte amaç mükemmel NLP yapmak değildir.
/// Amaç:
/// - Tek kelimeleri Word olarak ayırmak.
/// - Birden fazla kelimeli kısa ifadeleri Phrase olarak ayırmak.
/// - Noktalama veya daha uzun yapılar içerenleri Sentence olarak ayırmak.
/// 
/// Faz 13'te gerçek lookup desteği sadece Word için yazılacak.
/// Phrase/Sentence tespit edilirse şimdilik BusinessRuleException ile desteklenmediği söylenecek.
/// </summary>
public sealed class LookupClassifier : ILookupClassifier
{
    /// <summary>
    /// Bir metnin phrase sayılması için izin verilen maksimum kelime sayısı.
    /// 
    /// Örnek:
    /// "give up" → 2 kelime → Phrase
    /// "look after" → 2 kelime → Phrase
    /// "take care of" → 3 kelime → Phrase
    /// 
    /// Daha uzun yapıları cümle kabul ediyoruz.
    /// Bu kural ileride geliştirilebilir.
    /// </summary>
    private const int MaximumPhraseWordCount = 4;

    /// <summary>
    /// Normalize edilmiş text değerini Word/Phrase/Sentence olarak sınıflandırır.
    /// </summary>
    public LookupInputType Classify(string normalizedText)
    {
        // Güvenli tarafta kalmak için null değerini boş string gibi ele alıyoruz.
        // Validator zaten boş text'i engelleyecek.
        var safeText = normalizedText ?? string.Empty;

        // Baştaki/sondaki boşluklar normalizer tarafından temizlenmiş olmalı.
        // Yine de classifier tek başına da dayanıklı olsun.
        var text = safeText.Trim();

        // Boş text için Word/Phrase/Sentence kararı vermek anlamlı değildir.
        // Fakat validation bunu yakalayacağı için burada özel exception atmıyoruz.
        // Handler tarafı validation sonrası çalışacağı için buraya normalde boş değer gelmez.
        if (string.IsNullOrWhiteSpace(text))
        {
            return LookupInputType.Word;
        }

        // Nokta, soru işareti veya ünlem gibi cümle bitiş işaretleri varsa Sentence kabul ediyoruz.
        if (ContainsSentenceEndingPunctuation(text))
        {
            return LookupInputType.Sentence;
        }

        // Kelime sayısını hesaplıyoruz.
        var wordCount = CountWords(text);

        // Tek kelime ise Word.
        if (wordCount <= 1)
        {
            return LookupInputType.Word;
        }

        // 2-4 kelimelik kısa yapıları Phrase kabul ediyoruz.
        if (wordCount <= MaximumPhraseWordCount)
        {
            return LookupInputType.Phrase;
        }

        // Daha uzun yapıları Sentence kabul ediyoruz.
        return LookupInputType.Sentence;
    }

    /// <summary>
    /// Text içinde cümle bitiş noktalaması var mı kontrol eder.
    /// </summary>
    private static bool ContainsSentenceEndingPunctuation(string text)
    {
        return text.Contains('.')
            || text.Contains('?')
            || text.Contains('!');
    }

    /// <summary>
    /// Text içindeki kelime sayısını hesaplar.
    /// 
    /// Normalize edilmiş text'te birden fazla boşluk tek boşluğa düşürülmüş olmalı.
    /// Yine de RemoveEmptyEntries ile güvenli davranıyoruz.
    /// </summary>
    private static int CountWords(string text)
    {
        return text.Split(
                separator: ' ',
                options: StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}