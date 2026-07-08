using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Dış provider çağrılarının log kaydını temsil eder.
/// 
/// Bu entity neden gerekli?
/// - Azure Translator gibi dış servis çağrıları maliyet/kota doğurabilir.
/// - Provider çağrılarının başarılı mı, hatalı mı, timeout mu olduğunu izlemek isteriz.
/// - Aynı input için cache kullanıldı mı, gerçek provider çağrısı yapıldı mı takip etmek isteriz.
/// - ImportJob ile ilişkilendirerek hangi import sürecinde hangi provider çağrıları yapıldı görebiliriz.
/// 
/// Örnek provider çağrıları:
/// - Azure Translator runtime lookup çağrısı
/// - Kaikki parser/enrichment dosya okuma işlemi
/// - Tatoeba parser/enrichment işlemi
/// 
/// Not:
/// Bu tablo kullanıcıya ait dictionary/progress verisi değildir.
/// Analytics/operation log tablosudur.
/// </summary>
public class ProviderRequestLog : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected ProviderRequestLog()
    {
    }

    /// <summary>
    /// Yeni provider request log kaydı oluşturur.
    /// 
    /// importJobId neden nullable?
    /// - Bazı provider çağrıları bir import job içinde çalışır.
    /// - Runtime lookup gibi bazı çağrılar doğrudan kullanıcı isteği sırasında oluşabilir.
    /// 
    /// keycloakUserId neden nullable?
    /// - Runtime lookup sırasında kullanıcı varsa dolu olabilir.
    /// - Sistem/background importlarında kullanıcı olmayabilir.
    /// </summary>
    public ProviderRequestLog(
        ProviderType providerType,
        string providerName,
        string operationName,
        ProviderRequestStatus status,
        string requestKey,
        string? normalizedInput = null,
        string? sourceLanguageCode = null,
        string? targetLanguageCode = null,
        Guid? importJobId = null,
        string? keycloakUserId = null,
        Guid? learningItemId = null,
        bool wasServedFromCache = false,
        int? durationMs = null,
        int? httpStatusCode = null,
        string? errorCode = null,
        string? errorMessage = null)
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

        if (!Enum.IsDefined(status) || status == ProviderRequestStatus.Unknown)
        {
            throw new ArgumentException("Geçerli bir ProviderRequestStatus değeri verilmelidir.", nameof(status));
        }

        if (string.IsNullOrWhiteSpace(requestKey))
        {
            throw new ArgumentException("RequestKey boş olamaz.", nameof(requestKey));
        }

        if (importJobId == Guid.Empty)
        {
            throw new ArgumentException("ImportJobId boş Guid olamaz. Null veya geçerli Guid olmalıdır.", nameof(importJobId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz. Null veya geçerli Guid olmalıdır.", nameof(learningItemId));
        }

        if (durationMs is < 0)
        {
            throw new ArgumentException("DurationMs negatif olamaz.", nameof(durationMs));
        }

        ProviderType = providerType;
        ProviderName = providerName.Trim();
        OperationName = operationName.Trim();
        Status = status;

        RequestKey = requestKey.Trim();

        NormalizedInput = string.IsNullOrWhiteSpace(normalizedInput)
            ? null
            : normalizedInput.Trim().ToLowerInvariant();

        SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
            ? null
            : sourceLanguageCode.Trim().ToLowerInvariant();

        TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
            ? null
            : targetLanguageCode.Trim().ToLowerInvariant();

        ImportJobId = importJobId;

        KeycloakUserId = string.IsNullOrWhiteSpace(keycloakUserId)
            ? null
            : keycloakUserId.Trim();

        LearningItemId = learningItemId;

        WasServedFromCache = wasServedFromCache;
        DurationMs = durationMs;
        HttpStatusCode = httpStatusCode;

        ErrorCode = string.IsNullOrWhiteSpace(errorCode)
            ? null
            : errorCode.Trim();

        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
            ? null
            : errorMessage.Trim();
    }

    /// <summary>
    /// Provider çağrısının bağlı olduğu import job id değeridir.
    /// 
    /// Null olabilir.
    /// Çünkü runtime lookup provider çağrıları herhangi bir ImportJob içinde çalışmayabilir.
    /// </summary>
    public Guid? ImportJobId { get; private set; }

    /// <summary>
    /// Provider çağrısını yapan kullanıcıdır.
    /// 
    /// Runtime lookup gibi user-triggered çağrılarda dolu olabilir.
    /// Sistem/background importlarında null kalabilir.
    /// </summary>
    public string? KeycloakUserId { get; private set; }

    /// <summary>
    /// Provider çağrısı sonucunda oluşturulan veya ilişkili olan LearningItem id değeridir.
    /// 
    /// Örnek:
    /// Azure lookup sonrası global Word/Phrase oluşturulduysa o LearningItem id burada tutulabilir.
    /// </summary>
    public Guid? LearningItemId { get; private set; }

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
    /// Provider çağrısının durumudur.
    /// 
    /// Örnek:
    /// Succeeded, Failed, Timeout, RateLimited, ServedFromCache.
    /// </summary>
    public ProviderRequestStatus Status { get; private set; }

    /// <summary>
    /// Çağrıyı teknik olarak benzersizleştiren normalize edilmiş key değeridir.
    /// 
    /// Örnek:
    /// azure:translate:en:tr:sleep
    /// tatoeba:examples:en:tr:links_sample.csv
    /// 
    /// Bu alan cache/log correlation için kullanılır.
    /// </summary>
    public string RequestKey { get; private set; } = string.Empty;

    /// <summary>
    /// Provider çağrısına konu olan normalize input değeridir.
    /// 
    /// Örnek:
    /// sleep
    /// take responsibility
    /// 
    /// Dosya importlarında null kalabilir.
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
    /// Bu sonuç cache'den mi döndü?
    /// 
    /// True ise dış provider'a gerçek çağrı yapılmamış olabilir.
    /// </summary>
    public bool WasServedFromCache { get; private set; }

    /// <summary>
    /// Provider çağrısının kaç milisaniye sürdüğüdür.
    /// 
    /// Dosya parse gibi bazı operationlarda ölçüm varsa dolu olabilir.
    /// </summary>
    public int? DurationMs { get; private set; }

    /// <summary>
    /// HTTP tabanlı provider çağrılarında dönen status code değeridir.
    /// 
    /// Örnek:
    /// 200, 400, 401, 429, 500.
    /// </summary>
    public int? HttpStatusCode { get; private set; }

    /// <summary>
    /// Provider hata kodudur.
    /// 
    /// Örnek:
    /// RATE_LIMITED
    /// TIMEOUT
    /// AZURE_TRANSLATOR_ERROR
    /// </summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// Provider hata mesajıdır.
    /// 
    /// Çok uzun response body veya secret içeren veri burada tutulmamalıdır.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Log kaydını bir ImportJob ile ilişkilendirir.
    /// 
    /// Bazı durumlarda provider request log önce oluşabilir,
    /// job ilişkisi daha sonra atanabilir.
    /// </summary>
    public void AttachImportJob(Guid importJobId)
    {
        if (importJobId == Guid.Empty)
        {
            throw new ArgumentException("ImportJobId boş Guid olamaz.", nameof(importJobId));
        }

        if (ImportJobId == importJobId)
        {
            return;
        }

        ImportJobId = importJobId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Log kaydını ilgili LearningItem ile ilişkilendirir.
    /// 
    /// Örnek:
    /// Azure provider başarılı oldu ve global Word/Phrase LearningItem oluşturuldu.
    /// Bu durumda provider log ilgili LearningItem'a bağlanabilir.
    /// </summary>
    public void AttachLearningItem(Guid learningItemId)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (LearningItemId == learningItemId)
        {
            return;
        }

        LearningItemId = learningItemId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Provider çağrısını başarılı olarak işaretler.
    /// 
    /// Bu method genelde log önce Pending/Unknown oluşturulup sonra güncellenecekse kullanılır.
    /// İlk fazda constructor ile doğrudan Succeeded oluşturmak da yeterli olabilir.
    /// </summary>
    public void MarkAsSucceeded(
        int? durationMs = null,
        int? httpStatusCode = null,
        bool wasServedFromCache = false)
    {
        if (durationMs is < 0)
        {
            throw new ArgumentException("DurationMs negatif olamaz.", nameof(durationMs));
        }

        Status = wasServedFromCache
            ? ProviderRequestStatus.ServedFromCache
            : ProviderRequestStatus.Succeeded;

        WasServedFromCache = wasServedFromCache;
        DurationMs = durationMs;
        HttpStatusCode = httpStatusCode;
        ErrorCode = null;
        ErrorMessage = null;

        MarkAsUpdated();
    }

    /// <summary>
    /// Provider çağrısını hata aldı olarak işaretler.
    /// </summary>
    public void MarkAsFailed(
        string errorCode,
        string errorMessage,
        int? durationMs = null,
        int? httpStatusCode = null)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException("ErrorCode boş olamaz.", nameof(errorCode));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("ErrorMessage boş olamaz.", nameof(errorMessage));
        }

        if (durationMs is < 0)
        {
            throw new ArgumentException("DurationMs negatif olamaz.", nameof(durationMs));
        }

        Status = ProviderRequestStatus.Failed;
        WasServedFromCache = false;
        DurationMs = durationMs;
        HttpStatusCode = httpStatusCode;
        ErrorCode = errorCode.Trim();
        ErrorMessage = errorMessage.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Provider çağrısını timeout olarak işaretler.
    /// </summary>
    public void MarkAsTimeout(
        string? errorMessage = null,
        int? durationMs = null)
    {
        if (durationMs is < 0)
        {
            throw new ArgumentException("DurationMs negatif olamaz.", nameof(durationMs));
        }

        Status = ProviderRequestStatus.Timeout;
        WasServedFromCache = false;
        DurationMs = durationMs;
        HttpStatusCode = null;
        ErrorCode = "TIMEOUT";

        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
            ? "Provider request timed out."
            : errorMessage.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Provider çağrısını rate-limit/kota hatası olarak işaretler.
    /// </summary>
    public void MarkAsRateLimited(
        string? errorMessage = null,
        int? durationMs = null,
        int? httpStatusCode = 429)
    {
        if (durationMs is < 0)
        {
            throw new ArgumentException("DurationMs negatif olamaz.", nameof(durationMs));
        }

        Status = ProviderRequestStatus.RateLimited;
        WasServedFromCache = false;
        DurationMs = durationMs;
        HttpStatusCode = httpStatusCode;
        ErrorCode = "RATE_LIMITED";

        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
            ? "Provider request was rate limited."
            : errorMessage.Trim();

        MarkAsUpdated();
    }
}