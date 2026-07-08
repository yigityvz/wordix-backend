using MediatR;
using Wordix.Application.Features.UserStatistics.Dtos.Responses;

namespace Wordix.Application.Features.UserStatistics.Queries.GetUserLearningSummary;

/// <summary>
/// Current user'ın genel öğrenme özetini getiren MediatR query modelidir.
/// 
/// Bu query parametre almaz.
/// Çünkü kullanıcı id request'ten değil, handler içinde ICurrentUserService üzerinden token'dan alınır.
/// </summary>
public sealed class GetUserLearningSummaryQuery
    : IRequest<UserLearningSummaryResponse>
{
}