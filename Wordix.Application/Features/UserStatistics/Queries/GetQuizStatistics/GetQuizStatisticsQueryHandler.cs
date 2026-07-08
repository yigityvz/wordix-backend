using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Mappers;
using Wordix.Application.Features.UserStatistics.Services;

namespace Wordix.Application.Features.UserStatistics.Queries.GetQuizStatistics;

/// <summary>
/// GetQuizStatisticsQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Query parametrelerini QuizStatisticsFilter modeline dönüştürür.
/// - Repository üzerinden quiz statistics modelini çeker.
/// - Modeli API response DTO'suna dönüştürür.
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - QuizAnswer tablosunu direkt sorgulamaz.
/// - Enum validation yapmaz; validator yapar.
/// - Controller veya HttpContext bilmez.
/// </summary>
public sealed class GetQuizStatisticsQueryHandler
    : IRequestHandler<GetQuizStatisticsQuery, QuizStatisticsResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserStatisticsRepository _userStatisticsRepository;

    public GetQuizStatisticsQueryHandler(
        ICurrentUserService currentUserService,
        IUserStatisticsRepository userStatisticsRepository)
    {
        _currentUserService = currentUserService;
        _userStatisticsRepository = userStatisticsRepository;
    }

    public async Task<QuizStatisticsResponse> Handle(
        GetQuizStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        var filter = UserStatisticsQueryParameterResolver.ResolveQuizStatisticsFilter(
            request);

        var model = await _userStatisticsRepository.GetQuizStatisticsAsync(
            keycloakUserId,
            filter,
            cancellationToken);

        return UserStatisticsMapper.ToQuizStatisticsResponse(
            filter,
            model);
    }
}