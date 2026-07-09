using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Wordix.Shared.Responses;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// Wordix API rate limiting ayarlarını merkezi olarak yöneten extension sınıfıdır.
/// 
/// Rate limiting neden gerekli?
/// - API'nin tek kullanıcı/IP tarafından aşırı istekle zorlanmasını engeller.
/// - Lookup, quiz answer, statistics gibi endpointlerin kötüye kullanımını azaltır.
/// - Production ortamında basit ama etkili bir koruma katmanı sağlar.
/// 
/// Bu ilk sürümde global fixed window rate limit kullanıyoruz.
/// Yani her kullanıcı/IP belirli süre içinde belirli sayıda request atabilir.
/// </summary>
public static class RateLimitingServiceRegistration
{
    /// <summary>
    /// Wordix API için rate limiting servislerini ekler.
    /// 
    /// Config path:
    /// RateLimiting:Global:PermitLimit
    /// RateLimiting:Global:WindowSeconds
    /// 
    /// Örnek:
    /// "RateLimiting": {
    ///   "Global": {
    ///     "PermitLimit": 120,
    ///     "WindowSeconds": 60
    ///   }
    /// }
    /// </summary>
    public static IServiceCollection AddWordixRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue<int?>("RateLimiting:Global:PermitLimit") ?? 120;
        var windowSeconds = configuration.GetValue<int?>("RateLimiting:Global:WindowSeconds") ?? 60;

        services.AddRateLimiter(options =>
        {
            // Rate limit aşıldığında dönecek HTTP status code.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // GlobalLimiter tüm API endpointleri için geçerli olur.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = ResolvePartitionKey(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        // Belirlenen pencere içinde kaç request'e izin verileceği.
                        PermitLimit = permitLimit,

                        // Pencere süresi.
                        Window = TimeSpan.FromSeconds(windowSeconds),

                        // QueueLimit = 0 demek:
                        // Limit dolduysa request bekletilmez, direkt 429 döner.
                        QueueLimit = 0,

                        // QueueLimit 0 olduğu için pratikte kuyruk kullanılmayacak.
                        // Yine de explicit ayarlıyoruz.
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,

                        // Süre dolunca limit otomatik yenilenir.
                        AutoReplenishment = true
                    });
            });

            // 429 response'unu Wordix'in standart ErrorResponse formatına çeviriyoruz.
            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.ContentType = "application/json";

                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString();
                }

                var errorResponse = ErrorResponse.Create(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    errorCode: "RATE_LIMIT_EXCEEDED",
                    message: "Too many requests. Please try again later.",
                    traceId: httpContext.TraceIdentifier);

                var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

                var json = JsonSerializer.Serialize(
                    errorResponse,
                    jsonOptions);

                await httpContext.Response.WriteAsync(
                    json,
                    cancellationToken);
            };
        });

        return services;
    }

    /// <summary>
    /// Rate limit partition key üretir.
    /// 
    /// Authenticated kullanıcı varsa:
    /// - JWT içindeki sub claim kullanılır.
    /// - Böylece aynı IP arkasındaki farklı kullanıcılar birbirini etkilemez.
    /// 
    /// Kullanıcı authenticated değilse:
    /// - IP adresi kullanılır.
    /// - Böylece token'sız brute force veya public endpoint istekleri de sınırlanır.
    /// </summary>
    private static string ResolvePartitionKey(HttpContext httpContext)
    {
        var keycloakUserId =
            httpContext.User.FindFirst("sub")?.Value ??
            httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return $"user:{keycloakUserId}";
        }

        var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrWhiteSpace(remoteIpAddress))
        {
            return $"ip:{remoteIpAddress}";
        }

        return "anonymous";
    }
}