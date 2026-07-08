using MediatR;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Shared.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;

/// <summary>
/// Current user'ın zorlandığı learning item listesini getiren query modelidir.
/// 
/// Bu query sayfalı response döner:
/// PagedResult&lt;DifficultLearningItemResponse&gt;
/// 
/// Kullanıcı id request'ten alınmaz.
/// Handler current user token'ından KeycloakUserId değerini alır.
/// </summary>
public sealed class GetDifficultItemsQuery
    : IRequest<PagedResult<DifficultLearningItemResponse>>
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

    public int? PageNumber { get; }

    public int? PageSize { get; }

    public string? Source { get; }

    public string? SortBy { get; }

    public string? ItemType { get; }

    public string? LearningStatus { get; }
}