using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;

namespace Wordix.Application.Features.UserStatistics.Queries.GetConfidenceScoreDistribution;

/// <summary>
/// GetConfidenceScoreDistributionQuery isteğini işleyen handler'dır.
/// 
/// Bu handler current user'ın dictionary itemları üzerindeki confidence score dağılımını döndürür.
/// Frontend bu response ile grafik/chart oluşturabilir.
/// </summary>
public sealed class GetConfidenceScoreDistributionQueryHandler
    : IRequestHandler<GetConfidenceScoreDistributionQuery, ConfidenceScoreDistributionResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public GetConfidenceScoreDistributionQueryHandler(
        ICurrentUserService currentUserService,
        IUserStatisticsRepository userStatisticsRepository)
    {
        _currentUserService = currentUserService;
        _userStatisticsRepository = userStatisticsRepository;
    }

    public async Task<ConfidenceScoreDistributionResponse> Handle(
        GetConfidenceScoreDistributionQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        var model = await _userStatisticsRepository.GetConfidenceScoreDistributionAsync(
            keycloakUserId,
            cancellationToken);

        return UserStatisticsMapper.ToConfidenceScoreDistributionResponse(model);
    }
}