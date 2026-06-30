using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// LookupHistory için özel repository implementasyonudur.
/// 
/// Kullanıcıların lookup geçmişi ve ileride admin analytics sorguları için kullanılır.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Lookup geçmişi Keycloak token içindeki "sub" claiminden gelen KeycloakUserId ile filtrelenir.
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
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelen kullanıcı id değeridir.
    /// - UserProfileId yerine kullanılır.
    /// 
    /// count parametresi dışarıdan gelebileceği için güvenli bir üst limit uyguluyoruz.
    /// Böylece yanlışlıkla binlerce kayıt çekilmesini engelleriz.
    /// </summary>
    public async Task<IReadOnlyList<LookupHistory>> GetRecentLookupsByUserAsync(
        string keycloakUserId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return Array.Empty<LookupHistory>();
        }

        if (count <= 0)
        {
            return Array.Empty<LookupHistory>();
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();
        var safeCount = Math.Min(count, MaxRecentLookupCount);

        return await _dbContext.LookupHistories
            .AsNoTracking()
            .Where(lookup => lookup.KeycloakUserId == normalizedKeycloakUserId)
            .OrderByDescending(lookup => lookup.CreatedAt)
            .Take(safeCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Kullanıcının belirli bir normalized query için son lookup kaydını getirir.
    /// 
    /// Örneğin kullanıcı "achieve" kelimesini daha önce aramış mı?
    /// Aradıysa en son arama kaydı hangisi?
    /// 
    /// Kullanıcı filtresi artık UserProfileId ile değil, KeycloakUserId ile yapılır.
    /// </summary>
    public async Task<LookupHistory?> GetLastLookupByUserAndQueryAsync(
        string keycloakUserId,
        string normalizedQueryText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(normalizedQueryText))
        {
            return null;
        }

        var normalizedKeycloakUserId = keycloakUserId.Trim();
        var normalized = normalizedQueryText.Trim().ToLowerInvariant();

        return await _dbContext.LookupHistories
            .AsNoTracking()
            .Where(lookup =>
                lookup.KeycloakUserId == normalizedKeycloakUserId &&
                lookup.NormalizedQueryText == normalized)
            .OrderByDescending(lookup => lookup.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}