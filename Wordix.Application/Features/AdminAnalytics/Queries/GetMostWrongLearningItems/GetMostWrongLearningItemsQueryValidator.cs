using FluentValidation;
using Wordix.Application.Common.Constants;

namespace Wordix.Application.Features.AdminAnalytics.Queries.GetMostWrongLearningItems;

/// <summary>
/// GetMostWrongLearningItemsQuery validation kurallarıdır.
/// </summary>
public sealed class GetMostWrongLearningItemsQueryValidator
    : AbstractValidator<GetMostWrongLearningItemsQuery>
{
    public GetMostWrongLearningItemsQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue ||
                           !query.ToUtc.HasValue ||
                           query.FromUtc.Value <= query.ToUtc.Value)
            .WithMessage("FromUtc, ToUtc değerinden büyük olamaz.");

        RuleFor(query => query.Limit)
            .InclusiveBetween(1, AdminAnalyticsConstants.MaxLimit)
            .When(query => query.Limit.HasValue)
            .WithMessage($"Limit 1 ile {AdminAnalyticsConstants.MaxLimit} arasında olmalıdır.");
    }
}