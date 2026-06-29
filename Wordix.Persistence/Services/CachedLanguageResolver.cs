using Microsoft.Extensions.Caching.Memory;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Localization;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Localization;
using Wordix.Domain.Entities;

namespace Wordix.Persistence.Services;

/// <summary>
/// Dil koduna göre aktif language bilgisini cache destekli çözen persistence servisidir.
/// 
/// Bu class ne yapar?
/// - Dil kodunu normalize eder.
/// - Önce IMemoryCache içinde arar.
/// - Cache'te yoksa database'den aktif Language kaydını çeker.
/// - EF entity'sini doğrudan cachelemek yerine LanguageLookupData modeline map eder.
/// - Sonucu cache'e koyar.
/// 
/// Bu class neden Persistence katmanında?
/// - IRepository<Language> kullanır.
/// - IMemoryCache kullanır.
/// - Database/cache gibi altyapı detayları Application katmanında olmamalıdır.
/// </summary>
public sealed class CachedLanguageResolver : ILanguageResolver
{
    /// <summary>
    /// Cache key prefix'i.
    /// 
    /// İleride Redis gibi distributed cache'e geçersek key yapısı zaten hazır olur.
    /// </summary>
    private const string ActiveLanguageCacheKeyPrefix = "wordix:languages:active:";

    /// <summary>
    /// Dil kayıtları sık değişmediği için 24 saat cache'te tutulur.
    /// 
    /// Eğer ileride admin panelden dil aktif/pasif işlemleri yapılırsa,
    /// o işlem sırasında ilgili cache key invalidate edilebilir.
    /// </summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly IMemoryCache _memoryCache;
    private readonly IRepository<Language> _languageRepository;

    /// <summary>
    /// CachedLanguageResolver dependencies.
    /// </summary>
    public CachedLanguageResolver(
        IMemoryCache memoryCache,
        IRepository<Language> languageRepository)
    {
        _memoryCache = memoryCache;
        _languageRepository = languageRepository;
    }

    /// <summary>
    /// Verilen dil koduna göre aktif language bilgisini döner.
    /// 
    /// Akış:
    /// 1. Dil kodu normalize edilir.
    /// 2. Cache key oluşturulur.
    /// 3. Cache'te varsa direkt döner.
    /// 4. Cache'te yoksa database'den aktif Language aranır.
    /// 5. Bulunamazsa NotFoundException fırlatılır.
    /// 6. Bulunursa LanguageLookupData'ya map edilir ve cache'e yazılır.
    /// </summary>
    public async Task<LanguageLookupData> GetRequiredActiveLanguageByCodeAsync(
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguageCode = NormalizeLanguageCode(languageCode);
        var cacheKey = CreateActiveLanguageCacheKey(normalizedLanguageCode);

        if (_memoryCache.TryGetValue(cacheKey, out LanguageLookupData? cachedLanguage)
            && cachedLanguage is not null)
        {
            return cachedLanguage;
        }

        var language = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == normalizedLanguageCode && language.IsActive,
            cancellationToken);

        if (language is null)
        {
            throw new NotFoundException(
                "Language",
                normalizedLanguageCode);
        }

        var languageLookupData = MapToLookupData(language);

        _memoryCache.Set(
            cacheKey,
            languageLookupData,
            CreateCacheOptions());

        return languageLookupData;
    }

    /// <summary>
    /// Dil kodunu normalize eder.
    /// 
    /// Örnek:
    /// " EN " → "en"
    /// "Tr"   → "tr"
    /// </summary>
    private static string NormalizeLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return string.Empty;
        }

        return languageCode.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Aktif dil cache key'ini üretir.
    /// </summary>
    private static string CreateActiveLanguageCacheKey(string normalizedLanguageCode)
    {
        return $"{ActiveLanguageCacheKeyPrefix}{normalizedLanguageCode}";
    }

    /// <summary>
    /// Language entity'sini cache'e koyacağımız sade modele dönüştürür.
    /// 
    /// EF entity'sini doğrudan cachelemiyoruz.
    /// Bunun yerine sadece handlerların ihtiyaç duyduğu bilgileri taşıyan
    /// LanguageLookupData modelini cacheliyoruz.
    /// </summary>
    private static LanguageLookupData MapToLookupData(Language language)
    {
        return new LanguageLookupData
        {
            Id = language.Id,
            Code = language.Code,
            Name = language.Name,
            NativeName = language.NativeName
        };
    }

    /// <summary>
    /// Cache ayarlarını merkezi üretir.
    /// 
    /// AbsoluteExpirationRelativeToNow:
    /// - Dil bilgisi cache'e girdikten sonra en fazla 24 saat tutulur.
    /// 
    /// SlidingExpiration:
    /// - 6 saat boyunca hiç kullanılmazsa cache'ten düşebilir.
    /// 
    /// Böylece hem uzun süreli performans kazanırız hem de kullanılmayan kayıtlar
    /// memory'de gereksiz tutulmaz.
    /// </summary>
    private static MemoryCacheEntryOptions CreateCacheOptions()
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromHours(6)
        };
    }
}