using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.UserStatistics.Dtos.Requests;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Current user'ın öğrenme istatistiklerini ve dashboard verilerini yöneten controller.
/// 
/// Bu controller ne yapar?
/// - Kullanıcının learning summary bilgisini döner.
/// - Kullanıcının quiz istatistiklerini döner.
/// - Kullanıcının zorlandığı itemları sayfalı şekilde döner.
/// - Kullanıcının deck bazlı istatistiklerini döner.
/// - Kullanıcının confidence score dağılımını döner.
/// 
/// Bu controller ne yapmaz?
/// - Current user çözmez.
/// - User id request'ten almaz.
/// - DbContext veya repository kullanmaz.
/// - Statistics hesaplamaz.
/// - Enum parse etmez.
/// - Validation yapmaz.
/// 
/// Current user bilgisi handler içinde ICurrentUserService üzerinden alınır.
/// Böylece client başka bir user id göndererek başka kullanıcının verisini isteyemez.
/// </summary>
[ApiController]
[Route("api/user-statistics")]
[Authorize]
public sealed class UserStatisticsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden query göndermek için kullanılır.
    /// 
    /// Controller sadece query oluşturur ve gönderir.
    /// Gerçek business/use-case akışı Application handler içinde çalışır.
    /// </summary>
    public UserStatisticsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Current user'ın genel öğrenme özetini döner.
    /// 
    /// Endpoint:
    /// GET /api/user-statistics/learning-summary
    /// 
    /// Bu endpoint dashboard üst kartları için kullanılabilir:
    /// - Toplam kaydedilen item sayısı
    /// - Word/Phrase/Sentence dağılımı
    /// - Learning status dağılımı
    /// - Review zamanı gelen item sayısı
    /// - Ortalama confidence score
    /// - Flag sayıları
    /// - Genel quiz accuracy oranı
    /// </summary>
    [HttpGet("learning-summary")]
    [ProducesResponseType(typeof(ApiResponse<UserLearningSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserLearningSummaryResponse>>> GetLearningSummary(
        CancellationToken cancellationToken)
    {
        var query = UserStatisticsMapper.ToGetUserLearningSummaryQuery();

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<UserLearningSummaryResponse>.Ok(
            data: response,
            message: "Learning summary retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın quiz istatistiklerini döner.
    /// 
    /// Endpoint:
    /// GET /api/user-statistics/quizzes
    /// 
    /// Desteklenen query string filtreleri:
    /// - fromUtc
    /// - toUtc
    /// - quizType
    /// - quizSourceType
    /// - quizContentMode
    /// - difficultyGroup
    /// 
    /// Örnek:
    /// GET /api/user-statistics/quizzes?quizType=Test&quizSourceType=UserDictionary
    /// 
    /// Tarih aralığı gönderilmezse handler tarafında son 30 gün varsayımı uygulanır.
    /// </summary>
    [HttpGet("quizzes")]
    [ProducesResponseType(typeof(ApiResponse<QuizStatisticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<QuizStatisticsResponse>>> GetQuizStatistics(
        [FromQuery] QuizStatisticsRequest request,
        CancellationToken cancellationToken)
    {
        // Query string DTO -> MediatR query dönüşümü mapper üzerinden yapılır.
        //
        // Controller burada:
        // - fromUtc/toUtc kontrolü yapmaz.
        // - quizType enum parse etmez.
        // - current user çözmez.
        //
        // ValidationBehavior validator'ı çalıştırır.
        // Handler parametreleri normalize eder.
        var query = UserStatisticsMapper.ToGetQuizStatisticsQuery(request);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<QuizStatisticsResponse>.Ok(
            data: response,
            message: "Quiz statistics retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın zorlandığı learning itemları sayfalı şekilde döner.
    /// 
    /// Endpoint:
    /// GET /api/user-statistics/difficult-items
    /// 
    /// Desteklenen query string parametreleri:
    /// - pageNumber
    /// - pageSize
    /// - source
    /// - sortBy
    /// - itemType
    /// - learningStatus
    /// 
    /// Örnek:
    /// GET /api/user-statistics/difficult-items?pageNumber=1&pageSize=20&source=both&sortBy=confidenceAsc
    /// 
    /// source değerleri:
    /// - both
    /// - manual
    /// - progress
    /// 
    /// sortBy değerleri:
    /// - confidenceAsc
    /// - wrongCountDesc
    /// - consecutiveWrongDesc
    /// - nextReviewAsc
    /// - savedAtDesc
    /// </summary>
    [HttpGet("difficult-items")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DifficultLearningItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<DifficultLearningItemResponse>>>> GetDifficultItems(
        [FromQuery] DifficultItemsRequest request,
        CancellationToken cancellationToken)
    {
        // Request DTO -> Query dönüşümü mapper üzerinden yapılır.
        //
        // Controller pagination veya filter validation yapmaz.
        // GetDifficultItemsQueryValidator bu kuralları ValidationBehavior üzerinden uygular.
        var query = UserStatisticsMapper.ToGetDifficultItemsQuery(request);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<DifficultLearningItemResponse>>.Ok(
            data: response,
            message: "Difficult learning items retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın deck bazlı öğrenme ve quiz istatistiklerini döner.
    /// 
    /// Endpoint:
    /// GET /api/user-statistics/decks
    /// 
    /// Bu endpoint şunları döner:
    /// - Deck item sayısı
    /// - Ortalama confidence score
    /// - Due review item sayısı
    /// - Difficult item sayısı
    /// - Deck üzerinden çözülen quiz sayısı
    /// - Deck quiz accuracy oranı
    /// </summary>
    [HttpGet("decks")]
    [ProducesResponseType(typeof(ApiResponse<DeckStatisticsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DeckStatisticsResponse>>> GetDeckStatistics(
        CancellationToken cancellationToken)
    {
        var query = UserStatisticsMapper.ToGetDeckStatisticsQuery();

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<DeckStatisticsResponse>.Ok(
            data: response,
            message: "Deck statistics retrieved successfully."));
    }

    /// <summary>
    /// Current user'ın confidence score dağılımını döner.
    /// 
    /// Endpoint:
    /// GET /api/user-statistics/confidence-distribution
    /// 
    /// Bu endpoint frontend tarafında grafik/chart oluşturmak için uygundur.
    /// 
    /// Bucket örnekleri:
    /// - 0-20 VeryLow
    /// - 21-40 Low
    /// - 41-60 Medium
    /// - 61-80 High
    /// - 81-100 VeryHigh
    /// </summary>
    [HttpGet("confidence-distribution")]
    [ProducesResponseType(typeof(ApiResponse<ConfidenceScoreDistributionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ConfidenceScoreDistributionResponse>>> GetConfidenceScoreDistribution(
        CancellationToken cancellationToken)
    {
        var query = UserStatisticsMapper.ToGetConfidenceScoreDistributionQuery();

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(ApiResponse<ConfidenceScoreDistributionResponse>.Ok(
            data: response,
            message: "Confidence score distribution retrieved successfully."));
    }
}