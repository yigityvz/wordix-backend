using Wordix.Application.Common.Constants;

namespace Wordix.Application.Common.Models.Translation;

/// <summary>
/// Azure Translator gibi dış çeviri provider'larına gönderilecek standart çeviri isteğini temsil eder.
/// 
/// Bu model neden var?
/// - Azure'ın kendi request formatı Infrastructure detaydır.
/// - Application katmanı Azure request body'sini bilmemelidir.
/// - Lookup, import veya sentence translation gibi use-case'ler
///   aynı standart TranslationRequest modelini kullanabilmelidir.
/// </summary>
public sealed record TranslationRequest
{
    /// <summary>
    /// Çevrilecek metindir.
    /// 
    /// Örnek:
    /// - "abandon"
    /// - "give up"
    /// - "I abandoned the project."
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// Wordix'in ana akışında şu an İngilizce için "en" kullanıyoruz.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef dil kodudur.
    /// Wordix'in ana akışında Türkçe için "tr" kullanıyoruz.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Çevirinin hangi amaçla istendiğini belirtir.
    /// 
    /// Örnek:
    /// - "word-lookup-fallback"
    /// - "phrase-lookup-fallback"
    /// - "sentence-translation"
    /// 
    /// Azure tarafında bu alanı doğrudan göndermeyebiliriz.
    /// Ama loglama, debug ve ileride provider request log için faydalıdır.
    /// </summary>
    public string? Purpose { get; init; }

    /// <summary>
    /// Opsiyonel bağlam metnidir.
    /// 
    /// Örneğin phrase çevirisinde ileride şunu kullanabiliriz:
    /// "Translate this as a dictionary meaning, not as a full sentence."
    /// 
    /// İlk Azure implementasyonunda bu alanı kullanmak zorunda değiliz.
    /// Ama modelde hazır tutmak ileride genişletmeyi kolaylaştırır.
    /// </summary>
    public string? Context { get; init; }
}