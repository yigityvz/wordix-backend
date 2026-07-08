using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Dış provider sonuçlarını cache'den okuyan ve cache'e yazan service sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Azure gibi dış provider çağrıları maliyet/kota doğurur.
/// - Aynı input için tekrar tekrar provider'a gitmek istemeyiz.
/// - Cache okuma/yazma davranışını provider veya handler içine dağıtmak istemiyoruz.
/// - İleride cache policy değişirse tek yerden yönetmek isteriz.
/// </summary>
public interface IExternalContentCacheService
{
    /// <summary>
    /// CacheKey ile aktif ve kullanılabilir cache kaydını getirir.
    /// 
    /// Cache bulunursa:
    /// - HitCount artırılır.
    /// - LastAccessedAt güncellenir.
    /// 
    /// Cache yoksa veya expired/disabled ise null döner.
    /// </summary>
    Task<ExternalContentCache?> GetUsableByKeyAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache kaydı oluşturur veya aynı CacheKey zaten varsa payload'ı refresh eder.
    /// 
    /// Bu method idempotent çalışır:
    /// - CacheKey yoksa yeni kayıt oluşturur.
    /// - CacheKey varsa mevcut kaydı günceller.
    /// </summary>
    Task<ExternalContentCache> SetAsync(
        ExternalContentCacheSetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache kaydını expired olarak işaretler.
    /// </summary>
    Task MarkAsExpiredAsync(
        Guid externalContentCacheId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cache kaydını disabled olarak işaretler.
    /// </summary>
    Task DisableAsync(
        Guid externalContentCacheId,
        CancellationToken cancellationToken = default);
}