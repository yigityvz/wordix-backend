namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// IQuizRecommendationService'in ürettiği sistem önerisi sonucunu temsil eder.
/// 
/// Bu model entity değildir.
/// Sadece recommendation service ile StartQuizCommandHandler arasında veri taşır.
/// 
/// Result içinde doğrudan QuizQuestionCandidate dönüyoruz.
/// Çünkü generator zaten QuizQuestionCandidate üzerinden soru üretir.
/// Böylece recommendation service, mevcut quiz generator altyapısına ekstra dönüşüm yükü bindirmeden bağlanır.
/// </summary>
public sealed class QuizRecommendationResult
{
    /// <summary>
    /// Service'ten istenen öneri sayısıdır.
    /// 
    /// Bu sayı ile gerçekten üretilen öneri sayısı aynı olmak zorunda değildir.
    /// Çünkü database'de uygun yeterli item olmayabilir.
    /// </summary>
    public int RequestedRecommendationCount { get; init; }

    /// <summary>
    /// Üretilen sistem önerisi candidate listesidir.
    /// 
    /// Bu candidate'larda:
    /// - IsSystemRecommended true olmalıdır.
    /// - RecommendationReason dolu olmalıdır.
    /// - DifficultyGroup mümkünse LearningItem üzerinden set edilmelidir.
    /// </summary>
    public IReadOnlyCollection<QuizQuestionCandidate> Candidates { get; init; }
        = Array.Empty<QuizQuestionCandidate>();

    /// <summary>
    /// Gerçekten kaç öneri üretildiğini döner.
    /// </summary>
    public int GeneratedRecommendationCount => Candidates.Count;

    /// <summary>
    /// En az bir sistem önerisi üretildi mi?
    /// </summary>
    public bool HasRecommendations => Candidates.Count > 0;

    /// <summary>
    /// Boş recommendation result üretmek için yardımcı factory method.
    /// 
    /// Service uygun item bulamazsa null dönmek yerine boş result dönebilir.
    /// Bu, handler tarafında null kontrolünü azaltır.
    /// </summary>
    public static QuizRecommendationResult Empty(int requestedRecommendationCount)
    {
        return new QuizRecommendationResult
        {
            RequestedRecommendationCount = requestedRecommendationCount,
            Candidates = Array.Empty<QuizQuestionCandidate>()
        };
    }
}