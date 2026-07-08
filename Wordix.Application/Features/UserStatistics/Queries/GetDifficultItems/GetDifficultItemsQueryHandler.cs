using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;
using Wordix.Application.Features.UserStatistics.Services;
using Wordix.Shared.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;

/// <summary>
/// GetDifficultItemsQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Pagination/filter parametrelerini DifficultItemsFilter modeline dönüştürür.
/// - Repository üzerinden difficult items listesini sayfalı olarak çeker.
/// - PagedResult&lt;Model&gt; değerini PagedResult&lt;Response&gt; değerine dönüştürür.
/// 
/// Bu handler ne yapmaz?
/// - Difficult flag oluşturmaz.
/// - Progress güncellemez.
/// - DbContext kullanmaz.
/// - Listeyi memory'de sayfalayarak performans sorunu oluşturmaz.
/// </summary>
public sealed class GetDifficultItemsQueryHandler
    : IRequestHandler<GetDifficultItemsQuery, PagedResult<DifficultLearningItemResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public GetDifficultItemsQueryHandler(
        ICurrentUserService currentUserService,
        IUserStatisticsRepository userStatisticsRepository)
    {
        _currentUserService = currentUserService;
        _userStatisticsRepository = userStatisticsRepository;
    }

    public async Task<PagedResult<DifficultLearningItemResponse>> Handle(
        GetDifficultItemsQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        var filter = UserStatisticsQueryParameterResolver.ResolveDifficultItemsFilter(
            request);

        var pagedModel = await _userStatisticsRepository.GetDifficultItemsAsync(
            keycloakUserId,
            filter,
            cancellationToken);

        return UserStatisticsMapper.ToDifficultItemsResponse(pagedModel);
    }
}