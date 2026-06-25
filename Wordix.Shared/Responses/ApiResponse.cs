namespace Wordix.Shared.Responses;

/// <summary>
/// API'den dönen başarılı response'ları standart hale getiren modeldir.
/// 
/// Neden var?
/// - Frontend tarafı her endpoint için farklı başarılı response formatı beklemesin.
/// - "success", "message", "data" ve "timestamp" alanları her başarılı cevapta tutarlı olsun.
/// - Controller veya handler tarafında response üretimi daha okunabilir hale gelsin.
/// </summary>
public sealed class ApiResponse
{

    /// <summary>
    /// Dışarıdan new ApiResponse() kullanımını engellemek için private constructor kullandık.
    /// Böylece response üretimi factory method üzerinden kontrollü yapılır.
    /// </summary>
    private ApiResponse()
    {
    }

    /// <summary>
    /// Data dönmeyen başarılı işlemler için standart response üretir.
    /// Örnek:
    /// - Silme işlemi başarılı
    /// - Aktivasyon işlemi başarılı
    /// - Logout başarılı
    /// </summary>
    public static ApiResponse Ok(string message = "Operation completed successfully.")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// İşlemin başarılı olup olmadığını gösterir.
    /// Başarılı response için her zaman true olur.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Kullanıcıya veya frontend'e gösterilebilecek kısa mesajdır.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Response'un üretildiği UTC zaman bilgisidir.
    /// Loglama ve debugging için faydalıdır.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

}

/// <summary>
/// İçinde data taşıyan başarılı API response modelidir.
/// 
/// Örnek kullanım:
/// ApiResponse<UserProfileDto>.Ok(profileDto)
/// ApiResponse<IReadOnlyList<LanguageDto>>.Ok(languages)
/// </summary>
/// <typeparam name="TData">
/// Response içinde dönecek data tipidir.
/// </typeparam>
public sealed class ApiResponse<TData>
{
    /// <summary>
    /// Response üretimini factory methodlara yönlendirmek için private constructor kullandık.
    /// </summary>
    private ApiResponse()
    {
    }

    /// <summary>
    /// Data içeren başarılı response üretir.
    /// </summary>
    public static ApiResponse<TData> Ok(
        TData data,
        string message = "Operation completed successfully.")
    {
        return new ApiResponse<TData>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }


    /// <summary>
    /// İşlemin başarılı olup olmadığını gösterir.
    /// Başarılı response için her zaman true olur.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Kullanıcıya veya frontend'e gösterilebilecek kısa mesajdır.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Endpoint'in döndürmek istediği asıl veridir.
    /// Örnek:
    /// - UserProfileDto
    /// - LookupResponse
    /// - QuizSummaryResponse
    /// - Liste response'ları
    /// </summary>
    public TData? Data { get; init; }

    /// <summary>
    /// Response'un üretildiği UTC zaman bilgisidir.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

   
}