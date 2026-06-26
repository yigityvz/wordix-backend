namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Generator tarafından üretilen tek bir quiz seçeneğini temsil eder.
/// 
/// Bu model henüz QuizOption entity değildir.
/// Handler bu modeli alıp QuizOption entity'sine dönüştürecek.
/// </summary>
public sealed class GeneratedQuizOption
{
    /// <summary>
    /// Seçenek metnidir.
    /// 
    /// İlk prototipte Türkçe anlam olur.
    /// Örnek:
    /// başarmak
    /// çalışmak
    /// öğrenmek
    /// geliştirmek
    /// </summary>
    public string OptionText { get; init; } = string.Empty;

    /// <summary>
    /// Bu seçenek doğru cevap mı?
    /// 
    /// Önemli:
    /// Bu bilgi sadece backend içinde kullanılacak.
    /// API response'a IsCorrect gönderilmeyecek.
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Seçenek sırasıdır.
    /// 
    /// Örnek:
    /// 1, 2, 3, 4
    /// </summary>
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Eğer seçenek bir Meaning kaydından geliyorsa onun id değeridir.
    /// 
    /// Doğru seçenek için genellikle dolu olur.
    /// Yanlış seçenekler de başka Meaning kayıtlarından üretileceği için dolu olabilir.
    /// </summary>
    public Guid? MeaningId { get; init; }
}