using MediatR;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDeckStatistics;

/// <summary>
/// GetDeckStatisticsQuery isteğini işleyen handler'dır.
/// 
/// Bu handler current user'ın deck bazlı learning ve quiz statistics verilerini döndürür.
/// 
/// Ownership:
/// - Handler user id almaz.
/// - Current user token'ından KeycloakUserId alır.
/// - Repository sadece bu kullanıcıya ait deckleri analiz eder.
/// </summary>
public sealed class GetDeckStatisticsQueryHandler
    : IRequestHandler<GetDeckStatisticsQuery, DeckStatisticsResponse>
{
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public GetDeckStatisticsQueryHandler(
        IUserStatisticsRepository userStatisticsRepository)
    {
        _userStatisticsRepository = userStatisticsRepository;
    }

    public async Task<DeckStatisticsResponse> Handle(
        GetDeckStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = request.KeycloakUserId;

        var models = await _userStatisticsRepository.GetDeckStatisticsAsync(
            keycloakUserId,
            cancellationToken);

        return UserStatisticsMapper.ToDeckStatisticsResponse(models);
    }
}