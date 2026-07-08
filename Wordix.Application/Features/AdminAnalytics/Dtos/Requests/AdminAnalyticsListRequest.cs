namespace Wordix.Application.Features.AdminAnalytics.Dtos.Requests;

/// <summary>
/// Liste dönen admin analytics endpointleri için ortak request DTO'sudur.
/// 
/// Bu DTO hangi endpointlerde kullanılacak?
/// - Top searches
/// - Top saved
/// - Most wrong
/// 
/// Neden ayrı DTO?
/// - Bu endpointlerde tarih aralığına ek olarak limit bilgisi gerekir.
/// - Limit boş gelirse handler/mapper default limit kullanabilir.
/// </summary>
public sealed class AdminAnalyticsListRequest
{
    /// <summary>
    /// Analiz başlangıç tarihidir.
    /// </summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>
    /// Analiz bitiş tarihidir.
    /// </summary>
    public DateTime? ToUtc { get; init; }

    /// <summary>
    /// Dönecek maksimum kayıt sayısıdır.
    /// 
    /// Null gelirse DefaultLimit kullanılacaktır.
    /// MaxLimit üstüne çıkmasına izin verilmeyecektir.
    /// </summary>
    public int? Limit { get; init; }
}