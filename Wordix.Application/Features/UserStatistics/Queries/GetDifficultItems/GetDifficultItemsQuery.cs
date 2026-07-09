using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Shared.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;

/// <summary>
/// Current user'ın zorlandığı learning item listesini getiren query modelidir.
/// 
/// Bu query sayfalı response döner:
/// PagedResult&lt;DifficultLearningItemResponse&gt;
/// 
/// Kullanıcı id client'tan alınmaz.
/// CurrentUserBehavior tarafından request.KeycloakUserId üzerine yazılır.
/// </summary>
public sealed class GetDifficultItemsQuery
    : IRequest<PagedResult<DifficultLearningItemResponse>>, IRequiresCurrentUser
{
    public GetDifficultItemsQuery(
        int? pageNumber = null,
        int? pageSize = null,
        string? source = null,
        string? sortBy = null,
        string? itemType = null,
        string? learningStatus = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        Source = source;
        SortBy = sortBy;
        ItemType = itemType;
        LearningStatus = learningStatus;
    }

    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    public int? PageNumber { get; }

    public int? PageSize { get; }

    public string? Source { get; }

    public string? SortBy { get; }

    public string? ItemType { get; }

    public string? LearningStatus { get; }
}