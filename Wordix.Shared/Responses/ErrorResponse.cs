namespace Wordix.Shared.Responses;

/// <summary>
/// API'den dönen hata cevaplarını standart hale getiren modeldir.
/// 
/// Neden var?
/// - ExceptionMiddleware tüm hataları bu modele çevirir.
/// - Frontend her hata için aynı response yapısını görür.
/// - 400, 401, 403, 404, 500 gibi HTTP durumları aynı JSON formatında döner.
/// </summary>
public sealed class ErrorResponse
{

    /// <summary>
    /// Response üretimini kontrollü yapmak için private constructor kullandık.
    /// </summary>
    private ErrorResponse()
    {
    }

    /// <summary>
    /// Standart hata response'u üretir.
    /// ExceptionMiddleware içinde bu methodu kullanacağız.
    /// </summary>
    public static ErrorResponse Create(
        int statusCode,
        string errorCode,
        string message,
        string? detail = null,
        string? traceId = null,
        IReadOnlyCollection<ValidationError>? validationErrors = null)
    {
        return new ErrorResponse
        {
            Success = false,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message,
            Detail = detail,
            TraceId = traceId,
            ValidationErrors = validationErrors ?? Array.Empty<ValidationError>()
        };
    }

    /// <summary>
    /// Hata response'larında her zaman false olur.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// HTTP status code bilgisidir.
    /// Örnek:
    /// - 400 Bad Request
    /// - 403 Forbidden
    /// - 404 Not Found
    /// - 500 Internal Server Error
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Frontend'in programatik olarak kontrol edebileceği hata kodudur.
    /// 
    /// Örnek:
    /// - VALIDATION_ERROR
    /// - NOT_FOUND
    /// - BUSINESS_RULE_VIOLATION
    /// - FORBIDDEN
    /// - INTERNAL_SERVER_ERROR
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcıya veya frontend'e gösterilebilecek ana hata mesajıdır.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Debugging için ek detay bilgisidir.
    /// Production ortamında hassas bilgi dönmemek için dikkatli kullanılmalıdır.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Request'in izini sürmek için kullanılır.
    /// Middleware tarafında HttpContext.TraceIdentifier ile dolduracağız.
    /// Loglarda aynı traceId ile hatayı bulmak kolaylaşır.
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// FluentValidation veya custom validation hataları burada listelenir.
    /// Validation hatası yoksa boş liste döner.
    /// </summary>
    public IReadOnlyCollection<ValidationError> ValidationErrors { get; init; } = Array.Empty<ValidationError>();

    /// <summary>
    /// Response'un üretildiği UTC zaman bilgisidir.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    
}