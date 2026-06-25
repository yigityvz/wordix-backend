using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wordix.Application.Common.Exceptions;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

namespace Wordix.Api.Middlewares;

/// <summary>
/// Uygulama genelinde oluşan exception'ları merkezi olarak yakalayan middleware'dir.
/// 
/// Neden var?
/// - Controller içinde tekrar tekrar try-catch yazmamak için.
/// - Tüm hata response'larını standart ErrorResponse formatına çevirmek için.
/// - NotFound, BusinessRule, Forbidden, Validation gibi application exception'larını
///   doğru HTTP status code ile döndürmek için.
/// - Beklenmeyen hataları loglayıp frontend'e güvenli bir mesaj dönmek için.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Middleware constructor'ı.
    /// 
    /// RequestDelegate:
    /// Pipeline'daki bir sonraki middleware'i temsil eder.
    /// 
    /// ILogger:
    /// Hataları loglamak için kullanılır.
    /// 
    /// IHostEnvironment:
    /// Development mı Production mı anlamak için kullanılır.
    /// Production ortamında kullanıcıya teknik hata detayı dönmemeliyiz.
    /// </summary>
    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Middleware'in her HTTP request için çalışan ana metodudur.
    /// 
    /// Mantık:
    /// 1. Request pipeline'da ilerler.
    /// 2. Eğer alttaki middleware/controller/endpoint hata fırlatırsa catch bloğu çalışır.
    /// 3. Hata standart ErrorResponse formatına çevrilir.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Request'i pipeline'daki bir sonraki adıma gönderiyoruz.
            // Örneğin Authentication, Authorization, Controller gibi adımlar devam eder.
            await _next(context);
        }
        catch (Exception exception)
        {
            // Eğer response daha önce client'a yazılmaya başlandıysa artık güvenli şekilde
            // yeni bir JSON response yazamayız. Bu durumda hatayı loglayıp tekrar fırlatırız.
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    exception,
                    "The response has already started. ExceptionMiddleware cannot handle this exception.");

                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Yakalanan exception'ı HTTP response'a çeviren yardımcı metoddur.
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Exception tipine göre ErrorResponse oluşturuyoruz.
        var errorResponse = CreateErrorResponse(context, exception);

        // Hatanın önem seviyesine göre loglama yapıyoruz.
        LogException(context, exception, errorResponse.StatusCode);

        // Response'a yazmadan önce status code ve content type ayarlanır.
        context.Response.StatusCode = errorResponse.StatusCode;
        context.Response.ContentType = "application/json";

        // ASP.NET Core default olarak camelCase JSON döndürmeye yakındır.
        // Biz burada açıkça Web defaults kullandık.
        // Böylece StatusCode değil statusCode, ErrorCode değil errorCode döner.
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Exception tipini kontrol edip uygun ErrorResponse modelini üretir.
    /// </summary>
    private ErrorResponse CreateErrorResponse(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        return exception switch
        {
            // Kayıt bulunamadı → HTTP 404
            NotFoundException notFoundException => ErrorResponse.Create(
                statusCode: StatusCodes.Status404NotFound,
                errorCode: "NOT_FOUND",
                message: notFoundException.Message,
                traceId: traceId),

            // İş kuralı ihlali → HTTP 400
            // Örnek: Aynı kelimeyi dictionary'ye ikinci kez kaydetmeye çalışma.
            BusinessRuleException businessRuleException => ErrorResponse.Create(
                statusCode: StatusCodes.Status400BadRequest,
                errorCode: businessRuleException.RuleCode ?? "BUSINESS_RULE_VIOLATION",
                message: businessRuleException.Message,
                traceId: traceId),

            // Yetkisiz kaynak erişimi → HTTP 403
            ForbiddenException forbiddenException => ErrorResponse.Create(
                statusCode: StatusCodes.Status403Forbidden,
                errorCode: "FORBIDDEN",
                message: forbiddenException.Message,
                traceId: traceId),

            // Validation hatası → HTTP 400
            WordixValidationException validationException => ErrorResponse.Create(
                statusCode: StatusCodes.Status400BadRequest,
                errorCode: "VALIDATION_ERROR",
                message: validationException.Message,
                traceId: traceId,
                validationErrors: validationException.Errors),

            // .NET tarafında standart olarak fırlatılabilecek yetkisiz erişim hatası.
            UnauthorizedAccessException unauthorizedAccessException => ErrorResponse.Create(
                statusCode: StatusCodes.Status401Unauthorized,
                errorCode: "UNAUTHORIZED",
                message: unauthorizedAccessException.Message,
                traceId: traceId),

            // Beklenmeyen tüm hatalar → HTTP 500
            _ => ErrorResponse.Create(
                statusCode: StatusCodes.Status500InternalServerError,
                errorCode: "INTERNAL_SERVER_ERROR",
                message: "An unexpected error occurred.",
                detail: GetExceptionDetail(exception),
                traceId: traceId)
        };
    }

    /// <summary>
    /// Development ortamında teknik hata detayını döndürür.
    /// Production ortamında ise null döner.
    /// 
    /// Neden?
    /// Production'da kullanıcıya stack trace, SQL hatası, connection bilgisi gibi
    /// güvenlik açısından riskli detaylar dönülmemelidir.
    /// </summary>
    private string? GetExceptionDetail(Exception exception)
    {
        if (!_environment.IsDevelopment())
        {
            return null;
        }

        return exception.ToString();
    }

    /// <summary>
    /// Hataları merkezi şekilde loglar.
    /// 
    /// 500 seviyesindeki hatalar beklenmeyen sistem hatalarıdır, Error olarak loglanır.
    /// 400/403/404 gibi hatalar çoğunlukla kullanıcı veya business akışı kaynaklıdır,
    /// Warning olarak loglanması yeterlidir.
    /// </summary>
    private void LogException(HttpContext context, Exception exception, int statusCode)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            return;
        }

        _logger.LogWarning(
            exception,
            "Handled exception occurred. StatusCode: {StatusCode}, Method: {Method}, Path: {Path}, TraceId: {TraceId}",
            statusCode,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);
    }
}