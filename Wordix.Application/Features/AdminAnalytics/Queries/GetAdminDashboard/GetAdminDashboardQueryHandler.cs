using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Application.Features.AdminAnalytics.Services;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetAdminDashboard;

/// <summary>
/// Admin dashboard analytics query handler'ıdır.
/// 
/// Bu handler ne yapar?
/// - Query tarih aralığını çözer.
/// - Repository'den dashboard aggregate verisini alır.
/// - Mapper ile API response DTO'ya dönüştürür.
/// - Admin'in dashboard görüntülediğini AdminActionLogs tablosuna yazar.
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - SQL aggregate sorgusu yazmaz.
/// - Response propertylerini tek tek controller içinde dizmez.
/// </summary>
public sealed class GetAdminDashboardQueryHandler
    : IRequestHandler<GetAdminDashboardQuery, AdminDashboardAnalyticsResponse>
{
    private readonly IAdminAnalyticsRepository _adminAnalyticsRepository;
    private readonly IAdminActionLogService _adminActionLogService;

    public GetAdminDashboardQueryHandler(
        IAdminAnalyticsRepository adminAnalyticsRepository,
        IAdminActionLogService adminActionLogService)
    {
        _adminAnalyticsRepository = adminAnalyticsRepository
            ?? throw new ArgumentNullException(nameof(adminAnalyticsRepository));

        _adminActionLogService = adminActionLogService
            ?? throw new ArgumentNullException(nameof(adminActionLogService));
    }

    public async Task<AdminDashboardAnalyticsResponse> Handle(
        GetAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = AdminAnalyticsQueryParameterResolver.ResolveDateRange(
            request.FromUtc,
            request.ToUtc);

        var dashboardModel = await _adminAnalyticsRepository.GetDashboardAsync(
            dateRange,
            cancellationToken);

        var response = AdminAnalyticsMapper.ToDashboardResponse(dashboardModel);

        await _adminActionLogService.LogCurrentAdminActionAsync(
            AdminAnalyticsConstants.ActionTypes.ViewedAdminDashboard,
            AdminAnalyticsConstants.Descriptions.ViewedAdminDashboard,
            cancellationToken: cancellationToken);

        return response;
    }
}