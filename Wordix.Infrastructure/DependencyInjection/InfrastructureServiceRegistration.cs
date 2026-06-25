using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Infrastructure.Identity;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Infrastructure.Dictionary;

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
    public static IServiceCollection AddWordixInfrastructure(this IServiceCollection services)
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

        // Lookup dictionary provider:
        // Faz 13'te prototype provider kullanıyoruz.
        // Faz 24'te gerçek provider/import sistemi geldiğinde bu registration değiştirilebilir.
        services.AddSingleton<IDictionaryProvider, PrototypeDictionaryProvider>();

        return services;
    }
}