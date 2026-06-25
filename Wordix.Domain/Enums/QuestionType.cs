namespace Wordix.Domain.Enums;

/// <summary>
/// Quiz içinde sorulan sorunun tipini temsil eder.
/// 
/// QuizType genel quiz oturumunu,
/// QuestionType ise tek bir sorunun nasıl sorulduğunu temsil eder.
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Çoktan seçmeli soru.
    /// </summary>
    MultipleChoice = 1,

    /// <summary>
    /// Kullanıcının cevabı elle yazdığı soru.
    /// </summary>
    Writing = 2,

    /// <summary>
    /// Kaynak dilden hedef dile çeviri sorusu.
    /// Örnek: achieve kelimesinin Türkçesini yaz.
    /// </summary>
    TranslateToTargetLanguage = 3,

    /// <summary>
    /// Hedef dilden kaynak dile çeviri sorusu.
    /// Örnek: başarmak kelimesinin İngilizcesini yaz.
    /// </summary>
    TranslateToSourceLanguage = 4
}