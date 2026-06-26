using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Features.Quizzes.Commands.StartQuiz;
using Wordix.Application.Features.Quizzes.Requests;
using Wordix.Application.Features.Quizzes.Responses;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

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
        // Request body tamamen boş gelirse command'e map edemeyiz.
        // Bu durumda kendi ValidationException'ımızı fırlatıyoruz.
        // ExceptionMiddleware bunu standart 400 VALIDATION_ERROR formatına çevirir.
        if (request is null)
        {
            throw new WordixValidationException(new[]
            {
                new ValidationError(
                    propertyName: "Body",
                    errorMessage: "Request body is required.",
                    errorCode: "REQUEST_BODY_REQUIRED")
            });
        }

        // API request DTO'sunu Application command modeline manual map ediyoruz.
        // AutoMapper/Mapster kullanmıyoruz.
        var command = new StartQuizCommand
        {
            QuizType = request.QuizType,
            QuizSourceType = request.QuizSourceType,
            QuizContentMode = request.QuizContentMode,
            QuestionCount = request.QuestionCount
        };

        // Command'i MediatR'a gönderiyoruz.
        // Bundan sonra sırasıyla:
        // LoggingBehavior
        // ValidationBehavior
        // StartQuizCommandHandler
        // devreye girer.
        var response = await _sender.Send(command, cancellationToken);

        var apiResponse = ApiResponse<StartQuizResponse>.Ok(
            data: response,
            message: "Quiz started successfully.");

        // Quiz başlatıldığında yeni bir QuizSession oluşturulduğu için
        // 201 Created dönmek REST açısından uygundur.
        //
        // Henüz GET /api/quizzes/{id} endpointimiz olmadığı için
        // CreatedAtAction yerine direkt 201 StatusCode dönüyoruz.
        return StatusCode(
            StatusCodes.Status201Created,
            apiResponse);
    }
}