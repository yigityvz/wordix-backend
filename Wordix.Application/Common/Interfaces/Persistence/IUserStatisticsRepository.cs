using Wordix.Application.Features.UserStatistics.Models;
using Wordix.Shared.Responses;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Current user'ın öğrenme statistics/dashboard verilerini getirmek için kullanılan repository abstraction'ıdır.
/// 
/// Bu interface neden var?
/// - User statistics sorguları basit CRUD değildir.
/// - UserLearningItem, UserLearningProgress, UserLearningFlag, Deck, QuizSession,
///   QuizQuestion ve QuizAnswer gibi birden fazla tablodan aggregate veri üretir.
/// - Bu sorgular handler içinde yazılırsa Application katmanı DbContext bilmek zorunda kalır.
/// - Generic repository bu tarz raporlama sorguları için uygun değildir.
/// 
/// Mimari karar:
/// - Application katmanı sadece bu interface'i bilir.
/// - EF Core / SQL sorgu detayları Persistence katmanındaki UserStatisticsRepository içinde yazılır.
/// - Tüm methodlar KeycloakUserId parametresi alır.
/// - Böylece hiçbir endpoint request body/query üzerinden user id almaz.
/// - Current user token üzerinden gelen KeycloakUserId ile ownership korunur.
/// </summary>
public interface IUserStatisticsRepository
{
    /// <summary>
    /// Current user'ın genel öğrenme özetini döndürür.
    /// 
    /// Bu endpoint dashboard üst kartları için kullanılabilir.
    /// 
    /// Hesaplanacak örnek metrikler:
    /// - Toplam kaydedilen item sayısı
    /// - Aktif dictionary item sayısı
    /// - Word/Phrase/Sentence dağılımı
    /// - LearningStatus dağılımı
    /// - Review zamanı gelen item sayısı
    /// - Ortalama confidence score
    /// - Favorite/Difficult/WantMorePractice/Ignored flag sayıları
    /// - Toplam doğru/yanlış quiz cevapları
    /// - Genel accuracy oranı
    /// 
    /// keycloakUserId:
    /// - Token içindeki sub claiminden gelir.
    /// - Handler ICurrentUserService üzerinden alır.
    /// - Client bu değeri göndermez.
    /// </summary>
    Task<UserLearningSummaryModel> GetLearningSummaryAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Current user'ın quiz performans istatistiklerini döndürür.
    /// 
    /// Kullanılan kaynaklar:
    /// - QuizSessions
    /// - QuizQuestions
    /// - QuizAnswers
    /// 
    /// Filtreler:
    /// - Date range
    /// - QuizType
    /// - QuizSourceType
    /// - QuizContentMode
    /// - DifficultyGroup
    /// 
    /// Bu method sadece current user'ın quiz verilerini analiz eder.
    /// Başka kullanıcıların verisi sorguya dahil edilmez.
    /// </summary>
    Task<QuizStatisticsModel> GetQuizStatisticsAsync(
        string keycloakUserId,
        QuizStatisticsFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Current user'ın zorlandığı learning itemları sayfalı şekilde döndürür.
    /// 
    /// Kullanılan kaynaklar:
    /// - UserLearningItems
    /// - UserLearningProgresses
    /// - UserLearningFlags
    /// - LearningItems
    /// - Words / Phrases / Sentences
    /// - Meanings
    /// 
    /// Difficult item nasıl belirlenir?
    /// - Kullanıcı manuel Difficult flag eklemiş olabilir.
    /// - Progress verisi düşük confidence, yanlış cevap veya due review sinyali verebilir.
    /// 
    /// Bu method read-only çalışır.
    /// Yeni Difficult flag oluşturmaz.
    /// Progress güncellemez.
    /// </summary>
    Task<PagedResult<DifficultLearningItemModel>> GetDifficultItemsAsync(
        string keycloakUserId,
        DifficultItemsFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Current user'ın deck bazlı öğrenme ve quiz istatistiklerini döndürür.
    /// 
    /// Kullanılan kaynaklar:
    /// - Decks
    /// - DeckItems
    /// - UserLearningItems
    /// - UserLearningProgresses
    /// - UserLearningFlags
    /// - QuizSessions
    /// - QuizQuestions
    /// - QuizAnswers
    /// 
    /// Deck ownership:
    /// - Deck.KeycloakUserId current user ile eşleşmelidir.
    /// - Başka kullanıcının deck'i istatistiklere dahil edilmez.
    /// </summary>
    Task<IReadOnlyCollection<DeckStatisticsModel>> GetDeckStatisticsAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Current user'ın dictionary itemları üzerindeki confidence score dağılımını döndürür.
    /// 
    /// Bucket örnekleri:
    /// - 0-20 VeryLow
    /// - 21-40 Low
    /// - 41-60 Medium
    /// - 61-80 High
    /// - 81-100 VeryHigh
    /// 
    /// Bu veri frontend tarafında grafik/chart çizmek için uygundur.
    /// </summary>
    Task<ConfidenceScoreDistributionModel> GetConfidenceScoreDistributionAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);
}