using MediatR;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Queries.GetQuizSummary;

/// <summary>
/// Bir quiz session'ın summary bilgisini getiren query modelidir.
/// 
/// Neden query?
/// - Sistem durumunu değiştirmez.
/// - Sadece mevcut QuizSession, QuizQuestion ve QuizAnswer kayıtlarından özet üretir.
/// </summary>
public sealed record GetQuizSummaryQuery(Guid QuizSessionId)
    : IRequest<QuizSummaryResponse>;
