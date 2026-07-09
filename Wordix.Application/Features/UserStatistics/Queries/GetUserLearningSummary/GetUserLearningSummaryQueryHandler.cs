using MediatR;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;

namespace Wordix.Application.Features.UserStatistics.Queries.GetUserLearningSummary;

/// <summary>
/// GetUserLearningSummaryQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - IUserStatisticsRepository üzerinden learning summary modelini çeker.
/// - Modeli UserLearningSummaryResponse DTO'suna dönüştürür.
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - SQL sorgusu yazmaz.
/// - HTTP/Controller detayı bilmez.
/// - Response propertylerini tek tek elle dizmez.
/// </summary>
public sealed class GetUserLearningSummaryQueryHandler
    : IRequestHandler<GetUserLearningSummaryQuery, UserLearningSummaryResponse>
{
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public GetUserLearningSummaryQueryHandler(
        IUserStatisticsRepository userStatisticsRepository)
    {
        _userStatisticsRepository = userStatisticsRepository;
    }

    public async Task<UserLearningSummaryResponse> Handle(
        GetUserLearningSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = request.KeycloakUserId;
        var model = await _userStatisticsRepository.GetLearningSummaryAsync(
            keycloakUserId,
            cancellationToken);

        return UserStatisticsMapper.ToUserLearningSummaryResponse(model);
    }
}