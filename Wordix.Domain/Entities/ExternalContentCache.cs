using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Dış provider'lardan gelen sonuçların cache kaydını temsil eder.
/// 
/// Bu entity neden gerekli?
/// - Azure Translator gibi provider'lara yapılan çağrılar maliyet/kota doğurabilir.
/// - Aynı input için tekrar tekrar provider'a gitmek istemeyiz.
/// - Başarılı provider sonuçlarını belirli süre cache'leyerek sistemi hızlandırabiliriz.
/// - Provider başarısız olduğunda daha önceki cache sonucu kullanılabilir.
/// 
/// Örnek:
/// CacheKey = "azure:translate:en:tr:microservice"
/// CachedPayloadJson = "{ ... provider result json ... }"
/// 
/// Bu tablo kullanıcı dictionary/progress verisi değildir.
/// Teknik provider cache tablosudur.
/// </summary>
public class ExternalContentCache : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected ExternalContentCache()
    {
    }

    /// <summary>
    /// Yeni external content cache kaydı oluşturur.
    /// 
    /// cacheKey neden zorunlu?
    /// - Cache lookup bu key üzerinden yapılır.
    /// - Aynı provider + operation + input + language kombinasyonu için tekil olmalıdır.
    /// 
    /// cachedPayloadJson neden string?
    /// - Provider sonuçları farklı şekillerde olabilir.
    /// - Azure translation sonucu ile Tatoeba/Kaikki sonucu aynı DTO yapısında değildir.
    /// - Bu yüzden cache payload'ı JSON string olarak saklamak esnek olur.
    /// 
    /// expiresAtUtc neden nullable?
    /// - Bazı cache kayıtları belirli süre sonra geçersiz kabul edilebilir.
    /// - Bazı import kaynakları ise manuel disable edilene kadar kullanılabilir.
    /// </summary>
    public ExternalContentCache(
        ProviderType providerType,
        string providerName,
        string operationName,
        string cacheKey,
        string cachedPayloadJson,
        string? normalizedInput = null,
        string? sourceLanguageCode = null,
        string? targetLanguageCode = null,
        ContentSource contentSource = ContentSource.Unknown,
        ContentQualityStatus qualityStatus = ContentQualityStatus.Unknown,
        DateTime? expiresAtUtc = null)
    {
        if (!Enum.IsDefined(providerType) || providerType == ProviderType.Unknown)
        {
            throw new ArgumentException("Geçerli bir ProviderType değeri verilmelidir.", nameof(providerType));
        }

        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentException("ProviderName boş olamaz.", nameof(providerName));
        }

        if (string.IsNullOrWhiteSpace(operationName))
        {
            throw new ArgumentException("OperationName boş olamaz.", nameof(operationName));
        }

        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            throw new ArgumentException("CacheKey boş olamaz.", nameof(cacheKey));
        }

        if (string.IsNullOrWhiteSpace(cachedPayloadJson))
        {
            throw new ArgumentException("CachedPayloadJson boş olamaz.", nameof(cachedPayloadJson));
        }

        ProviderType = providerType;
        ProviderName = providerName.Trim();
        OperationName = operationName.Trim();

        CacheKey = cacheKey.Trim();

        NormalizedInput = string.IsNullOrWhiteSpace(normalizedInput)
            ? null
            : normalizedInput.Trim().ToLowerInvariant();

        SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
            ? null
            : sourceLanguageCode.Trim().ToLowerInvariant();

        TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
            ? null
            : targetLanguageCode.Trim().ToLowerInvariant();

        CachedPayloadJson = cachedPayloadJson.Trim();

        ContentSource = contentSource;
        QualityStatus = qualityStatus;

        Status = ExternalContentCacheStatus.Active;
        ExpiresAt = expiresAtUtc;
    }

    /// <summary>
    /// Provider türüdür.
    /// 
    /// Örnek:
    /// Translation, Dictionary, ExampleSentence, Import.
    /// </summary>
    public ProviderType ProviderType { get; private set; }

    /// <summary>
    /// Provider adıdır.
    /// 
    /// Örnek:
    /// AzureTranslator, Tatoeba, WiktionaryKaikki.
    /// </summary>
    public string ProviderName { get; private set; } = string.Empty;

    /// <summary>
    /// Provider üzerinde yapılan operasyon adıdır.
    /// 
    /// Örnek:
    /// Translate
    /// ParseTatoebaExampleSentences
    /// EnrichKaikkiMeanings
    /// </summary>
    public string OperationName { get; private set; } = string.Empty;

    /// <summary>
    /// Cache lookup için kullanılan teknik anahtardır.
    /// 
    /// Örnek:
    /// azure:translate:en:tr:sleep
    /// azure:translate:en:tr:take responsibility
    /// 
    /// Bu alan unique index ile korunacaktır.
    /// </summary>
    public string CacheKey { get; private set; } = string.Empty;

    /// <summary>
    /// Cache kaydına konu olan normalize input değeridir.
    /// 
    /// Örnek:
    /// sleep
    /// take responsibility
    /// 
    /// Dosya tabanlı import/cache senaryolarında null olabilir.
    /// </summary>
    public string? NormalizedInput { get; private set; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string? SourceLanguageCode { get; private set; }

    /// <summary>
    /// Hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string? TargetLanguageCode { get; private set; }

    /// <summary>
    /// Cache'e yazılan provider sonucunun JSON payload değeridir.
    /// 
    /// Dikkat:
    /// Burada secret, API key, authorization header gibi bilgiler kesinlikle tutulmamalıdır.
    /// Sadece provider sonucunun işlenebilir response verisi tutulmalıdır.
    /// </summary>
    public string CachedPayloadJson { get; private set; } = string.Empty;

    /// <summary>
    /// Cache içeriğinin gerçek veri kaynağıdır.
    /// 
    /// Örnek:
    /// AzureTranslator, Tatoeba, WiktionaryKaikki.
    /// </summary>
    public ContentSource ContentSource { get; private set; } = ContentSource.Unknown;

    /// <summary>
    /// Cache içeriğinin kalite/güven durumudur.
    /// 
    /// Örnek:
    /// Azure çevirisi AutoGenerated,
    /// Tatoeba cümlesi Imported olabilir.
    /// </summary>
    public ContentQualityStatus QualityStatus { get; private set; } = ContentQualityStatus.Unknown;

    /// <summary>
    /// Cache kaydının güncel durumudur.
    /// 
    /// Örnek:
    /// Active, Expired, Disabled.
    /// </summary>
    public ExternalContentCacheStatus Status { get; private set; }

    /// <summary>
    /// Cache kaydı ne zaman geçersiz kabul edilecek?
    /// 
    /// Null ise süre bazlı expiration uygulanmayabilir.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// Cache kaydı en son ne zaman kullanıldı?
    /// 
    /// Cache hit analizi için kullanılır.
    /// </summary>
    public DateTime? LastAccessedAt { get; private set; }

    /// <summary>
    /// Bu cache kaydı kaç kez kullanıldı?
    /// 
    /// Provider maliyet analizi için faydalıdır.
    /// </summary>
    public int HitCount { get; private set; }

    /// <summary>
    /// Cache kaydı aktif ve kullanılabilir mi?
    /// 
    /// Bu property database'e ayrı kolon olarak yazılmaz.
    /// Domain tarafında okunabilirlik için hesaplanır.
    /// </summary>
    public bool IsUsable =>
        Status == ExternalContentCacheStatus.Active &&
        (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);

    /// <summary>
    /// Cache kaydı kullanıldığında hit sayısını ve erişim zamanını günceller.
    /// </summary>
    public void MarkAsAccessed(DateTime? accessedAtUtc = null)
    {
        LastAccessedAt = accessedAtUtc ?? DateTime.UtcNow;
        HitCount++;

        MarkAsUpdated();
    }

    /// <summary>
    /// Cache payload'ını günceller ve cache'i tekrar aktif hale getirir.
    /// 
    /// Ne zaman kullanılır?
    /// - Provider sonucu değişmişse.
    /// - Expired cache yenilenmişse.
    /// - Daha kaliteli bir provider cevabı ile cache refresh yapılmışsa.
    /// </summary>
    public void RefreshPayload(
        string cachedPayloadJson,
        ContentSource contentSource,
        ContentQualityStatus qualityStatus,
        DateTime? expiresAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(cachedPayloadJson))
        {
            throw new ArgumentException("CachedPayloadJson boş olamaz.", nameof(cachedPayloadJson));
        }

        CachedPayloadJson = cachedPayloadJson.Trim();
        ContentSource = contentSource;
        QualityStatus = qualityStatus;
        ExpiresAt = expiresAtUtc;
        Status = ExternalContentCacheStatus.Active;

        MarkAsUpdated();
    }

    /// <summary>
    /// Cache kaydını expired olarak işaretler.
    /// 
    /// Fiziksel silme yapmıyoruz.
    /// Çünkü cache geçmişi provider analizleri için faydalı olabilir.
    /// </summary>
    public void MarkAsExpired()
    {
        if (Status == ExternalContentCacheStatus.Expired)
        {
            return;
        }

        Status = ExternalContentCacheStatus.Expired;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cache kaydını manuel/sistemsel olarak devre dışı bırakır.
    /// 
    /// Örneğin cache içeriği hatalı veya lisans açısından sorunluysa kullanılabilir.
    /// </summary>
    public void Disable()
    {
        if (Status == ExternalContentCacheStatus.Disabled)
        {
            return;
        }

        Status = ExternalContentCacheStatus.Disabled;
        MarkAsUpdated();
    }

    /// <summary>
    /// Expired veya disabled cache kaydını tekrar aktif hale getirir.
    /// 
    /// Genelde RefreshPayload tercih edilir.
    /// Ama payload değişmeden sadece cache tekrar kullanılabilir yapılacaksa bu method kullanılabilir.
    /// </summary>
    public void Activate(DateTime? expiresAtUtc = null)
    {
        if (Status == ExternalContentCacheStatus.Active &&
            ExpiresAt == expiresAtUtc)
        {
            return;
        }

        Status = ExternalContentCacheStatus.Active;
        ExpiresAt = expiresAtUtc;

        MarkAsUpdated();
    }
}