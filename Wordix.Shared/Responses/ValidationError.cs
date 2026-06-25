namespace Wordix.Shared.Responses;

/// <summary>
/// Validation hatalarını frontend'e düzenli şekilde döndürmek için kullanılan modeldir.
/// 
/// Neden ayrı model?
/// - FluentValidation birden fazla alan için hata üretebilir.
/// - Frontend hangi input alanında hangi hata olduğunu bu model üzerinden anlar.
/// - Örneğin "text" alanı boşsa, frontend direkt text inputunun altında hata gösterebilir.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Dışarıdan object initializer ile üretime izin veriyoruz.
    /// Çünkü bu model basit bir data taşıyıcıdır.
    /// </summary>
    public ValidationError()
    {
    }

    /// <summary>
    /// Daha kısa ve okunabilir üretim için yardımcı constructor.
    /// </summary>
    public ValidationError(
        string propertyName,
        string errorMessage,
        string? errorCode = null)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Hatanın oluştuğu property/field adıdır.
    /// Örnek:
    /// - Text
    /// - Email
    /// - QuestionCount
    /// </summary>
    public string PropertyName { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcıya veya frontend'e gösterilecek hata mesajıdır.
    /// Örnek:
    /// - Text cannot be empty.
    /// - Email format is invalid.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// İleride FluentValidation ErrorCode kullanırsak burada taşıyabiliriz.
    /// Şimdilik optional bıraktık.
    /// </summary>
    public string? ErrorCode { get; init; }

   
}