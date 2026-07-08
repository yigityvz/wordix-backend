using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Application.Features.AdminAnalytics.Services;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetTopSearches;

/// <summary>
/// En çok aranan metinler analytics query handler'ıdır.
/// 
/// Kaynak veri repository tarafında LookupHistories tablosundan aggregate edilir.
/// Handler sadece use-case akışını yönetir.
/// </summary>
public sealed class GetTopSearchesQueryHandler
    : IRequestHandler<GetTopSearchesQuery, TopSearchesAnalyticsResponse>
{
    private readonly IAdminAnalyticsRepository _adminAnalyticsRepository;
    private readonly IAdminActionLogService _adminActionLogService;

    public GetTopSearchesQueryHandler(
        IAdminAnalyticsRepository adminAnalyticsRepository,
        IAdminActionLogService adminActionLogService)
    {
        _adminAnalyticsRepository = adminAnalyticsRepository
            ?? throw new ArgumentNullException(nameof(adminAnalyticsRepository));

        _adminActionLogService = adminActionLogService
            ?? throw new ArgumentNullException(nameof(adminActionLogService));
    }

    public async Task<TopSearchesAnalyticsResponse> Handle(
        GetTopSearchesQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = AdminAnalyticsQueryParameterResolver.ResolveDateRange(
            request.FromUtc,
            request.ToUtc);

        var limit = AdminAnalyticsQueryParameterResolver.ResolveLimit(
            request.Limit);

        var items = await _adminAnalyticsRepository.GetTopSearchesAsync(
            dateRange,
            limit,
            cancellationToken);

        var response = AdminAnalyticsMapper.ToTopSearchesResponse(
            dateRange,
            limit,
            items);

        await _adminActionLogService.LogCurrentAdminActionAsync(
            AdminAnalyticsConstants.ActionTypes.ViewedTopSearches,
            AdminAnalyticsConstants.Descriptions.ViewedTopSearches,
            cancellationToken: cancellationToken);

        return response;
    }
}