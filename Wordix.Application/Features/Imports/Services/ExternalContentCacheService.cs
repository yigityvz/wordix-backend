using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// ExternalContentCache kayıtlarını yöneten Application service'tir.
/// 
/// Bu service ne yapar?
/// - Cache key ile kullanılabilir cache kaydını getirir.
/// - Cache hit olduğunda HitCount ve LastAccessedAt günceller.
/// - Provider sonucu cache'e yazar.
/// - Aynı CacheKey varsa duplicate insert yerine payload refresh yapar.
/// - Cache kaydını expired veya disabled yapabilir.
/// 
/// Bu service ne yapmaz?
/// - Azure'a istek atmaz.
/// - Provider response parse etmez.
/// - Cache payload JSON'unu domain DTO'ya dönüştürmez.
/// - HTTP/controller detayı bilmez.
/// 
/// Bu service sadece external content cache yönetimi sorumluluğuna sahiptir.
/// </summary>
public sealed class ExternalContentCacheService : IExternalContentCacheService
{
    private readonly IRepository<ExternalContentCache> _externalContentCacheRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExternalContentCacheService(
        IRepository<ExternalContentCache> externalContentCacheRepository,
        IUnitOfWork unitOfWork)
    {
        _externalContentCacheRepository = externalContentCacheRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// CacheKey ile kullanılabilir cache kaydını getirir.
    /// 
    /// Not:
    /// ExternalContentCache.IsUsable property’si calculated property olduğu için EF query içinde kullanılmaz.
    /// Bunun yerine Status/ExpiresAt alanlarını SQL'e çevrilebilir şekilde filtreliyoruz.
    /// </summary>
    public async Task<ExternalContentCache?> GetUsableByKeyAsync(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return null;
        }

        var normalizedCacheKey = NormalizeCacheKey(cacheKey);
        var nowUtc = DateTime.UtcNow;

        var cache = await _externalContentCacheRepository.FirstOrDefaultAsync(
            item =>
                item.CacheKey == normalizedCacheKey &&
                item.Status == ExternalContentCacheStatus.Active &&
                (!item.ExpiresAt.HasValue || item.ExpiresAt > nowUtc),
            cancellationToken);

        if (cache is null)
        {
            return null;
        }

        // Cache hit oldu.
        // HitCount/LastAccessedAt güncelleyerek provider maliyetinden ne kadar kaçındığımızı ölçebiliriz.
        cache.MarkAsAccessed(nowUtc);

        _externalContentCacheRepository.Update(cache);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cache;
    }

    /// <summary>
    /// Cache kaydı oluşturur veya mevcut kaydı günceller.
    /// </summary>
    public async Task<ExternalContentCache> SetAsync(
        ExternalContentCacheSetRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCacheKey = NormalizeCacheKey(request.CacheKey);

        var existingCache = await _externalContentCacheRepository.FirstOrDefaultAsync(
            cache => cache.CacheKey == normalizedCacheKey,
            cancellationToken);

        if (existingCache is not null)
        {
            // Aynı CacheKey zaten varsa yeni satır açmıyoruz.
            // Payload'ı güncelliyor ve cache'i tekrar Active hale getiriyoruz.
            existingCache.RefreshPayload(
                cachedPayloadJson: request.CachedPayloadJson,
                contentSource: request.ContentSource,
                qualityStatus: request.QualityStatus,
                expiresAtUtc: request.ExpiresAtUtc);

            _externalContentCacheRepository.Update(existingCache);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return existingCache;
        }

        var cache = new ExternalContentCache(
            providerType: request.ProviderType,
            providerName: request.ProviderName,
            operationName: request.OperationName,
            cacheKey: normalizedCacheKey,
            cachedPayloadJson: request.CachedPayloadJson,
            normalizedInput: request.NormalizedInput,
            sourceLanguageCode: request.SourceLanguageCode,
            targetLanguageCode: request.TargetLanguageCode,
            contentSource: request.ContentSource,
            qualityStatus: request.QualityStatus,
            expiresAtUtc: request.ExpiresAtUtc);

        await _externalContentCacheRepository.AddAsync(
            cache,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cache;
    }

    /// <summary>
    /// Cache kaydını expired yapar.
    /// </summary>
    public async Task MarkAsExpiredAsync(
        Guid externalContentCacheId,
        CancellationToken cancellationToken = default)
    {
        if (externalContentCacheId == Guid.Empty)
        {
            throw new ArgumentException(
                "ExternalContentCacheId boş Guid olamaz.",
                nameof(externalContentCacheId));
        }

        var cache = await _externalContentCacheRepository.GetByIdAsync(
            externalContentCacheId,
            cancellationToken);

        if (cache is null)
        {
            throw new InvalidOperationException(
                $"ExternalContentCache bulunamadı. Id: {externalContentCacheId}");
        }

        cache.MarkAsExpired();

        _externalContentCacheRepository.Update(cache);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Cache kaydını disabled yapar.
    /// </summary>
    public async Task DisableAsync(
        Guid externalContentCacheId,
        CancellationToken cancellationToken = default)
    {
        if (externalContentCacheId == Guid.Empty)
        {
            throw new ArgumentException(
                "ExternalContentCacheId boş Guid olamaz.",
                nameof(externalContentCacheId));
        }

        var cache = await _externalContentCacheRepository.GetByIdAsync(
            externalContentCacheId,
            cancellationToken);

        if (cache is null)
        {
            throw new InvalidOperationException(
                $"ExternalContentCache bulunamadı. Id: {externalContentCacheId}");
        }

        cache.Disable();

        _externalContentCacheRepository.Update(cache);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// CacheKey değerini normalize eder.
    /// 
    /// CacheKey teknik bir lookup anahtarı olduğu için küçük harfe çeviriyoruz.
    /// Böylece:
    /// azure:translate:en:tr:Sleep
    /// azure:translate:en:tr:sleep
    /// aynı cache kaydına gider.
    /// </summary>
    private static string NormalizeCacheKey(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("CacheKey boş olamaz.", nameof(cacheKey));
        }

        return cacheKey.Trim().ToLowerInvariant();
    }
}