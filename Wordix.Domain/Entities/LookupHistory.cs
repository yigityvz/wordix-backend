using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının yaptığı lookup/search işlemlerini kayıt altına alır.
/// 
/// LookupHistory sayesinde sistem:
/// - Kullanıcının ne aradığını,
/// - Sonucun database'de bulunup bulunmadığını,
/// - Provider kullanılıp kullanılmadığını,
/// - Kaç sonuç döndüğünü
/// takip edebilir.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık kendi UserProfileId/UserId değerini üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Bu entity, kullanıcıyı token içindeki "sub" claiminden gelen KeycloakUserId ile ilişkilendirir.
/// </summary>
public class LookupHistory : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core entity'leri database'den okurken parametreli constructor yerine
    /// bu constructor'ı kullanabilir.
    /// Dışarıdan bilinçsiz boş nesne oluşturulmasını engellemek için protected bırakıyoruz.
    /// </summary>
    protected LookupHistory()
    {
    }

    /// <summary>
    /// Yeni lookup geçmiş kaydı oluşturur.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelir.
    /// - Kullanıcının Wordix içindeki lookup geçmişini sahiplenmek için kullanılır.
    /// - Backend tarafından üretilen bir UserProfileId değildir.
    /// </summary>
    public LookupHistory(
        string keycloakUserId,
        string queryText,
        string normalizedQueryText,
        InputType inputType,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        Guid? learningItemId,
        bool wasFoundInDatabase,
        bool wasCreatedFromProvider,
        ProviderType? providerType,
        string? providerName,
        int resultCount)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("KeycloakUserId boş olamaz.", nameof(keycloakUserId));
        }

        if (string.IsNullOrWhiteSpace(queryText))
        {
            throw new ArgumentException("QueryText boş olamaz.", nameof(queryText));
        }

        if (string.IsNullOrWhiteSpace(normalizedQueryText))
        {
            throw new ArgumentException("NormalizedQueryText boş olamaz.", nameof(normalizedQueryText));
        }

        if (sourceLanguageId == Guid.Empty)
        {
            throw new ArgumentException("SourceLanguageId boş Guid olamaz.", nameof(sourceLanguageId));
        }

        if (targetLanguageId == Guid.Empty)
        {
            throw new ArgumentException("TargetLanguageId boş Guid olamaz.", nameof(targetLanguageId));
        }

        if (resultCount < 0)
        {
            throw new ArgumentException("ResultCount negatif olamaz.", nameof(resultCount));
        }

        KeycloakUserId = keycloakUserId.Trim();
        QueryText = queryText.Trim();
        NormalizedQueryText = normalizedQueryText.Trim().ToLowerInvariant();
        InputType = inputType;
        SourceLanguageId = sourceLanguageId;
        TargetLanguageId = targetLanguageId;
        LearningItemId = learningItemId;
        WasFoundInDatabase = wasFoundInDatabase;
        WasCreatedFromProvider = wasCreatedFromProvider;
        ProviderType = providerType;
        ProviderName = string.IsNullOrWhiteSpace(providerName) ? null : providerName.Trim();
        ResultCount = resultCount;
    }

    /// <summary>
    /// Lookup işlemini yapan Keycloak kullanıcısının id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden gelir.
    /// Wordix backend ayrıca UserProfileId/UserId üretmediği için
    /// kullanıcıya ait lookup kayıtları bu alan üzerinden filtrelenir.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanıcının yazdığı orijinal arama metnidir.
    /// Örnek: " Achieve "
    /// </summary>
    public string QueryText { get; private set; } = string.Empty;

    /// <summary>
    /// Arama için normalize edilmiş metindir.
    /// Örnek: "achieve"
    /// </summary>
    public string NormalizedQueryText { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanıcının girdiği metnin tipidir.
    /// İlk prototipte genelde Word olacaktır.
    /// </summary>
    public InputType InputType { get; private set; }

    /// <summary>
    /// Kaynak dil Id'sidir.
    /// Örneğin kullanıcı İngilizce kelime arıyorsa English language Id.
    /// </summary>
    public Guid SourceLanguageId { get; private set; }

    /// <summary>
    /// Hedef dil Id'sidir.
    /// Örneğin kullanıcı Türkçe anlam istiyorsa Turkish language Id.
    /// </summary>
    public Guid TargetLanguageId { get; private set; }

    /// <summary>
    /// Arama sonucunda eşleşen LearningItem Id'sidir.
    /// 
    /// Nullable olmasının sebebi:
    /// Bazı aramalar sonucunda sistem hiçbir içerik bulamayabilir.
    /// </summary>
    public Guid? LearningItemId { get; private set; }

    /// <summary>
    /// Sonuç local database'de bulundu mu?
    /// </summary>
    public bool WasFoundInDatabase { get; private set; }

    /// <summary>
    /// Sonuç provider'dan gelip global havuza eklendi mi?
    /// </summary>
    public bool WasCreatedFromProvider { get; private set; }

    /// <summary>
    /// Kullanılan provider türüdür.
    /// Örneğin Dictionary veya Translation provider.
    /// 
    /// Provider kullanılmadıysa null kalır.
    /// </summary>
    public ProviderType? ProviderType { get; private set; }

    /// <summary>
    /// Kullanılan provider adıdır.
    /// Örnek: Wiktionary, Kaikki, LibreTranslate
    /// 
    /// İlk prototipte boş kalabilir.
    /// </summary>
    public string? ProviderName { get; private set; }

    /// <summary>
    /// Kullanıcıya dönen sonuç sayısıdır.
    /// Örneğin bir kelime için 3 anlam dönmüş olabilir.
    /// </summary>
    public int ResultCount { get; private set; }
}