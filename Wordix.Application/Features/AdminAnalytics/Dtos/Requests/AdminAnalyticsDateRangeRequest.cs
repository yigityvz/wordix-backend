namespace Wordix.Application.Features.AdminAnalytics.Dtos.Requests;

/// <summary>
/// Admin analytics endpointlerinde kullanılan ortak tarih aralığı request DTO'sudur.
/// 
/// Bu DTO neden var?
/// - Dashboard ve provider stats gibi endpointler tarih aralığı alabilir.
/// - Controller query string'den gelen fromUtc/toUtc değerlerini bu DTO ile karşılar.
/// - Controller içinde validation yapılmaz; validation MediatR query validatorlarında yapılır.
/// 
/// Örnek query string:
/// GET /api/admin/analytics/dashboard?fromUtc=2026-07-01&toUtc=2026-07-08
/// </summary>
public sealed class AdminAnalyticsDateRangeRequest
{
    /// <summary>
    /// Analiz başlangıç tarihidir.
    /// 
    /// Null gelirse handler varsayılan tarih aralığını kullanabilir.
    /// </summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>
    /// Analiz bitiş tarihidir.
    /// 
    /// Null gelirse handler güncel UTC zamanı kullanabilir.
    /// </summary>
    public DateTime? ToUtc { get; init; }
}