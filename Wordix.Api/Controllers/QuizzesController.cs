using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.Quizzes.Mappers;
using Wordix.Application.Features.Quizzes.Queries.GetQuizSummary;
using Wordix.Application.Features.Quizzes.Requests;
using Wordix.Application.Features.Quizzes.Responses;
using Wordix.Shared.Responses;

namespace Wordix.Api.Controllers;

/// <summary>
/// Quiz işlemlerini yöneten controller.
/// 
/// Bu controller ne yapar?
/// - Kullanıcının quiz başlatma isteğini alır.
/// - HTTP request modelini Application command modeline manual map eder.
/// - Command'i MediatR üzerinden ilgili handler'a gönderir.
/// - Handler'dan gelen response'u standart ApiResponse formatında döner.
/// 
/// Bu controller ne yapmaz?
/// - Current user çözmez.
/// - Dictionary item sorgulamaz.
/// - QuizSession oluşturmaz.
/// - QuizQuestion oluşturmaz.
/// - QuizOption oluşturmaz.
/// - Soru üretme algoritması çalıştırmaz.
/// - DbContext veya repository kullanmaz.
/// 
/// Tüm business logic Application katmanındaki StartQuizCommandHandler içindedir.
/// </summary>
[ApiController]
[Route("api/quizzes")]
[Authorize]
public sealed class QuizzesController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// ISender, MediatR üzerinden command/query göndermek için kullanılır.
    /// 
    /// Neden ISender?
    /// - Bu controller sadece command gönderiyor.
    /// - Notification publish etmiyor.
    /// - IMediator yerine daha dar interface kullanmak daha temizdir.
    /// </summary>
    public QuizzesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Kullanıcının dictionary'sinden multiple choice test quiz başlatır.
    /// 
    /// Endpoint:
    /// POST /api/quizzes
    /// 
    /// İlk prototipte desteklenen request:
    /// {
    ///   "quizType": "Test",
    ///   "quizSourceType": "Dictionary",
    ///   "quizContentMode": "WordsOnly",
    ///   "questionCount": 5
    /// }
    /// 
    /// Akış:
    /// 1. Request body alınır.
    /// 2. StartQuizCommand'e manual map edilir.
    /// 3. MediatR'a gönderilir.
    /// 4. ValidationBehavior çalışır.
    /// 5. StartQuizCommandHandler çalışır.
    /// 6. QuizSession + QuizQuestion + QuizOption kayıtları oluşur.
    /// 7. Sorular ve seçenekler response olarak döner.
    /// 
    /// Önemli:
    /// Response içinde seçeneklerin doğru/yanlış bilgisi dönmez.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StartQuizResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StartQuizResponse>>> StartQuiz(
    [FromBody] StartQuizRequest? request,
    CancellationToken cancellationToken)
    {
        // Request DTO → Command dönüşümü feature mapper üzerinden yapılır.
        //
        // Controller burada null body, boş quizType veya questionCount kontrolü yapmaz.
        // Eğer request null gelirse mapper boş/default değerlerle command üretir.
        // StartQuizCommandValidator bu durumu ValidationBehavior üzerinden yakalar.
        var command = QuizMapper.ToStartQuizCommand(request);

        var response = await _sender.Send(command, cancellationToken);

        var apiResponse = ApiResponse<StartQuizResponse>.Ok(
            data: response,
            message: "Quiz started successfully.");

        return StatusCode(
            StatusCodes.Status201Created,
            apiResponse);
    }

    /// <summary>
    /// Current user'ın quiz session'ındaki bir soruya cevap gönderir.
    /// 
    /// Endpoint:
    /// POST /api/quizzes/{quizSessionId}/answers
    /// 
    /// Body:
    /// {
    ///   "selectedQuizOptionId": "...",
    ///   "questionResponseTimeInMilliseconds": 3500
    /// }
    /// </summary>
    [HttpPost("{quizSessionId:guid}/answers")]
    [ProducesResponseType(typeof(ApiResponse<SubmitQuizAnswerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SubmitQuizAnswerResponse>>> SubmitAnswer(
    [FromRoute] Guid quizSessionId,
    [FromBody] SubmitQuizAnswerRequest? request,
    CancellationToken cancellationToken)
    {
        // Route'tan gelen quizSessionId ve request body,
        // feature mapper üzerinden SubmitQuizAnswerCommand modeline dönüştürülür.
        //
        // Controller burada null body veya Guid.Empty kontrolü yapmaz.
        // SubmitQuizAnswerCommandValidator bu kontrolleri ValidationBehavior üzerinden yapar.
        var command = QuizMapper.ToSubmitQuizAnswerCommand(
            quizSessionId,
            request);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(
            ApiResponse<SubmitQuizAnswerResponse>.Ok(
                response,
                "Quiz answer submitted successfully."));
    }

    /// <summary>
    /// Current user'ın quiz session summary bilgisini getirir.
    /// 
    /// Endpoint:
    /// GET /api/quizzes/{quizSessionId}/summary
    /// </summary>
    [HttpGet("{quizSessionId:guid}/summary")]
    [ProducesResponseType(typeof(ApiResponse<QuizSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QuizSummaryResponse>>> GetSummary(
    [FromRoute] Guid quizSessionId,
    CancellationToken cancellationToken)
    {
        // Route'tan gelen quizSessionId query modeline aktarılır.
        //
        // Controller burada Guid.Empty kontrolü yapmaz.
        // GetQuizSummaryQueryValidator bu kontrolü ValidationBehavior üzerinden yapar.
        var query = new GetQuizSummaryQuery(quizSessionId);

        var response = await _sender.Send(
            query,
            cancellationToken);

        return Ok(
            ApiResponse<QuizSummaryResponse>.Ok(
                response,
                "Quiz summary retrieved successfully."));
    }

}