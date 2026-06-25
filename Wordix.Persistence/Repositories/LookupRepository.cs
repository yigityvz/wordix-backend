using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// LookupHistory için özel repository implementasyonudur.
/// 
/// Kullanıcıların lookup geçmişi ve ileride admin analytics sorguları için kullanılır.
/// </summary>
public class LookupRepository : ILookupRepository
{
    private const int MaxRecentLookupCount = 50;

    private readonly WordixDbContext _dbContext;

    public LookupRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Kullanıcının son lookup kayıtlarını getirir.
    /// 
    /// count parametresi dışarıdan gelebileceği için güvenli bir üst limit uyguluyoruz.
    /// Böylece yanlışlıkla binlerce kayıt çekilmesini engelleriz.
    /// </summary>
    public async Task<IReadOnlyList<LookupHistory>> GetRecentLookupsByUserAsync(
        Guid userProfileId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return Array.Empty<LookupHistory>();
        }

        if (count <= 0)
        {
            return Array.Empty<LookupHistory>();
        }

        var safeCount = Math.Min(count, MaxRecentLookupCount);

        return await _dbContext.LookupHistories
            .AsNoTracking()
            .Where(lookup => lookup.UserProfileId == userProfileId)
            .OrderByDescending(lookup => lookup.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Kullanıcının belirli bir normalized query için son lookup kaydını getirir.
    /// 
    /// Örneğin kullanıcı "achieve" kelimesini daha önce aramış mı?
    /// Aradıysa en son arama kaydı hangisi?
    /// </summary>
    public async Task<LookupHistory?> GetLastLookupByUserAndQueryAsync(
        Guid userProfileId,
        string normalizedQueryText,
        CancellationToken cancellationToken = default)
    {
        if (userProfileId == Guid.Empty)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(normalizedQueryText))
        {
            return null;
        }

        var normalized = normalizedQueryText.Trim().ToLowerInvariant();

        return await _dbContext.LookupHistories
            .AsNoTracking()
            .Where(lookup =>
                lookup.UserProfileId == userProfileId &&
                lookup.NormalizedQueryText == normalized)
            .OrderByDescending(lookup => lookup.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}