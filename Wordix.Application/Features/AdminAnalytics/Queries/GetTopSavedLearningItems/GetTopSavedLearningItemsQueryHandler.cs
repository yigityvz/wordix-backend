using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Application.Features.AdminAnalytics.Services;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetTopSavedLearningItems;

/// <summary>
/// En çok dictionary'ye kaydedilen LearningItem kayıtları analytics query handler'ıdır.
/// 
/// Bu handler LearningItem merkezli çalışır.
/// Böylece sadece Word değil, Phrase ve ileride Sentence içerikleri de analytics'e dahil olabilir.
/// </summary>
public sealed class GetTopSavedLearningItemsQueryHandler
    : IRequestHandler<GetTopSavedLearningItemsQuery, TopSavedLearningItemsAnalyticsResponse>
{
    private readonly IAdminAnalyticsRepository _adminAnalyticsRepository;
    private readonly IAdminActionLogService _adminActionLogService;

    public GetTopSavedLearningItemsQueryHandler(
        IAdminAnalyticsRepository adminAnalyticsRepository,
        IAdminActionLogService adminActionLogService)
    {
        _adminAnalyticsRepository = adminAnalyticsRepository
            ?? throw new ArgumentNullException(nameof(adminAnalyticsRepository));

        _adminActionLogService = adminActionLogService
            ?? throw new ArgumentNullException(nameof(adminActionLogService));
    }

    public async Task<TopSavedLearningItemsAnalyticsResponse> Handle(
        GetTopSavedLearningItemsQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = AdminAnalyticsQueryParameterResolver.ResolveDateRange(
            request.FromUtc,
            request.ToUtc);

        var limit = AdminAnalyticsQueryParameterResolver.ResolveLimit(
            request.Limit);

        var items = await _adminAnalyticsRepository.GetTopSavedLearningItemsAsync(
            dateRange,
            limit,
            cancellationToken);

        var response = AdminAnalyticsMapper.ToTopSavedResponse(
            dateRange,
            limit,
            items);

        await _adminActionLogService.LogCurrentAdminActionAsync(
            AdminAnalyticsConstants.ActionTypes.ViewedTopSavedItems,
            AdminAnalyticsConstants.Descriptions.ViewedTopSavedItems,
            cancellationToken: cancellationToken);

        return response;
    }
}