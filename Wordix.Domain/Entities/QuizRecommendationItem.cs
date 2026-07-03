using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sistem önerisiyle quiz'e eklenen öğrenme item'ını temsil eder.
/// 
/// Bu entity neden var?
/// - QuizQuestion.IsSystemRecommended sadece sorunun sistem önerisi olduğunu söyler.
/// - Ama önerinin neden geldiğini, doğru bilinip bilinmediğini,
///   sonradan dictionary'ye eklenip eklenmediğini ayrıca izlemek isteriz.
/// 
/// Bu entity, öneri davranışını quiz sonrasında analiz edebilmek için kullanılır.
/// 
/// Örnek akış:
/// 1. Sistem "struggle" kelimesini önerir.
/// 2. Bu öneriden bir QuizQuestion oluşur.
/// 3. Kullanıcı yanlış cevap verir.
/// 4. QuizRecommendationItem.WasAnsweredCorrectly = false olur.
/// 5. Kullanıcı isterse bu öneriyi dictionary'sine ekler.
/// 6. QuizRecommendationItem.WasAddedToDictionary = true olur.
/// </summary>
public class QuizRecommendationItem : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected QuizRecommendationItem()
    {
    }

    /// <summary>
    /// Yeni quiz recommendation kaydı oluşturur.
    /// </summary>
    public QuizRecommendationItem(
        Guid quizSessionId,
        Guid quizQuestionId,
        Guid learningItemId,
        RecommendationReason recommendationReason,
        DifficultyGroup difficultyGroup)
    {
        if (quizSessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "QuizSessionId boş Guid olamaz.",
                nameof(quizSessionId));
        }

        if (quizQuestionId == Guid.Empty)
        {
            throw new ArgumentException(
                "QuizQuestionId boş Guid olamaz.",
                nameof(quizQuestionId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "LearningItemId boş Guid olamaz.",
                nameof(learningItemId));
        }

        if (!Enum.IsDefined(recommendationReason))
        {
            throw new ArgumentException(
                "Geçersiz RecommendationReason değeri.",
                nameof(recommendationReason));
        }

        if (!Enum.IsDefined(difficultyGroup))
        {
            throw new ArgumentException(
                "Geçersiz DifficultyGroup değeri.",
                nameof(difficultyGroup));
        }

        QuizSessionId = quizSessionId;
        QuizQuestionId = quizQuestionId;
        LearningItemId = learningItemId;
        RecommendationReason = recommendationReason;
        DifficultyGroup = difficultyGroup;
    }

    /// <summary>
    /// Önerinin eklendiği quiz session id değeridir.
    /// </summary>
    public Guid QuizSessionId { get; private set; }

    /// <summary>
    /// Bu öneriden oluşturulan quiz question id değeridir.
    /// 
    /// Faz 23'te recommendation item, question oluşturulduktan sonra kaydedileceği için dolu tutulur.
    /// </summary>
    public Guid QuizQuestionId { get; private set; }

    /// <summary>
    /// Sistem tarafından önerilen global LearningItem id değeridir.
    /// 
    /// Bu item başlangıçta kullanıcının dictionary'sinde olmayabilir.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Bu item neden önerildi?
    /// 
    /// Örnek:
    /// DifficultyLevelMatch
    /// StarterRecommendation
    /// SimilarToDifficultItems
    /// </summary>
    public RecommendationReason RecommendationReason { get; private set; }

    /// <summary>
    /// Önerilen item'ın zorluk grubudur.
    /// 
    /// Recommendation anındaki snapshot gibi düşünülebilir.
    /// LearningItem daha sonra güncellense bile öneri anındaki grup burada korunur.
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; private set; }

    /// <summary>
    /// Kullanıcı bu öneri sorusunu doğru bildi mi?
    /// 
    /// Nullable olmasının sebebi:
    /// Kullanıcı henüz bu soruya cevap vermemiş olabilir.
    /// </summary>
    public bool? WasAnsweredCorrectly { get; private set; }

    /// <summary>
    /// Kullanıcı bu öneriyi sonradan dictionary'sine ekledi mi?
    /// </summary>
    public bool WasAddedToDictionary { get; private set; }

    /// <summary>
    /// Öneri sorusunun cevap sonucunu kaydeder.
    /// 
    /// Bu method submit answer akışında çağrılır.
    /// </summary>
    public void RegisterAnswerResult(bool wasAnsweredCorrectly)
    {
        WasAnsweredCorrectly = wasAnsweredCorrectly;
        MarkAsUpdated();
    }

    /// <summary>
    /// Önerilen item'ın kullanıcı dictionary'sine eklendiğini işaretler.
    /// 
    /// Bu method save-to-dictionary endpointinde çağrılır.
    /// </summary>
    public void MarkAsAddedToDictionary()
    {
        if (WasAddedToDictionary)
        {
            return;
        }

        WasAddedToDictionary = true;
        MarkAsUpdated();
    }
}