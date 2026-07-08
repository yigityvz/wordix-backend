namespace Wordix.Domain.Enums;

/// <summary>
/// Sistemde öğrenilebilir içerik tipini temsil eder.
/// 
/// Wordix sadece kelime odaklı kurulmadığı için,
/// Word, Phrase ve Sentence gibi içerikleri ortak LearningItem çatısı altında yönetiriz.
/// </summary>
public enum LearningItemType
{

    Unknown = 0,

    /// <summary>
    /// Tekil kelime.
    /// Örnek: achieve, improve, struggle
    /// </summary>
    Word = 1,

    /// <summary>
    /// Kalıp ifade veya phrase.
    /// Örnek: give up, look after, by the way
    /// </summary>
    Phrase = 2,

    /// <summary>
    /// Tam cümle.
    /// Örnek: I have been working on this project for two weeks.
    /// </summary>
    Sentence = 3
}