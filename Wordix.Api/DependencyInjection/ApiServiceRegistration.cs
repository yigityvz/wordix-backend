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
        services.AddControllers(options =>
        {
            // Empty JSON body geldiğinde ASP.NET Core'un action'a girmeden
            // otomatik model binding hatası üretmesini istemiyoruz.
            //
            // Bizim validation standardımız:
            // Controller -> Mapper -> Command/Query -> ValidationBehavior -> FluentValidation
            //
            // Bu ayar sayesinde boş body null olarak action'a geçebilir.
            // Mapper null request'i default command'e çevirir.
            // Validator eksik alanları standart VALIDATION_ERROR formatında döner.
            options.AllowEmptyInputInBodyModelBinding = true;
        });

#warning Swagger UI production ortamında kapatılmalıdır. Bu uyarıyı dikkate alınız.
        services.AddWordixSwagger(); 

        services.AddWordixJwtAuthentication(configuration);

        services.AddWordixAuthorization();

        // Application katmanındaki use-case servislerini ekler.
        // Örnek: currentUserProfileService.
        services.AddWordixApplication();

        // EF Core, DbContext ve MSSQL bağlantı ayarları Persistence katmanında tanımlıdır.
        // Program.cs şişmesin diye burada sadece extension methodu çağırıyoruz.
        services.AddWordixPersistence(configuration);

        // Infrastructure katmanını ekler.
        // CurrentUserService gibi teknik implementasyonlar burada register edilir.
        services.AddWordixInfrastructure(configuration);

        return services;
    }
}