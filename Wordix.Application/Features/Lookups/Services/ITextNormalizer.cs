namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Lookup'a gelen ham text değerini standart hale getiren servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Normalizasyon kuralı handler içine gömülmesin.
/// - İleride farklı normalizasyon stratejileri kolayca değiştirilebilsin.
/// - Test edilebilirlik artsın.
/// </summary>
public interface ITextNormalizer
{
    /// <summary>
    /// Kullanıcının gönderdiği ham metni normalize eder.
    /// 
    /// Örnek:
    /// " Achieve " → "achieve"
    /// "  give   up " → "give up"
    /// </summary>
    /// <param name="text">Kullanıcının gönderdiği ham text.</param>
    /// <returns>Normalize edilmiş text.</returns>
    string Normalize(string text);
}