namespace Wordix.Application.Features.Quizzes.Dtos.Requests;

/// <summary>
/// Kullanıcının quiz başlatmak için API'ye göndereceği request modelidir.
/// 
/// Bu model HTTP request body contract'ıdır.
/// Controller bu modeli alır ve StartQuizCommand'e manual map eder.
/// 
/// İlk prototipte desteklenen değerler:
/// - QuizType: Test
/// - QuizSourceType: Dictionary
/// - QuizContentMode: WordsOnly, PhrasesOnly, Mixed
/// - SentencesOnly Faz 19'a kadar desteklenmez.
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


    /// <summary>
    /// Quiz bir deck üzerinden başlatılacaksa kullanılacak DeckId değeridir.
    /// 
    /// Kurallar:
    /// - QuizSourceType = Deck ise zorunludur.
    /// - QuizSourceType = UserDictionary ise kullanılmaz.
    /// 
    /// Bu alan current user'ın kendi deck id değeri olmalıdır.
    /// Başka kullanıcıya ait deck ile quiz başlatılamaz.
    /// </summary>
    public Guid? DeckId { get; init; }


    /// <summary>
    /// Bu quiz içine sistem önerisi itemlar dahil edilsin mi?
    /// 
    /// Nullable olmasının sebebi:
    /// - true  => Bu quiz için sistem önerilerini açıkça aç.
    /// - false => Bu quiz için sistem önerilerini açıkça kapat.
    /// - null  => Kullanıcının UserPreference.IncludeSystemRecommendations ayarı kullanılsın.
    /// 
    /// Faz 23 kararı:
    /// Bu alan ilk aşamada UserDictionary ve Deck quizlerine sistem önerisi karıştırmak için kullanılacak.
    /// QuizSourceType = SystemRecommendations saf kaynak akışı şimdilik açılmayacak.
    /// </summary>
    public bool? IncludeSystemRecommendations { get; init; }
}
