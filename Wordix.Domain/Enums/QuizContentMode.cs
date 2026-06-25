namespace Wordix.Domain.Enums;

/// <summary>
/// Quiz içinde hangi içerik tiplerinin kullanılacağını temsil eder.
/// </summary>
public enum QuizContentMode
{
    /// <summary>
    /// Sadece kelimelerden quiz oluşturulur.
    /// </summary>
    WordsOnly = 1,

    /// <summary>
    /// Sadece phrase/expression içeriklerinden quiz oluşturulur.
    /// </summary>
    PhrasesOnly = 2,

    /// <summary>
    /// Sadece cümlelerden quiz oluşturulur.
    /// </summary>
    SentencesOnly = 3,

    /// <summary>
    /// Word, Phrase ve Sentence karışık kullanılabilir.
    /// </summary>
    Mixed = 4
}