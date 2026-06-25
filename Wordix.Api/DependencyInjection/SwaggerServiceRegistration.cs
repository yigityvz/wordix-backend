using Microsoft.OpenApi;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Swagger/OpenAPI ile ilgili servis kayıtlarını içerir.
/// Program.cs dosyasının şişmemesi için Swagger ayarlarını bu dosyada topluyoruz.
/// </summary>
public static class SwaggerServiceRegistration
{
    /// <summary>
    /// Wordix API için Swagger dokümantasyonu ve Bearer token desteğini ekler.
    /// </summary>
    public static IServiceCollection AddWordixSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Wordix API",
                Version = "v1",
                Description = "Wordix language learning backend API"
            });

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Keycloak access_token değerini buraya yapıştır. 'Bearer' yazma, sadece token'ı gir."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        return services;
    }
}