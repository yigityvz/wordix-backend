namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary'sindeki Sentence item için gösterilecek cümle çevirisini temsil eder.
/// 
/// Neden UserDictionaryMeaningResponse'tan ayrı?
/// - Meaning, Word/Phrase anlamları içindir.
/// - SentenceTranslation tam cümle çevirisidir.
/// - Sentence çevirisini Meaning gibi göstermek domain kavramlarını karıştırır.
/// </summary>
public sealed class UserDictionarySentenceTranslationResponse
{
    /// <summary>
    /// SentenceTranslation entity id değeridir.
    /// </summary>
    public Guid SentenceTranslationId { get; init; }

    /// <summary>
    /// Hedef dildeki cümle çevirisidir.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Bu çeviri primary çeviri mi?
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Gösterim sırası.
    /// </summary>
    public int DisplayOrder { get; init; }
}