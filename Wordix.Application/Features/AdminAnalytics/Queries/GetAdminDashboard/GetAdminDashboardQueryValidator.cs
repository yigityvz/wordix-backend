using FluentValidation;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetAdminDashboard;

/// <summary>
/// GetAdminDashboardQuery için validation kurallarını içerir.
/// 
/// Controller içinde tarih kontrolü yapmayız.
/// Query MediatR pipeline'a girdiğinde FluentValidation çalışır.
/// </summary>
public sealed class GetAdminDashboardQueryValidator
    : AbstractValidator<GetAdminDashboardQuery>
{
    public GetAdminDashboardQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue ||
                           !query.ToUtc.HasValue ||
                           query.FromUtc.Value <= query.ToUtc.Value)
            .WithMessage("FromUtc, ToUtc değerinden büyük olamaz.");
    }
}