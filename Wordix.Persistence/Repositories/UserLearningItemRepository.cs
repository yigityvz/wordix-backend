using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// UserLearningItem için özel repository implementasyonudur.
/// 
/// Kullanıcının dictionary kayıtlarıyla ilgili özel sorgular burada tutulur.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Dictionary kayıtları Keycloak token içindeki "sub" claiminden gelen KeycloakUserId ile filtrelenir.
/// </summary>
public class UserLearningItemRepository : IUserLearningItemRepository
{
    private readonly WordixDbContext _dbContext;

    public UserLearningItemRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Kullanıcının belirli bir LearningItem'ı daha önce dictionary'ye ekleyip eklemediğini kontrol eder.
    /// 
    /// Bu kontrol hem business rule olarak yapılacak,
    /// hem de database tarafında KeycloakUserId + LearningItemId unique index ile korunacak.
    /// </summary>
    public async Task<bool> ExistsByUserAndLearningItemAsync(
        string keycloakUserId,
        Guid learningItemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId) || learningItemId == Guid.Empty)
        {
            return false;
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();

        return await _dbContext.UserLearningItems
            .AsNoTracking()
            .AnyAsync(item =>
                item.KeycloakUserId == normalizedKeycloakUserId &&
                item.LearningItemId == learningItemId,
                cancellationToken);
    }

    /// <summary>
    /// Kullanıcı + LearningItem eşleşmesine göre dictionary kaydını getirir.
    /// 
    /// Bu method tracking açık çalışır.
    /// Çünkü ileride bu kayıt üzerinde:
    /// - Activate
    /// - Deactivate
    /// - ChangeSelectedMeaning
    /// gibi domain methodları çalıştırılabilir.
    /// 
    /// Kullanıcı filtresi artık UserProfileId ile değil, KeycloakUserId ile yapılır.
    /// </summary>
    public async Task<UserLearningItem?> GetByUserAndLearningItemAsync(
        string keycloakUserId,
        Guid learningItemId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId) || learningItemId == Guid.Empty)
        {
            return null;
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();

        return await _dbContext.UserLearningItems
            .FirstOrDefaultAsync(item =>
                item.KeycloakUserId == normalizedKeycloakUserId &&
                item.LearningItemId == learningItemId,
                cancellationToken);
    }

    /// <summary>
    /// Kullanıcının aktif dictionary kayıtlarını getirir.
    /// 
    /// AsNoTracking kullanıyoruz çünkü bu method listeleme/read-only amaçlıdır.
    /// Dictionary ekranı için kullanılabilir.
    /// 
    /// Kullanıcıya ait kayıtlar KeycloakUserId üzerinden filtrelenir.
    /// </summary>
    public async Task<IReadOnlyList<UserLearningItem>> GetActiveItemsByUserAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Array.Empty<UserLearningItem>();
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();

        return await _dbContext.UserLearningItems
            .AsNoTracking()
            .Where(item =>
                item.KeycloakUserId == normalizedKeycloakUserId &&
                item.IsActive)
            .OrderByDescending(item => item.SavedAt)
            .ToListAsync(cancellationToken);
    }
}