using Wordix.Persistence.DependencyInjection;
using Wordix.Infrastructure.DependencyInjection;
using Wordix.Application.DependencyInjection;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// API katmanına ait servis kayıtlarını tek noktadan çağırır.
/// Program.cs dosyası bu merkezi extension methodu çağırır.
/// </summary>
public static class ApiServiceRegistration
{
    /// <summary>
    /// Wordix.Api katmanının ihtiyaç duyduğu servisleri ekler.
    /// </summary>
    public static IServiceCollection AddWordixApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.AddWordixSwagger();

        services.AddWordixJwtAuthentication(configuration);

        services.AddWordixAuthorization();

        // Application katmanındaki use-case servislerini ekler.
        // Örnek: UserProfileSyncService.
        services.AddWordixApplication();

        // EF Core, DbContext ve MSSQL bağlantı ayarları Persistence katmanında tanımlıdır.
        // Program.cs şişmesin diye burada sadece extension methodu çağırıyoruz.
        services.AddWordixPersistence(configuration);

        // Infrastructure katmanını ekler.
        // CurrentUserService gibi teknik implementasyonlar burada register edilir.
        services.AddWordixInfrastructure();

        return services;
    }
}