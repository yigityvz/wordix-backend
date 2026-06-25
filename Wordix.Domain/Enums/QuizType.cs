namespace Wordix.Domain.Enums;

/// <summary>
/// Quiz'in genel tipini temsil eder.
/// </summary>
public enum QuizType
{
    /// <summary>
    /// Çoktan seçmeli quiz.
    /// </summary>
    Test = 1,

    /// <summary>
    /// Kullanıcının cevabı kendisinin yazdığı quiz.
    /// </summary>
    Writing = 2,

    /// <summary>
    /// Test ve writing sorularının karışık kullanıldığı quiz.
    /// </summary>
    Mixed = 3
}