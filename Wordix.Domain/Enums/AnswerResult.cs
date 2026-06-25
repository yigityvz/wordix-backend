namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcının quiz sorusuna verdiği cevabın sonucunu temsil eder.
/// </summary>
public enum AnswerResult
{
    /// <summary>
    /// Cevap doğru.
    /// </summary>
    Correct = 1,

    /// <summary>
    /// Cevap yanlış.
    /// </summary>
    Incorrect = 2,

    /// <summary>
    /// Cevap kısmen doğru.
    /// Özellikle writing quizlerde toleranslı değerlendirme için kullanılabilir.
    /// </summary>
    PartiallyCorrect = 3,

    /// <summary>
    /// Kullanıcı soruyu boş geçti.
    /// </summary>
    Skipped = 4
}