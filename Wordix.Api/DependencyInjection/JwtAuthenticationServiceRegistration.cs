using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Keycloak JWT authentication ayarlarını içerir.
/// Program.cs içinde JWT detaylarının kalabalık oluşturmasını engeller.
/// </summary>
public static class JwtAuthenticationServiceRegistration
{
    /// <summary>
    /// Keycloak tarafından üretilen JWT access tokenlarını doğrulamak için JwtBearer authentication ekler.
    /// </summary>
    public static IServiceCollection AddWordixJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtSettings["Authority"];
                options.Audience = jwtSettings["Audience"];
                options.RequireHttpsMetadata = jwtSettings.GetValue<bool>("RequireHttpsMetadata");

                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["ValidIssuer"],

                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;

                        if (identity is null)
                        {
                            return Task.CompletedTask;
                        }

                        var realmAccessClaim = context.Principal?.FindFirst("realm_access")?.Value;

                        if (string.IsNullOrWhiteSpace(realmAccessClaim))
                        {
                            return Task.CompletedTask;
                        }

                        using var realmAccessJson = JsonDocument.Parse(realmAccessClaim);

                        if (!realmAccessJson.RootElement.TryGetProperty("roles", out var rolesElement))
                        {
                            return Task.CompletedTask;
                        }

                        foreach (var roleElement in rolesElement.EnumerateArray())
                        {
                            var roleName = roleElement.GetString();

                            if (string.IsNullOrWhiteSpace(roleName))
                            {
                                continue;
                            }

                            if (!identity.HasClaim(ClaimTypes.Role, roleName))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}