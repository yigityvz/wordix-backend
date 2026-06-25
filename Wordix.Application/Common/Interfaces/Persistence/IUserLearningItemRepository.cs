using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Kullanıcının dictionary kayıtları için özel repository sözleşmesidir.
/// 
/// UserLearningItem, kullanıcının kaydettiği LearningItem'ı temsil eder.
/// Bu yapı sadece Word için değil, ileride Phrase ve Sentence için de çalışacaktır.
/// </summary>
public interface IUserLearningItemRepository
{
    /// <summary>
    /// Kullanıcının belirli bir LearningItem'ı dictionary'sine kaydedip kaydetmediğini kontrol eder.
    /// 
    /// Örnek:
    /// Yiğit "achieve" kelimesini daha önce kaydetmiş mi?
    /// 
    /// Bu kontrol Faz 14 Save Dictionary akışında kullanılacak.
    /// </summary>
    Task<bool> ExistsByUserAndLearningItemAsync(
        Guid userProfileId,
        Guid learningItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının belirli bir LearningItem için dictionary kaydını getirir.
    /// 
    /// Kayıt yoksa null döner.
    /// </summary>
    Task<UserLearningItem?> GetByUserAndLearningItemAsync(
        Guid userProfileId,
        Guid learningItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının aktif dictionary kayıtlarını getirir.
    /// 
    /// Dictionary ekranı bu kayıtlar üzerinden beslenecektir.
    /// İlk prototipte sade liste döneceğiz.
    /// İleride sayfalama ve filtreleme eklenebilir.
    /// </summary>
    Task<IReadOnlyList<UserLearningItem>> GetActiveItemsByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default);
}