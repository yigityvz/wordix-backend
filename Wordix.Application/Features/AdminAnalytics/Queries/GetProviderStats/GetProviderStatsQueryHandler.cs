using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Application.Features.AdminAnalytics.Services;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetProviderStats;

/// <summary>
/// Provider/cache/import istatistikleri analytics query handler'ıdır.
/// 
/// Bu handler Faz 24'te kurduğumuz ProviderRequestLog, ExternalContentCache ve ImportJob
/// altyapısını admin dashboard için okunabilir rapora dönüştürür.
/// </summary>
public sealed class GetProviderStatsQueryHandler
    : IRequestHandler<GetProviderStatsQuery, ProviderStatsAnalyticsResponse>
{
    private readonly IAdminAnalyticsRepository _adminAnalyticsRepository;
    private readonly IAdminActionLogService _adminActionLogService;

    public GetProviderStatsQueryHandler(
        IAdminAnalyticsRepository adminAnalyticsRepository,
        IAdminActionLogService adminActionLogService)
    {
        _adminAnalyticsRepository = adminAnalyticsRepository
            ?? throw new ArgumentNullException(nameof(adminAnalyticsRepository));

        _adminActionLogService = adminActionLogService
            ?? throw new ArgumentNullException(nameof(adminActionLogService));
    }

    public async Task<ProviderStatsAnalyticsResponse> Handle(
        GetProviderStatsQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = AdminAnalyticsQueryParameterResolver.ResolveDateRange(
            request.FromUtc,
            request.ToUtc);

        var items = await _adminAnalyticsRepository.GetProviderStatsAsync(
            dateRange,
            cancellationToken);

        var response = AdminAnalyticsMapper.ToProviderStatsResponse(
            dateRange,
            items);

        await _adminActionLogService.LogCurrentAdminActionAsync(
            AdminAnalyticsConstants.ActionTypes.ViewedProviderStats,
            AdminAnalyticsConstants.Descriptions.ViewedProviderStats,
            cancellationToken: cancellationToken);

        return response;
    }
}