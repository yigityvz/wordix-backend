using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Kullanıcının dictionary kayıtları için özel repository sözleşmesidir.
/// 
/// UserLearningItem, kullanıcının kaydettiği LearningItem'ı temsil eder.
/// Bu yapı sadece Word için değil, ileride Phrase ve Sentence için de çalışacaktır.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Kullanıcıya ait dictionary kayıtları token içindeki "sub" claiminden gelen KeycloakUserId ile ilişkilendirilir.
/// </summary>
public interface IUserLearningItemRepository
{
    /// <summary>
    /// Kullanıcının belirli bir LearningItem'ı dictionary'sine kaydedip kaydetmediğini kontrol eder.
    /// 
    /// Örnek:
    /// Yiğit "achieve" kelimesini daha önce kaydetmiş mi?
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelen kullanıcı id değeridir.
    /// - Eski UserProfileId yerine kullanılır.
    /// - Backend tarafından üretilmez.
    /// 
    /// Bu kontrol Save Dictionary ve Lookup akışında kullanılır.
    /// </summary>
    Task<bool> ExistsByUserAndLearningItemAsync(
        string keycloakUserId,
        Guid learningItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının belirli bir LearningItem için dictionary kaydını getirir.
    /// 
    /// Kayıt yoksa null döner.
    /// 
    /// Kullanıcı filtresi artık UserProfileId ile değil, KeycloakUserId ile yapılır.
    /// </summary>
    Task<UserLearningItem?> GetByUserAndLearningItemAsync(
        string keycloakUserId,
        Guid learningItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının aktif dictionary kayıtlarını getirir.
    /// 
    /// Dictionary ekranı bu kayıtlar üzerinden beslenecektir.
    /// İlk prototipte sade liste döneceğiz.
    /// İleride sayfalama ve filtreleme eklenebilir.
    /// 
    /// Kullanıcıya ait kayıtlar KeycloakUserId üzerinden filtrelenir.
    /// </summary>
    Task<IReadOnlyList<UserLearningItem>> GetActiveItemsByUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);
}