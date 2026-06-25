namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Wordix API authorization policy kayıtlarını içerir.
/// </summary>
public static class AuthorizationServiceRegistration
{
    /// <summary>
    /// Role based authorization policylerini ekler.
    /// </summary>
    public static IServiceCollection AddWordixAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("BasicUserOnly", policy =>
                policy.RequireRole("basic_user"));

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("admin"));
        });

        return services;
    }
}