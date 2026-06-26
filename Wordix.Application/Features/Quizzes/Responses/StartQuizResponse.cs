namespace Wordix.Application.Features.Quizzes.Responses;

/// <summary>
/// Quiz başlatıldığında API'ye dönecek ana response modelidir.
/// 
/// Bu response:
/// - Oluşturulan quiz session bilgisini
/// - Quiz ayarlarını
/// - Oluşturulan soru listesini
/// frontend'e döndürür.
/// </summary>
public sealed class StartQuizResponse
{
    /// <summary>
    /// Oluşturulan QuizSession id değeridir.
    /// 
    /// Sonraki fazlarda cevap gönderme ve quiz bitirme endpointlerinde kullanılacak.
    /// </summary>
    public Guid QuizSessionId { get; init; }

    /// <summary>
    /// Quiz türüdür.
    /// 
    /// İlk prototipte:
    /// Test
    /// </summary>
    public string QuizType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz kaynağıdır.
    /// 
    /// İlk prototipte:
    /// Dictionary
    /// </summary>
    public string QuizSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz içerik modudur.
    /// 
    /// İlk prototipte:
    /// WordsOnly
    /// </summary>
    public string QuizContentMode { get; init; } = string.Empty;

    /// <summary>
    /// Oluşturulan soru sayısıdır.
    /// 
    /// Kullanıcı 10 istese bile dictionary'de yeterli item yoksa daha az üretilebilir.
    /// Bu yüzden actual question count response'ta ayrıca döndürülür.
    /// </summary>
    public int QuestionCount { get; init; }

    /// <summary>
    /// Quiz session başlangıç zamanıdır.
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Quiz session durumudur.
    /// 
    /// Örnek:
    /// Started
    /// InProgress
    /// Active
    /// 
    /// Domain enum değerine göre handler'da map edilecek.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Quiz soruları.
    /// </summary>
    public IReadOnlyCollection<QuizQuestionResponse> Questions { get; init; }
        = Array.Empty<QuizQuestionResponse>();
}