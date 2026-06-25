using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Wordix.Application.Common.Behaviors;

/// <summary>
/// MediatR command/query requestleri için merkezi logging pipeline behavior'dır.
/// 
/// Bu class ne yapar?
/// - Her MediatR request başlamadan önce log atar.
/// - Handler başarılı biterse geçen süreyi loglar.
/// - Handler veya sonraki pipeline behavior hata fırlatırsa hatayı loglar ve tekrar fırlatır.
/// 
/// Neden var?
/// - Controller veya handler içine tek tek log yazmamak için.
/// - Command/query bazlı takip yapabilmek için.
/// - Performans ölçümü için request süresini görebilmek için.
/// </summary>
/// <typeparam name="TRequest">
/// MediatR'a gönderilen command/query tipidir.
/// Örnek:
/// - GetCurrentUserProfileQuery
/// - CreateLookupCommand
/// </typeparam>
/// <typeparam name="TResponse">
/// Handler'ın döndürdüğü response tipidir.
/// </typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// ILogger, ASP.NET Core built-in logging altyapısı üzerinden log yazmamızı sağlar.
    /// </summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// MediatR pipeline'da request handler'a gitmeden önce ve handler'dan sonra çalışan methoddur.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Stopwatch ile handler'ın ne kadar sürdüğünü ölçüyoruz.
        // Bu özellikle lookup/provider gibi ileride yavaşlayabilecek işlemleri fark etmek için faydalıdır.
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling MediatR request: {RequestName}",
            requestName);

        try
        {
            // Pipeline'daki bir sonraki adıma geçiyoruz.
            // Bu, sıradaki behavior veya asıl handler olabilir.
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled MediatR request: {RequestName} in {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            // Hatayı burada logluyoruz ama yutmuyoruz.
            // Mutlaka tekrar throw ediyoruz ki ExceptionMiddleware standart ErrorResponse dönebilsin.
            _logger.LogError(
                exception,
                "MediatR request failed: {RequestName} after {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}