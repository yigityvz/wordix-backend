using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
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
/// 
/// Kullanıcı id client'tan alınmaz.
/// CurrentUserBehavior tarafından request.KeycloakUserId üzerine yazılır.
/// </summary>
public sealed class GetQuizStatisticsQuery
    : IRequest<QuizStatisticsResponse>, IRequiresCurrentUser
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

    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    public DateTime? FromUtc { get; }

    public DateTime? ToUtc { get; }

    public string? QuizType { get; }

    public string? QuizSourceType { get; }

    public string? QuizContentMode { get; }

    public string? DifficultyGroup { get; }
}