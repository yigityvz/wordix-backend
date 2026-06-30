namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Wordix API authorization policy tanımlarını içerir.
/// 
/// Roller Keycloak realm role olarak yönetilir.
/// JwtAuthenticationServiceRegistration, token içindeki realm_access.roles değerlerini
/// ASP.NET Core role claimlerine dönüştürdüğü için burada RequireRole kullanılabilir.
/// 
/// Örnek:
/// - Keycloak realm role: admin
/// - Backend policy: AdminOnly -> RequireRole("admin")
/// 
/// Bu yapı sayesinde backend kullanıcı rolünü kendi database'inden değil,
/// Keycloak token'ındaki rol bilgisinden okur.
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