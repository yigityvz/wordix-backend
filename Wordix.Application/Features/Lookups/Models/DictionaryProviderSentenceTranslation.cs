namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Translation provider'dan dönen tek bir sentence translation bilgisini temsil eder.
/// 
/// Bu model entity değildir.
/// Provider sonucunu application katmanına taşır.
/// 
/// Neden DictionaryProviderMeaning kullanmıyoruz?
/// - DictionaryProviderMeaning Word/Phrase anlamları içindir.
/// - SentenceTranslation ise tam cümle çevirisidir.
/// - Cümle çevirisini Meaning gibi ele almak domain kavramlarını karıştırır.
/// 
/// Örnek:
/// Source:
/// I want to improve my English.
/// 
/// Translation:
/// İngilizcemi geliştirmek istiyorum.
/// </summary>
public sealed class DictionaryProviderSentenceTranslation
{
    /// <summary>
    /// Hedef dildeki tam cümle çevirisidir.
    /// 
    /// Örnek:
    /// Bu proje üzerinde iki haftadır çalışıyorum.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Çeviriyi üreten provider adıdır.
    /// 
    /// Prototype aşamasında genellikle PrototypeProvider olur.
    /// Faz 24'te gerçek provider/import sistemi geldiğinde daha anlamlı hale gelir.
    /// </summary>
    public string? SourceProvider { get; init; }

    /// <summary>
    /// Provider/import kaynağının lisans bilgisidir.
    /// 
    /// Prototype aşamasında null kalabilir.
    /// Faz 24'te Tatoeba/LibreTranslate/başka kaynaklar konuşulurken önemli olacaktır.
    /// </summary>
    public string? License { get; init; }
}