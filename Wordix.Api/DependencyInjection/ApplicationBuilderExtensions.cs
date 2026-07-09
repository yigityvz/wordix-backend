using Wordix.Api.Middlewares;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// WebApplication pipeline ayarlarını merkezi olarak topladığımız extension class.
/// 
/// Neden var?
/// - Program.cs dosyasını sade tutmak için.
/// - Middleware sırasını tek dosyada yönetmek için.
/// - Swagger, Authentication, Authorization ve Controller mapping ayarlarını
///   okunabilir şekilde bir arada tutmak için.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Wordix API request pipeline'ını yapılandırır.
    /// 
    /// Program.cs içinde sadece:
    /// app.UseWordixApiPipeline();
    /// şeklinde çağrılır.
    /// </summary>
    public static WebApplication UseWordixApiPipeline(this WebApplication app)
    {
        // ExceptionMiddleware'i pipeline'ın en başına yakın ekliyoruz.
        // Böylece alttaki middleware/controller/endpointlerde oluşan hataları
        // merkezi olarak yakalayıp standart ErrorResponse formatına çevirebilir.
        app.UseMiddleware<ExceptionMiddleware>();

        // Swagger sadece Development ortamında açık tutulur.
        // Production ortamında API dokümantasyonunu herkese açmak güvenlik açısından
        // ayrıca değerlendirilmelidir.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // HTTP isteklerini HTTPS'e yönlendirmek için kullanılır.
        // Local geliştirmede bazı durumlarda port/sertifika ayarlarına göre davranışı değişebilir.
        app.UseHttpsRedirection();

        // CORS, browser tabanlı frontend isteklerinde origin kontrolü yapar.
        //
        // Authentication/Authorization'dan önce konumlandırıyoruz.
        // Böylece tarayıcının preflight OPTIONS istekleri auth engeline takılmadan
        // CORS policy tarafından doğru şekilde cevaplanabilir.
        app.UseCors(WordixCorsPolicies.Frontend);

        // Authentication:
        // Kullanıcının kim olduğunu belirler.
        // JWT token geçerli mi, claimler okunabiliyor mu burada kontrol edilir.
        app.UseAuthentication();

        // RateLimiter'ı Authentication'dan sonra koyuyoruz.
        // Böylece authenticated kullanıcılarda partition key olarak KeycloakUserId kullanılabilir.
        // Token yoksa IP bazlı limit uygulanır.
        app.UseRateLimiter();

        // Authorization:
        // Kullanıcının ilgili endpoint'e erişim yetkisi var mı kontrol eder.
        // Örnek: AdminOnly policy, BasicUserOnly policy.
        app.UseAuthorization();

        // Controller endpointlerini pipeline'a ekler.
        app.MapControllers();

        // Health check endpointleri.
        //
        // Bu endpointler authentication gerektirmez.
        // Çünkü deployment, monitoring veya container orchestration sistemleri
        // servis sağlığını token almadan kontrol edebilmelidir.
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        })
            .AllowAnonymous();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("live"),
            ResponseWriter = WriteHealthCheckResponse
        })
            .AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        })
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// Health check response'unu okunabilir JSON formatında döndürür.
    /// 
    /// Teknik exception detail döndürmeyiz.
    /// Production'da database connection string, SQL hatası veya stack trace gibi bilgiler
    /// response'a sızmamalıdır.
    /// </summary>
    private static Task WriteHealthCheckResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(
                report.TotalDuration.TotalMilliseconds,
                2,
                MidpointRounding.AwayFromZero),

            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = Math.Round(
                    entry.Value.Duration.TotalMilliseconds,
                    2,
                    MidpointRounding.AwayFromZero)
            })
        };

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var json = JsonSerializer.Serialize(
            response,
            jsonOptions);

        return context.Response.WriteAsync(json);
    }
}