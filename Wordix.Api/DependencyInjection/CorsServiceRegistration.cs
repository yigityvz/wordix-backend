namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Wordix API CORS ayarlarını merkezi olarak yöneten extension sınıfıdır.
/// 
/// CORS neden gerekli?
/// - Frontend uygulaması backend API'den farklı bir origin üzerinde çalışabilir.
/// - Örneğin Angular frontend http://localhost:4200 üzerinde,
///   backend ise https://localhost:7000 üzerinde çalışabilir.
/// - Tarayıcı güvenlik modeli gereği backend'in bu origin'e izin vermesi gerekir.
/// 
/// Production hardening kararı:
/// - AllowAnyOrigin kullanılmaz.
/// - İzin verilen originler appsettings üzerinden yönetilir.
/// - Böylece production ortamında sadece gerçek frontend domainleri API'ye erişebilir.
/// </summary>
public static class CorsServiceRegistration
{
    /// <summary>
    /// Wordix için CORS policy kaydını yapar.
    /// 
    /// Config path:
    /// Cors:AllowedOrigins
    /// 
    /// Örnek:
    /// "Cors": {
    ///   "AllowedOrigins": [
    ///     "http://localhost:4200",
    ///     "https://localhost:4200"
    ///   ]
    /// }
    /// </summary>
    public static IServiceCollection AddWordixCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(
                name: WordixCorsPolicies.Frontend,
                configurePolicy: policy =>
                {
                    // Güvenlik kararı:
                    // Origin listesi appsettings/env üzerinden gelir.
                    // Production'da AllowAnyOrigin kesinlikle kullanılmaz.
                    policy.WithOrigins(allowedOrigins)
                        .WithMethods(
                            HttpMethods.Get,
                            HttpMethods.Post,
                            HttpMethods.Put,
                            HttpMethods.Delete,
                            HttpMethods.Patch,
                            HttpMethods.Options)
                        .AllowAnyHeader();
                });
        });

        return services;
    }
}

/// <summary>
/// CORS policy adlarını merkezi olarak tutar.
/// 
/// Magic string kullanımını azaltmak için ayrı sınıfta tanımlıyoruz.
/// </summary>
public static class WordixCorsPolicies
{
    /// <summary>
    /// Frontend uygulamalarının kullanacağı CORS policy adıdır.
    /// </summary>
    public const string Frontend = "WordixFrontendCors";
}