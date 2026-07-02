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
    /// 
    /// Faz 21 Writing Quiz kararında aktif kullanılmaz.
    /// Çünkü writing quizde eksik/kısmi cevap yanlış kabul edilir.
    /// Future-ready olarak tutulur; ileride farklı quiz tiplerinde kullanılabilir.
    /// </summary>
    PartiallyCorrect = 3,

    /// <summary>
    /// Kullanıcı soruyu boş geçti.
    /// </summary>
    Skipped = 4
}