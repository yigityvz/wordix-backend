using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// UserLearningItem için özel repository implementasyonudur.
/// 
/// Kullanıcının dictionary kayıtlarıyla ilgili özel sorgular burada tutulur.
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
    /// hem de database tarafında UserProfileId + LearningItemId unique index ile korunacak.
    /// </summary>
    public async Task<bool> ExistsByUserAndLearningItemAsync(
        Guid userProfileId,
        Guid learningItemId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty || learningItemId == Guid.Empty)
        {
            return false;
        }

        return await _dbContext.UserLearningItems
            .AsNoTracking()
            .AnyAsync(item =>
                item.UserProfileId == userProfileId &&
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
    /// </summary>
    public async Task<UserLearningItem?> GetByUserAndLearningItemAsync(
        Guid userProfileId,
        Guid learningItemId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty || learningItemId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.UserLearningItems
            .FirstOrDefaultAsync(item =>
                item.UserProfileId == userProfileId &&
                item.LearningItemId == learningItemId,
                cancellationToken);
    }

    /// <summary>
    /// Kullanıcının aktif dictionary kayıtlarını getirir.
    /// 
    /// AsNoTracking kullanıyoruz çünkü bu method listeleme/read-only amaçlıdır.
    /// Dictionary ekranı için kullanılabilir.
    /// </summary>
    public async Task<IReadOnlyList<UserLearningItem>> GetActiveItemsByUserAsync(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return Array.Empty<UserLearningItem>();
        }

        return await _dbContext.UserLearningItems
            .AsNoTracking()
            .Where(item =>
                item.UserProfileId == userProfileId &&
                item.IsActive)
            .OrderByDescending(item => item.SavedAt)
            .ToListAsync(cancellationToken);
    }
}