namespace Wordix.Application.Features.UserStatistics.Dtos.Requests;

/// <summary>
/// Kullanıcının quiz istatistiklerini filtrelemek için kullanılan request DTO'sudur.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/quizzes
/// 
/// Bu DTO query string'den gelir.
/// Controller bu değerleri doğrulamaz.
/// Validation, ileride GetQuizStatisticsQueryValidator içinde yapılacaktır.
/// 
/// Örnek:
/// GET /api/user-statistics/quizzes?fromUtc=2026-07-01&toUtc=2026-07-08&quizType=Test
/// </summary>
public sealed class QuizStatisticsRequest
{
    /// <summary>
    /// Analiz başlangıç tarihidir.
    /// Null gelirse handler son 30 gün varsayımını kullanacaktır.
    /// </summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>
    /// Analiz bitiş tarihidir.
    /// Null gelirse handler DateTime.UtcNow kullanacaktır.
    /// </summary>
    public DateTime? ToUtc { get; init; }

    /// <summary>
    /// Quiz tipi filtresidir.
    /// 
    /// Desteklenen örnek değerler:
    /// - Test
    /// - Writing
    /// - Mixed
    /// 
    /// String tutuyoruz çünkü query string'den gelir.
    /// Mapper/validator tarafında enum'a çevrilecektir.
    /// </summary>
    public string? QuizType { get; init; }

    /// <summary>
    /// Quiz kaynak tipi filtresidir.
    /// 
    /// Desteklenen örnek değerler:
    /// - UserDictionary
    /// - Deck
    /// - DifficultItems
    /// - SystemRecommendations
    /// - Mixed
    /// </summary>
    public string? QuizSourceType { get; init; }

    /// <summary>
    /// Quiz içerik modu filtresidir.
    /// 
    /// Desteklenen örnek değerler:
    /// - WordsOnly
    /// - PhrasesOnly
    /// - SentencesOnly
    /// - Mixed
    /// </summary>
    public string? QuizContentMode { get; init; }

    /// <summary>
    /// Zorluk grubu filtresidir.
    /// 
    /// Örnek:
    /// - Beginner
    /// - Intermediate
    /// - Hard
    /// - Mixed
    /// </summary>
    public string? DifficultyGroup { get; init; }
}