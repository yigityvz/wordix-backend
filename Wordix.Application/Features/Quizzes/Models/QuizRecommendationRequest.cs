using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz için sistem önerisi item istenirken IQuizRecommendationService'e gönderilen request modelidir.
/// 
/// Bu model entity değildir.
/// Database tablosu değildir.
/// Sadece Application katmanında recommendation use-case verisini taşır.
/// 
/// Bu request neyi temsil eder?
/// - Hangi kullanıcı için öneri üretilecek?
/// - Hangi quiz tipi için öneri üretilecek?
/// - Hangi content mode'a uygun itemlar seçilecek?
/// - Hangi zorluk grubuna öncelik verilecek?
/// - Kullanıcının dictionary'sinde zaten olan itemlar hangileri?
/// - Kaç öneri isteniyor?
/// 
/// Neden ayrı model?
/// - Service methoduna çok fazla parametre geçmek istemiyoruz.
/// - İleride recommendation algoritması büyüdüğünde modeli genişletmek kolay olur.
/// - StartQuizCommandHandler sade kalır.
/// </summary>
public sealed class QuizRecommendationRequest
{
    /// <summary>
    /// Öneri üretilecek current user'ın Keycloak user id değeridir.
    /// 
    /// Bu değer JWT token içindeki sub claiminden gelir.
    /// UserProfileId değildir.
    /// </summary>
    public string KeycloakUserId { get; init; } = string.Empty;

    /// <summary>
    /// Başlatılan quiz tipidir.
    /// 
    /// Örnek:
    /// Test
    /// Writing
    /// 
    /// Service bu bilgiyle hangi itemların soru üretmeye uygun olduğunu değerlendirebilir.
    /// </summary>
    public QuizType QuizType { get; init; }

    /// <summary>
    /// Quiz içerik modudur.
    /// 
    /// Örnek:
    /// WordsOnly
    /// PhrasesOnly
    /// SentencesOnly
    /// Mixed
    /// </summary>
    public QuizContentMode QuizContentMode { get; init; }

    /// <summary>
    /// Önerilerde öncelik verilecek zorluk grubudur.
    /// 
    /// Faz 23 ilk algoritmasında LearningItem.DifficultyGroup ile eşleşen itemlara öncelik verilecek.
    /// </summary>
    public DifficultyGroup PreferredDifficultyGroup { get; init; } = DifficultyGroup.Beginner;

    /// <summary>
    /// Service'in en fazla kaç sistem önerisi üretmeye çalışacağını belirtir.
    /// 
    /// Örnek:
    /// Kullanıcı 10 soru istiyorsa ve recommendation oranı %30 ise burada 3 gelebilir.
    /// </summary>
    public int RequestedRecommendationCount { get; init; }

    /// <summary>
    /// Öneri olarak seçilmemesi gereken LearningItem id listesidir.
    /// 
    /// Faz 23'te burada current user'ın dictionary/deck itemlarının LearningItemId değerleri bulunur.
    /// Böylece kullanıcıya zaten kayıtlı olan item tekrar sistem önerisi olarak gelmez.
    /// </summary>
    public IReadOnlyCollection<Guid> ExcludedLearningItemIds { get; init; }
        = Array.Empty<Guid>();
}