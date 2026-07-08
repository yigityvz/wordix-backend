using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.AdminAnalytics.Dtos.Requests;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Admin analytics endpointlerini yöneten controller.
/// 
/// Bu controller ne yapar?
/// - Admin dashboard için sistem geneli özet verileri döner.
/// - En çok aranan içerikleri döner.
/// - En çok dictionary'ye kaydedilen içerikleri döner.
/// - Quizlerde en çok yanlış yapılan içerikleri döner.
/// - Provider/cache/import istatistiklerini döner.
/// 
/// Bu controller ne yapmaz?
/// - DbContext kullanmaz.
/// - SQL GroupBy/Count/Average sorgusu yazmaz.
/// - Current admin kullanıcısını çözmez.
/// - AdminActionLog kaydını kendisi oluşturmaz.
/// - Validation yapmaz.
/// - Business rule içermez.
/// 
/// Bunların tamamı Application/Persistence katmanlarında yapılır:
/// - Query validation: FluentValidation + ValidationBehavior
/// - Query handling: AdminAnalytics query handlerları
/// - Aggregate SQL sorguları: IAdminAnalyticsRepository implementasyonu
/// - Response mapping: AdminAnalyticsMapper
/// - Admin audit logging: IAdminActionLogService
/// </summary>
[ApiController]
[Route("api/admin/analytics")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminAnalyticsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden query göndermek için kullanılır.
    /// </summary>
    public AdminAnalyticsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Admin dashboard için sistem geneli özet analytics verisini döner.
    /// 
    /// Endpoint:
    /// GET /api/admin/analytics/dashboard
    /// 
    /// Opsiyonel query string:
    /// - fromUtc
    /// - toUtc
    /// 
    /// Örnek:
    /// GET /api/admin/analytics/dashboard?fromUtc=2026-07-01&toUtc=2026-07-08
    /// 
    /// Bu endpoint şunları özetler:
    /// - Lookup sayıları
    /// - Dictionary save sayıları
    /// - Quiz session/answer istatistikleri
    /// - Provider/cache/import job özetleri
    /// 
    /// Güvenlik:
    /// Sadece Keycloak realm role "admin" olan kullanıcılar erişebilir.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardAnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AdminDashboardAnalyticsResponse>>> GetDashboard(
        [FromQuery] AdminAnalyticsDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        // Request DTO -> Query dönüşümü feature mapper üzerinden yapılır.
        //
        // Controller burada tarih aralığı kontrolü yapmaz.
        // FromUtc > ToUtc gibi validation kuralları FluentValidation pipeline'da çalışır.
        var query = AdminAnalyticsMapper.ToGetAdminDashboardQuery(request);

        var response = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<AdminDashboardAnalyticsResponse>.Ok(
            data: response,
            message: "Admin dashboard analytics retrieved successfully."));
    }

    /// <summary>
    /// Sistemde en çok aranan lookup metinlerini döner.
    /// 
    /// Endpoint:
    /// GET /api/admin/analytics/top-searches
    /// 
    /// Opsiyonel query string:
    /// - fromUtc
    /// - toUtc
    /// - limit
    /// 
    /// Örnek:
    /// GET /api/admin/analytics/top-searches?limit=20
    /// 
    /// Bu endpoint admin'e şunu gösterir:
    /// - Kullanıcılar en çok ne arıyor?
    /// - Arama database'de bulunmuş mu?
    /// - Provider kullanılmış mı?
    /// - Provider sonucunda yeni içerik oluşturulmuş mu?
    /// </summary>
    [HttpGet("top-searches")]
    [ProducesResponseType(typeof(ApiResponse<TopSearchesAnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TopSearchesAnalyticsResponse>>> GetTopSearches(
        [FromQuery] AdminAnalyticsListRequest request,
        CancellationToken cancellationToken)
    {
        var query = AdminAnalyticsMapper.ToGetTopSearchesQuery(request);

        var response = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<TopSearchesAnalyticsResponse>.Ok(
            data: response,
            message: "Top searched items analytics retrieved successfully."));
    }

    /// <summary>
    /// Kullanıcılar tarafından en çok dictionary'ye kaydedilen LearningItem kayıtlarını döner.
    /// 
    /// Endpoint:
    /// GET /api/admin/analytics/top-saved
    /// 
    /// Opsiyonel query string:
    /// - fromUtc
    /// - toUtc
    /// - limit
    /// 
    /// Bu endpoint LearningItem merkezli çalışır.
    /// Yani sadece Word değil, Phrase ve ileride Sentence içerikleri de desteklenir.
    /// </summary>
    [HttpGet("top-saved")]
    [ProducesResponseType(typeof(ApiResponse<TopSavedLearningItemsAnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TopSavedLearningItemsAnalyticsResponse>>> GetTopSavedLearningItems(
        [FromQuery] AdminAnalyticsListRequest request,
        CancellationToken cancellationToken)
    {
        var query = AdminAnalyticsMapper.ToGetTopSavedLearningItemsQuery(request);

        var response = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<TopSavedLearningItemsAnalyticsResponse>.Ok(
            data: response,
            message: "Top saved learning items analytics retrieved successfully."));
    }

    /// <summary>
    /// Quizlerde en çok yanlış yapılan LearningItem kayıtlarını döner.
    /// 
    /// Endpoint:
    /// GET /api/admin/analytics/most-wrong
    /// 
    /// Opsiyonel query string:
    /// - fromUtc
    /// - toUtc
    /// - limit
    /// 
    /// Bu endpoint şunları gösterir:
    /// - Hangi içeriklerde en çok yanlış yapılmış?
    /// - Yanlış oranı nedir?
    /// - Ortalama cevap süresi nedir?
    /// - Sistem önerisi sorularda kaç yanlış var?
    /// </summary>
    [HttpGet("most-wrong")]
    [ProducesResponseType(typeof(ApiResponse<MostWrongLearningItemsAnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MostWrongLearningItemsAnalyticsResponse>>> GetMostWrongLearningItems(
        [FromQuery] AdminAnalyticsListRequest request,
        CancellationToken cancellationToken)
    {
        var query = AdminAnalyticsMapper.ToGetMostWrongLearningItemsQuery(request);

        var response = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<MostWrongLearningItemsAnalyticsResponse>.Ok(
            data: response,
            message: "Most wrong learning items analytics retrieved successfully."));
    }

    /// <summary>
    /// Provider, cache ve import job istatistiklerini döner.
    /// 
    /// Endpoint:
    /// GET /api/admin/analytics/provider-stats
    /// 
    /// Opsiyonel query string:
    /// - fromUtc
    /// - toUtc
    /// 
    /// Bu endpoint Faz 24'te kurulan provider/import altyapısını görünür hale getirir:
    /// - ProviderRequestLogs
    /// - ExternalContentCaches
    /// - ImportJobs
    /// 
    /// Admin bu endpoint ile Azure, FreeDict, Tatoeba gibi kaynakların
    /// başarı/hata/cache/import durumunu izleyebilir.
    /// </summary>
    [HttpGet("provider-stats")]
    [ProducesResponseType(typeof(ApiResponse<ProviderStatsAnalyticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProviderStatsAnalyticsResponse>>> GetProviderStats(
        [FromQuery] AdminAnalyticsDateRangeRequest request,
        CancellationToken cancellationToken)
    {
        var query = AdminAnalyticsMapper.ToGetProviderStatsQuery(request);

        var response = await _sender.Send(query, cancellationToken);

        return Ok(ApiResponse<ProviderStatsAnalyticsResponse>.Ok(
            data: response,
            message: "Provider statistics analytics retrieved successfully."));
    }
}