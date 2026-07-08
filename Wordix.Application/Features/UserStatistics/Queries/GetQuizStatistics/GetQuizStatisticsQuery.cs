using MediatR;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetQuizStatistics;

/// <summary>
/// Current user'ın quiz istatistiklerini getiren query modelidir.
/// 
/// Filtreler query string'den gelir:
/// - fromUtc
/// - toUtc
/// - quizType
/// - quizSourceType
/// - quizContentMode
/// - difficultyGroup
/// 
/// Bu class sadece veri taşıma modelidir.
/// Gerçek filtre normalizasyonu handler tarafında yapılacaktır.
/// </summary>
public sealed class GetQuizStatisticsQuery
    : IRequest<QuizStatisticsResponse>
{
    public GetQuizStatisticsQuery(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? quizType = null,
        string? quizSourceType = null,
        string? quizContentMode = null,
        string? difficultyGroup = null)
    {
        FromUtc = fromUtc;
        ToUtc = toUtc;
        QuizType = quizType;
        QuizSourceType = quizSourceType;
        QuizContentMode = quizContentMode;
        DifficultyGroup = difficultyGroup;
    }

    public DateTime? FromUtc { get; }

    public DateTime? ToUtc { get; }

    public string? QuizType { get; }

    public string? QuizSourceType { get; }

    public string? QuizContentMode { get; }

    public string? DifficultyGroup { get; }
}