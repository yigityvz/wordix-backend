namespace Wordix.Application.Features.Quizzes.Dtos.Responses;

/// <summary>
/// Sistem önerisi olarak gelen bir quiz item'ı dictionary'ye eklendiğinde dönen response modelidir.
/// 
/// Bu response neden Quizzes feature altında?
/// - Endpoint quiz recommendation üzerinden çalışır.
/// - Route parametresi QuizRecommendationItemId'dir.
/// - Ama sonuç olarak kullanıcı dictionary'sinde UserLearningItem oluşur veya aktif hale gelir.
/// 
/// Bu yüzden response hem recommendation bilgisini hem dictionary kayıt bilgisini taşır.
/// </summary>
public sealed class SaveRecommendedItemToDictionaryResponse
{
    /// <summary>
    /// Dictionary'ye ekleme işleminin başlatıldığı recommendation kayıt id değeridir.
    /// </summary>
    public Guid QuizRecommendationItemId { get; init; }

    /// <summary>
    /// Dictionary'ye eklenen global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının dictionary kaydı id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının bu dictionary item için progress kaydı id değeridir.
    /// </summary>
    public Guid UserLearningProgressId { get; init; }

    /// <summary>
    /// Eğer Word/Phrase için seçili meaning varsa id değeridir.
    /// Sentence itemlarda null olabilir.
    /// </summary>
    public Guid? SelectedMeaningId { get; init; }

    /// <summary>
    /// Önerinin hangi sebeple geldiğini gösterir.
    /// 
    /// Örnek:
    /// DifficultyLevelMatch
    /// StarterRecommendation
    /// </summary>
    public string RecommendationReason { get; init; } = string.Empty;

    /// <summary>
    /// Bu item zaten kullanıcının aktif dictionary'sinde var mıydı?
    /// 
    /// true ise yeni UserLearningItem oluşturulmamıştır.
    /// </summary>
    public bool WasAlreadySaved { get; init; }

    /// <summary>
    /// Bu item daha önce pasif hale getirilmişti ve tekrar aktif mi edildi?
    /// </summary>
    public bool WasReactivated { get; init; }

    /// <summary>
    /// Recommendation kaydı dictionary'ye eklendi olarak işaretlendi mi?
    /// </summary>
    public bool WasAddedToDictionary { get; init; }

    /// <summary>
    /// UserLearningItem'ın kaydedildiği zamandır.
    /// </summary>
    public DateTime SavedAt { get; init; }

    /// <summary>
    /// Başlangıç learning status değeridir.
    /// </summary>
    public string LearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// Başlangıç confidence score değeridir.
    /// </summary>
    public int LearningConfidenceScore { get; init; }

    /// <summary>
    /// Dictionary kaydı aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}