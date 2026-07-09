using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// Kullanıcının quiz sorusuna cevap verme use-case command modelidir.
/// 
/// Test quiz:
/// - SelectedQuizOptionId dolu gelir.
/// - QuizQuestionId opsiyoneldir.
/// - UserAnswer genellikle null gelir.
/// 
/// Writing quiz:
/// - QuizQuestionId dolu gelir.
/// - UserAnswer dolu gelir.
/// - SelectedQuizOptionId null gelir.
/// 
/// Hangi alanın zorunlu olduğu quiz session tipine göre handler içinde kontrol edilir.
/// Çünkü validator tek başına quiz tipini database'den bilemez.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// - Handler quiz session ownership ve answer ownership için request.KeycloakUserId değerini kullanır.
/// </summary>
public sealed record SubmitQuizAnswerCommand(
    Guid QuizSessionId,
    Guid? QuizQuestionId,
    Guid? SelectedQuizOptionId,
    string? UserAnswer,
    int? QuestionResponseTimeInMilliseconds)
    : IRequest<SubmitQuizAnswerResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}