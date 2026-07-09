using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.HealthChecks;

/// <summary>
/// Wordix database bağlantısını kontrol eden health check sınıfıdır.
/// 
/// Bu class neden Persistence katmanında?
/// - DbContext kullanır.
/// - EF Core database bağlantı detayını bilir.
/// - Application katmanı DbContext bilmemelidir.
/// 
/// Ne kontrol eder?
/// - API, MSSQL database'e bağlanabiliyor mu?
/// - Connection string doğru mu?
/// - SQL Server erişilebilir durumda mı?
/// 
/// Production açısından:
/// Bu check deployment sonrası API'nin gerçekten database'e ulaşabildiğini görmek için kullanılır.
/// </summary>
public sealed class WordixDatabaseHealthCheck : IHealthCheck
{
    private readonly WordixDbContext _dbContext;

    public WordixDatabaseHealthCheck(WordixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Health check çalıştığında çağrılır.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // CanConnectAsync EF Core'un database bağlantısını test eden güvenli methodudur.
            // Burada veri okumuyoruz veya değiştirmiyoruz.
            // Sadece bağlantı kurulabiliyor mu kontrol ediyoruz.
            var canConnect = await _dbContext.Database.CanConnectAsync(
                cancellationToken);

            if (canConnect)
            {
                return HealthCheckResult.Healthy(
                    "Database connection is healthy.");
            }

            return HealthCheckResult.Unhealthy(
                "Database connection could not be established.");
        }
        catch (Exception exception)
        {
            // Exception detayını response'a direkt basmıyoruz.
            // HealthCheckResult exception bilgisini log/diagnostic için taşıyabilir.
            // Response writer tarafında teknik exception detail döndürmeyeceğiz.
            return HealthCheckResult.Unhealthy(
                "Database health check failed.",
                exception);
        }
    }
}