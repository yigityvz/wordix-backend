using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Deck içindeki tek bir itemın response modelidir.
/// 
/// DeckItem, UserLearningItem üzerinden çalışır.
/// Bu yüzden response içinde hem DeckItemId hem UserLearningItemId hem de LearningItem bilgileri döner.
/// </summary>
public sealed class DeckItemResponse
{
    /// <summary>
    /// DeckItem entity id değeridir.
    /// </summary>
    public Guid DeckItemId { get; init; }

    /// <summary>
    /// Kullanıcının dictionary item id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer item Word ise Word id değeridir.
    /// </summary>
    public Guid? WordId { get; init; }

    /// <summary>
    /// Eğer item Phrase ise Phrase id değeridir.
    /// </summary>
    public Guid? PhraseId { get; init; }

    /// <summary>
    /// Eğer item Sentence ise Sentence id değeridir.
    /// </summary>
    public Guid? SentenceId { get; init; }

    /// <summary>
    /// İçerik tipi.
    /// Örnek: Word, Phrase, Sentence
    /// </summary>
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcıya gösterilecek ana metin.
    /// </summary>
    public string DisplayText { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş ana metin.
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodu.
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Word/Phrase için seçili veya primary meaning.
    /// Sentence itemlarında null olur.
    /// </summary>
    public UserDictionaryMeaningResponse? SelectedMeaning { get; init; }

    /// <summary>
    /// Sentence için ana çeviri.
    /// Word/Phrase itemlarında null olur.
    /// </summary>
    public UserDictionarySentenceTranslationResponse? SentenceTranslation { get; init; }

    /// <summary>
    /// Item'ın deck'e eklenme zamanı.
    /// </summary>
    public DateTime AddedAt { get; init; }
}