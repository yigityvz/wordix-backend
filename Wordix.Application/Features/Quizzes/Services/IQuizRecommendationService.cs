using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz içine dahil edilebilecek sistem önerisi itemları üretir.
/// 
/// Bu interface neden var?
/// - StartQuizCommandHandler içine recommendation algoritması gömmek istemiyoruz.
/// - Recommendation mantığı ileride büyüyecek:
///   - difficulty bazlı öneri
///   - tag/category bazlı öneri
///   - difficult item benzerliği
///   - import/provider kaynaklı yeni içerikler
///   - analytics destekli öneri
/// - Handler sadece bu servisi çağırmalı, algoritmanın detayını bilmemeli.
/// 
/// Faz 23 ilk implementation:
/// - Local database'deki aktif LearningItem kayıtlarından seçim yapacak.
/// - Kullanıcının dictionary'sinde zaten olan LearningItemları önermeyecek.
/// - QuizContentMode ve QuizType'a uygun itemları seçecek.
/// - PreferredDifficultyGroup ile eşleşen itemlara öncelik verecek.
/// 
/// Bu service ne yapmaz?
/// - QuizSession oluşturmaz.
/// - QuizQuestion oluşturmaz.
/// - QuizOption oluşturmaz.
/// - UserLearningItem oluşturmaz.
/// - Provider/API çağrısı yapmaz.
/// 
/// Bu service sadece öneri candidate üretir.
/// </summary>
public interface IQuizRecommendationService
{
    /// <summary>
    /// Verilen request'e göre quiz için sistem önerisi candidate listesi üretir.
    /// 
    /// Eğer uygun öneri bulunamazsa null değil, boş QuizRecommendationResult döner.
    /// </summary>
    Task<QuizRecommendationResult> GetRecommendationsAsync(
        QuizRecommendationRequest request,
        CancellationToken cancellationToken);
}