using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Difficult items repository sorgusunda kullanılacak filtre modelidir.
/// 
/// Bu model API request DTO değildir.
/// Request DTO'dan gelen nullable/string değerler handler/mapper tarafında normalize edilerek bu modele çevrilir.
/// </summary>
public sealed class DifficultItemsFilter
{
    public int PageNumber { get; init; } = UserStatisticsConstants.DefaultPageNumber;

    public int PageSize { get; init; } = UserStatisticsConstants.DefaultPageSize;

    public string Source { get; init; } = UserStatisticsConstants.DifficultItemSources.Both;

    public string SortBy { get; init; } = UserStatisticsConstants.DifficultItemSortBy.ConfidenceAsc;

    public LearningItemType? ItemType { get; init; }

    public LearningStatus? LearningStatus { get; init; }
}