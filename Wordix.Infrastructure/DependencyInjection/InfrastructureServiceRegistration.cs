using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Infrastructure.Identity;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Infrastructure.Dictionary;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Infrastructure.Providers.Import;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Wordix.Application.Common.Interfaces.Translation;
using Wordix.Infrastructure.Options;
using Wordix.Infrastructure.Providers.Translation;

namespace Wordix.Infrastructure.DependencyInjection;

/// <summary>
/// Infrastructure katmanındaki servislerin DI kayıtlarını yapan extension class.
/// 
/// Neden var?
/// - Program.cs dosyasını sade tutmak için.
/// - Infrastructure servis kayıtlarını tek yerde toplamak için.
/// - Dış dünya/teknik servis implementasyonlarını Application interface'lerine bağlamak için.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Wordix Infrastructure servislerini DI container'a ekler.
    /// 
    /// Burada:
    /// - Current user servisi
    /// - Dış provider servisleri
    /// - DateTime servisleri
    /// - Cache/file/provider log servisleri
    /// gibi teknik implementasyonlar ileride kaydedilecektir.
    /// </summary>
    public static IServiceCollection AddWordixInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // IHttpContextAccessor, o anki HTTP request'in HttpContext bilgisine erişmek için kullanılır.
        // KeycloakCurrentUserService token claimlerini HttpContext.User üzerinden okuyacağı için gereklidir.
        services.AddHttpContextAccessor();

        // Application katmanı ICurrentUserService interface'ini bilir.
        // Infrastructure katmanı bu interface'in Keycloak/JWT tabanlı gerçek implementasyonunu sağlar.
        //
        // Scoped seçiyoruz çünkü current user bilgisi HTTP request'e bağlıdır.
        // Her request içinde aynı kullanıcı bilgisi kullanılmalıdır.
        services.AddScoped<ICurrentUserService, KeycloakCurrentUserService>();

        // CEFR-J CSV provider kaydı.
        //
        // Application katmanı sadece ICefrWordListProvider interface'ini bilir.
        // Gerçek CSV okuma implementasyonu Infrastructure katmanındadır.
        services.AddScoped<ICefrWordListProvider, CefrJCsvWordListProvider>();

        // Kaikki/Wiktionary meaning provider kaydı.
        //
        // Application katmanı sadece IMeaningImportProvider interface'ini bilir.
        // Gerçek JSONL okuma implementasyonu Infrastructure katmanındadır.
        //
        // Bu provider şu aşamada database'e kayıt atmaz.
        // Sadece Kaikki JSONL datasını MeaningImportRow listesine çevirir.
        services.AddScoped<IMeaningImportProvider, KaikkiMeaningImportProvider>();

        // FreeDict English-Turkish TEI XML provider kaydı.
        //
        // Bu provider IFreeDictMeaningImportProvider marker interface'i ile kaydedilir.
        // Böylece mevcut Kaikki IMeaningImportProvider kaydı bozulmaz.
        services.AddScoped<IFreeDictMeaningImportProvider, FreeDictMeaningImportProvider>();

        services.AddScoped<IExampleSentenceImportProvider, TatoebaExampleSentenceImportProvider>();

        // Azure Translator options kaydı.
        //
        // Gerçek API key appsettings.json içine yazılmamalıdır.
        // Development ortamında user-secrets veya environment variable üzerinden okunmalıdır.
        services.Configure<AzureTranslatorOptions>(
            configuration.GetSection(AzureTranslatorOptions.SectionName));

        // Azure Translator HTTP provider kaydı.
        //
        // Application katmanı ITranslationProvider interface'ini bilir.
        // Gerçek HTTP/Azure implementasyonu Infrastructure katmanındadır.
        services.AddHttpClient<ITranslationProvider, AzureTranslatorProvider>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<AzureTranslatorOptions>>()
                .Value;

            var timeoutSeconds = options.TimeoutSeconds <= 0
                ? 10
                : Math.Min(options.TimeoutSeconds, 60);

            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        // Lookup dictionary provider:
        //
        // Production mindset:
        // DB'de bulunmayan lookup istekleri artık prototype sabit veri provider'ına değil,
        // Azure Translator tabanlı fallback provider'a gider.
        //
        // Scoped kullanıyoruz çünkü bu provider ITranslationProvider typed HttpClient bağımlılığı kullanır.
        services.AddScoped<IDictionaryProvider, AzureTranslationDictionaryProvider>();

        return services;
    }
}