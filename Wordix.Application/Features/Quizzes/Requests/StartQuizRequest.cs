namespace Wordix.Application.Features.Quizzes.Requests;

/// <summary>
/// Kullanıcının quiz başlatmak için API'ye göndereceği request modelidir.
/// 
/// Bu model HTTP request body contract'ıdır.
/// Controller bu modeli alır ve StartQuizCommand'e manual map eder.
/// 
/// İlk prototipte desteklenen değerler:
/// - QuizType: Test
/// - QuizSourceType: Dictionary
/// - QuizContentMode: WordsOnly
/// 
/// Örnek JSON:
/// {
///   "quizType": "Test",
///   "quizSourceType": "Dictionary",
///   "quizContentMode": "WordsOnly",
///   "questionCount": 5
/// }
/// </summary>
public sealed class StartQuizRequest
{
    /// <summary>
    /// Quiz türüdür.
    /// 
    /// İlk prototipte sadece Test desteklenir.
    /// İleride Flashcard, Writing, Listening gibi quiz türleri eklenebilir.
    /// </summary>
    public string QuizType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz sorularının hangi kaynaktan üretileceğini belirtir.
    /// 
    /// İlk prototipte sadece kullanıcının dictionary'sinden soru üretilecek.
    /// Bu yüzden desteklenen değer:
    /// Dictionary
    /// </summary>
    public string QuizSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz içinde hangi içerik türünün kullanılacağını belirtir.
    /// 
    /// İlk prototipte sadece kelime bazlı soru üretilecek.
    /// Bu yüzden desteklenen değer:
    /// WordsOnly
    /// </summary>
    public string QuizContentMode { get; init; } = string.Empty;

    /// <summary>
    /// Quiz içinde kaç soru üretileceğini belirtir.
    /// 
    /// Örnek:
    /// 5
    /// 
    /// Validator aşamasında minimum ve maksimum sınırlar kontrol edilecek.
    /// </summary>
    public int QuestionCount { get; init; }
}