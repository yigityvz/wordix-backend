namespace Wordix.Infrastructure.Options;

/// <summary>
/// Azure Translator provider için configuration ayarlarını temsil eder.
/// 
/// Bu class neden Infrastructure katmanında?
/// - Azure endpoint, subscription key, region gibi bilgiler teknik provider detaylarıdır.
/// - Application katmanı Azure'ın nasıl configure edildiğini bilmemelidir.
/// - Application sadece ITranslationProvider interface'ini bilir.
/// </summary>
public sealed class AzureTranslatorOptions
{
    /// <summary>
    /// appsettings / user-secrets / environment variable altında kullanılacak config section adıdır.
    /// </summary>
    public const string SectionName = "TranslationProviders:AzureTranslator";

    /// <summary>
    /// Azure provider aktif mi?
    /// 
    /// Development sırasında provider'ı kapatmak istersek false yapılabilir.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Azure Translator endpoint adresidir.
    /// 
    /// Global text translation endpoint genellikle:
    /// https://api.cognitive.microsofttranslator.com
    /// </summary>
    public string Endpoint { get; init; } = "https://api.cognitive.microsofttranslator.com";

    /// <summary>
    /// Azure Translator subscription key değeridir.
    /// 
    /// Güvenlik notu:
    /// Bu değer appsettings.json içine yazılmamalıdır.
    /// Development için user-secrets veya environment variable kullanılmalıdır.
    /// </summary>
    public string? SubscriptionKey { get; init; }

    /// <summary>
    /// Azure resource region değeridir.
    /// 
    /// Örnek:
    /// - westeurope
    /// - eastus
    /// - global resource kullanılıyorsa boş bırakılabilir.
    /// 
    /// Azure dokümantasyonuna göre regional veya multi-service resource kullanıldığında
    /// region header'ı gerekir.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// HTTP isteği için timeout süresidir.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 10;
}