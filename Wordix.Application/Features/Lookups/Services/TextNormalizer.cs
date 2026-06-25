using System.Text.RegularExpressions;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Lookup metni için varsayılan normalizasyon servisidir.
/// 
/// İlk prototipte basit ve güvenli kurallar uyguluyoruz:
/// - Baştaki/sondaki boşlukları sil.
/// - Birden fazla boşluğu tek boşluğa indir.
/// - Metni küçük harfe çevir.
/// 
/// Örnek:
/// "  Achieve  " → "achieve"
/// "  GIVE    UP " → "give up"
/// </summary>
public sealed class TextNormalizer : ITextNormalizer
{
    /// <summary>
    /// Bir veya daha fazla whitespace karakterini yakalayan Regex.
    /// 
    /// Whitespace şunları kapsayabilir:
    /// - normal boşluk
    /// - tab
    /// - newline
    /// 
    /// Regex'i static readonly tuttuk çünkü her Normalize çağrısında yeniden oluşturulmasın.
    /// </summary>
    private static readonly Regex MultipleWhitespaceRegex = new(
        pattern: @"\s+",
        options: RegexOptions.Compiled);

    /// <summary>
    /// Kullanıcının gönderdiği ham text değerini normalize eder.
    /// </summary>
    public string Normalize(string text)
    {
        // Null gelirse string.Empty üzerinden devam ediyoruz.
        // Normalde validator Text alanını zorunlu yapacak.
        // Ama servis yine de null'a karşı dayanıklı olsun.
        var safeText = text ?? string.Empty;

        // Baştaki ve sondaki boşlukları temizliyoruz.
        var trimmedText = safeText.Trim();

        // Aradaki birden fazla boşluğu tek boşluğa indiriyoruz.
        var singleSpacedText = MultipleWhitespaceRegex.Replace(
            input: trimmedText,
            replacement: " ");

        // Lookup işlemlerinde karşılaştırma için küçük harf kullanıyoruz.
        // ToLowerInvariant kültürden bağımsız çalışır.
        return singleSpacedText.ToLowerInvariant();
    }
}