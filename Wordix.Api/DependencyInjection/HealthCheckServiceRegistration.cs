using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wordix.Persistence.HealthChecks;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Wordix API health check servis kayıtlarını merkezi olarak yöneten extension sınıfıdır.
/// 
/// Health check neden gerekli?
/// - API ayakta mı?
/// - Database bağlantısı çalışıyor mu?
/// - Deployment sonrası uygulama istek almaya hazır mı?
/// 
/// Production ortamlarında load balancer, container orchestrator veya monitoring sistemi
/// bu endpointleri kullanarak servisin durumunu kontrol edebilir.
/// </summary>
public static class HealthCheckServiceRegistration
{
    /// <summary>
    /// Wordix health check servislerini DI container'a ekler.
    /// </summary>
    public static IServiceCollection AddWordixHealthChecks(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()

            // Self check:
            // Uygulama process olarak ayakta mı kontrol eder.
            // Database'e gitmez.
            .AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy("Wordix API is running."),
                tags: new[] { "live" })

            // Database readiness check:
            // Uygulama database'e bağlanabiliyor mu kontrol eder.
            .AddCheck<WordixDatabaseHealthCheck>(
                name: "database",
                tags: new[] { "ready", "database" });

        return services;
    }
}