using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Queries.GetQuizSummary;

/// <summary>
/// Bir quiz session'ın summary bilgisini getiren query modelidir.
/// 
/// Neden query?
/// - Sistem durumunu değiştirmez.
/// - Sadece mevcut QuizSession, QuizQuestion ve QuizAnswer kayıtlarından özet üretir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// - Handler quiz session ownership kontrolü için request.KeycloakUserId değerini kullanır.
/// </summary>
public sealed record GetQuizSummaryQuery(Guid QuizSessionId)
    : IRequest<QuizSummaryResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}