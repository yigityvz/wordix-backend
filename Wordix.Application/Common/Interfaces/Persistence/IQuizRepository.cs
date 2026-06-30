using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Quiz akışları için özel repository sözleşmesidir.
/// 
/// Quiz sistemi birden fazla entity ile çalışır:
/// - QuizSession
/// - QuizQuestion
/// - QuizOption
/// - QuizAnswer
/// 
/// Bu yüzden quiz'e özel sorguları generic repository içinde dağıtmak yerine
/// IQuizRepository altında topluyoruz.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Quiz ownership kontrolleri token içindeki "sub" claiminden gelen KeycloakUserId ile yapılır.
/// </summary>
public interface IQuizRepository
{
    /// <summary>
    /// Belirli bir quiz session'ı kullanıcıya ait mi kontrol ederek getirir.
    /// 
    /// Bu method ownership kontrolü için önemlidir.
    /// Kullanıcı başkasının quiz session'ına erişmemelidir.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelen kullanıcı id değeridir.
    /// - Eski UserProfileId yerine kullanılır.
    /// - Backend tarafından üretilmez.
    /// </summary>
    Task<QuizSession?> GetSessionByIdForUserAsync(
        Guid quizSessionId,
        string keycloakUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir quiz session'a ait soruları sıralı şekilde getirir.
    /// </summary>
    Task<IReadOnlyList<QuizQuestion>> GetQuestionsBySessionAsync(
        Guid quizSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir quiz sorusuna ait seçenekleri sıralı şekilde getirir.
    /// 
    /// Test quiz için kullanılır.
    /// Writing quizlerde option olmayabilir.
    /// </summary>
    Task<IReadOnlyList<QuizOption>> GetOptionsByQuestionAsync(
        Guid quizQuestionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Id'ye göre quiz option getirir.
    /// 
    /// Kullanıcı cevap gönderdiğinde seçilen option gerçekten var mı kontrol etmek için kullanılır.
    /// </summary>
    Task<QuizOption?> GetOptionByIdAsync(
        Guid quizOptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı belirli bir quiz sorusuna daha önce cevap vermiş mi kontrol eder.
    /// 
    /// Aynı soruya birden fazla cevap verilmesini engellemek için kullanılabilir.
    /// 
    /// Yeni mimaride kullanıcı filtresi UserProfileId ile değil, KeycloakUserId ile yapılır.
    /// </summary>
    Task<bool> HasAnswerForQuestionAsync(
        Guid quizQuestionId,
        string keycloakUserId,
        CancellationToken cancellationToken = default);
}