using FluentValidation;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetProviderStats;

/// <summary>
/// GetProviderStatsQuery validation kurallarıdır.
/// </summary>
public sealed class GetProviderStatsQueryValidator
    : AbstractValidator<GetProviderStatsQuery>
{
    public GetProviderStatsQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue ||
                           !query.ToUtc.HasValue ||
                           query.FromUtc.Value <= query.ToUtc.Value)
            .WithMessage("FromUtc, ToUtc değerinden büyük olamaz.");
    }
}