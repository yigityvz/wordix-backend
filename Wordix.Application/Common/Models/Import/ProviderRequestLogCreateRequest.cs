using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// ProviderRequestLog oluşturmak için kullanılan Application modelidir.
/// 
/// Bu model neden var?
/// - ProviderRequestLog entity constructor'ı çok sayıda bilgi ister.
/// - Handler/provider/service tarafında uzun parametre listeleri kullanmak istemeyiz.
/// - Azure, Tatoeba, Kaikki gibi farklı provider akışları aynı standart modelle log yazabilir.
/// 
/// Bu model entity değildir.
/// Sadece provider log oluşturma request modelidir.
/// </summary>
public sealed record ProviderRequestLogCreateRequest
{
    /// <summary>
    /// Provider çağrısının bağlı olduğu ImportJob id değeridir.
    /// 
    /// Runtime lookup gibi user-triggered çağrılarda null kalabilir.
    /// Import/enrichment job içinde çalışıyorsa dolu olmalıdır.
    /// </summary>
    public Guid? ImportJobId { get; init; }

    /// <summary>
    /// Provider çağrısını tetikleyen Keycloak kullanıcısıdır.
    /// 
    /// Sistem/background job çağrılarında null kalabilir.
    /// </summary>
    public string? KeycloakUserId { get; init; }

    /// <summary>
    /// Provider çağrısı sonucunda oluşan veya ilişkili olan LearningItem id değeridir.
    /// 
    /// İlk log oluşturulurken null olabilir.
    /// Daha sonra AttachLearningItemAsync ile doldurulabilir.
    /// </summary>
    public Guid? LearningItemId { get; init; }

    /// <summary>
    /// Provider türüdür.
    /// 
    /// Örnek:
    /// Translation, Dictionary, ExampleSentence, Import.
    /// </summary>
    public ProviderType ProviderType { get; init; }

    /// <summary>
    /// Provider adıdır.
    /// 
    /// Örnek:
    /// AzureTranslator, Tatoeba, WiktionaryKaikki.
    /// </summary>
    public string ProviderName { get; init; } = string.Empty;

    /// <summary>
    /// Provider üzerinde yapılan operasyon adıdır.
    /// 
    /// Örnek:
    /// Translate
    /// ParseTatoebaExampleSentences
    /// EnrichKaikkiMeanings
    /// </summary>
    public string OperationName { get; init; } = string.Empty;

    /// <summary>
    /// Provider request durumudur.
    /// 
    /// Örnek:
    /// Succeeded, Failed, ServedFromCache, Timeout, RateLimited.
    /// </summary>
    public ProviderRequestStatus Status { get; init; }

    /// <summary>
    /// Provider çağrısını teknik olarak benzersizleştiren key değeridir.
    /// 
    /// Örnek:
    /// azure:translate:en:tr:sleep
    /// tatoeba:example-enrichment:en:tr:links_sample.csv
    /// </summary>
    public string RequestKey { get; init; } = string.Empty;

    /// <summary>
    /// Provider çağrısına konu olan normalize input değeridir.
    /// 
    /// Örnek:
    /// sleep
    /// take responsibility
    /// </summary>
    public string? NormalizedInput { get; init; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string? SourceLanguageCode { get; init; }

    /// <summary>
    /// Hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string? TargetLanguageCode { get; init; }

    /// <summary>
    /// Sonuç cache'den mi döndü?
    /// 
    /// Status ServedFromCache ise service bu değeri otomatik true kabul eder.
    /// </summary>
    public bool WasServedFromCache { get; init; }

    /// <summary>
    /// Provider çağrısının kaç milisaniye sürdüğüdür.
    /// </summary>
    public int? DurationMs { get; init; }

    /// <summary>
    /// HTTP tabanlı provider çağrılarında dönen status code değeridir.
    /// </summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// Provider hata kodudur.
    /// 
    /// Örnek:
    /// TIMEOUT, RATE_LIMITED, AZURE_TRANSLATOR_ERROR.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Provider hata mesajıdır.
    /// 
    /// Buraya secret, token, API key veya authorization header yazılmamalıdır.
    /// </summary>
    public string? ErrorMessage { get; init; }
}