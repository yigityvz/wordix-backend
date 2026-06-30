using MediatR;
using Wordix.Application.Features.Lookups.Dtos.Responses;

namespace Wordix.Application.Features.Lookups.Commands.CreateLookup;

/// <summary>
/// Kullanıcının kelime/phrase/cümle arama isteğini temsil eden command modelidir.
/// 
/// Command nedir?
/// - Sistemde bir aksiyon başlatan istektir.
/// - Genelde database'e kayıt atabilir veya sistem durumunu değiştirebilir.
/// 
/// Bu command neden var?
/// - Lookup işlemi sadece veri okumaz.
/// - Kullanıcının arama geçmişini LookupHistory olarak kaydeder.
/// - Database'de olmayan içerik ileride provider üzerinden oluşturulabilir.
/// - Bu yüzden lookup işlemi CQRS açısından Command olarak modellenir.
/// 
/// Faz 12'de bu command sadece iskelet olarak oluşturulmuştu.
/// Faz 13'te artık gerçek LookupResponse dönecek hale getiriyoruz.
/// </summary>
public sealed record CreateLookupCommand : IRequest<LookupResponse>
{
    /// <summary>
    /// Kullanıcının arattığı ham metindir.
    /// 
    /// Örnek:
    /// - achieve
    /// - give up
    /// - I want to improve my English.
    /// 
    /// Bu değer handler içinde doğrudan kullanılmadan önce normalize edilecek.
    /// Örnek:
    /// " Achieve " → "achieve"
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının aradığı metnin kaynak dil kodudur.
    /// 
    /// İlk prototipte çoğunlukla:
    /// en
    /// 
    /// Örnek:
    /// Kullanıcı İngilizce kelime arıyorsa sourceLanguageCode = "en"
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının anlam/çeviri görmek istediği hedef dil kodudur.
    /// 
    /// İlk prototipte çoğunlukla:
    /// tr
    /// 
    /// Örnek:
    /// Kullanıcı İngilizce kelimenin Türkçe anlamını istiyorsa targetLanguageCode = "tr"
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

}
