using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Mappers;
using Wordix.Application.Features.AdminAnalytics.Services;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetMostWrongLearningItems;

/// <summary>
/// Quizlerde en çok yanlış yapılan LearningItem kayıtları analytics query handler'ıdır.
/// 
/// Repository tarafı QuizAnswers + QuizQuestions üzerinden aggregate hesaplama yapar.
/// Handler ise sadece:
/// - tarih aralığı çözer,
/// - limit çözer,
/// - repository çağırır,
/// - mapper ile response üretir,
/// - admin action log yazar.
/// </summary>
public sealed class GetMostWrongLearningItemsQueryHandler
    : IRequestHandler<GetMostWrongLearningItemsQuery, MostWrongLearningItemsAnalyticsResponse>
{
    private readonly IAdminAnalyticsRepository _adminAnalyticsRepository;
    private readonly IAdminActionLogService _adminActionLogService;

    public GetMostWrongLearningItemsQueryHandler(
        IAdminAnalyticsRepository adminAnalyticsRepository,
        IAdminActionLogService adminActionLogService)
    {
        _adminAnalyticsRepository = adminAnalyticsRepository
            ?? throw new ArgumentNullException(nameof(adminAnalyticsRepository));

        _adminActionLogService = adminActionLogService
            ?? throw new ArgumentNullException(nameof(adminActionLogService));
    }

    public async Task<MostWrongLearningItemsAnalyticsResponse> Handle(
        GetMostWrongLearningItemsQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = AdminAnalyticsQueryParameterResolver.ResolveDateRange(
            request.FromUtc,
            request.ToUtc);

        var limit = AdminAnalyticsQueryParameterResolver.ResolveLimit(
            request.Limit);

        var items = await _adminAnalyticsRepository.GetMostWrongLearningItemsAsync(
            dateRange,
            limit,
            cancellationToken);

        var response = AdminAnalyticsMapper.ToMostWrongResponse(
            dateRange,
            limit,
            items);

        await _adminActionLogService.LogCurrentAdminActionAsync(
            AdminAnalyticsConstants.ActionTypes.ViewedMostWrongItems,
            AdminAnalyticsConstants.Descriptions.ViewedMostWrongItems,
            cancellationToken: cancellationToken);

        return response;
    }
}