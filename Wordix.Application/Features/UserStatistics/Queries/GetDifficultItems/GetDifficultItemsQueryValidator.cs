using FluentValidation;
using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;

/// <summary>
/// GetDifficultItemsQuery validation kurallarıdır.
/// 
/// Bu endpoint liste döndürdüğü için pagination kontrolleri burada yapılır.
/// Controller içinde page/pageSize kontrolü yapılmaz.
/// </summary>
public sealed class GetDifficultItemsQueryValidator
    : AbstractValidator<GetDifficultItemsQuery>
{
    private static readonly string[] SupportedSources =
    {
        UserStatisticsConstants.DifficultItemSources.Both,
        UserStatisticsConstants.DifficultItemSources.Manual,
        UserStatisticsConstants.DifficultItemSources.Progress
    };

    private static readonly string[] SupportedSortValues =
    {
        UserStatisticsConstants.DifficultItemSortBy.ConfidenceAsc,
        UserStatisticsConstants.DifficultItemSortBy.WrongCountDesc,
        UserStatisticsConstants.DifficultItemSortBy.ConsecutiveWrongDesc,
        UserStatisticsConstants.DifficultItemSortBy.NextReviewAsc,
        UserStatisticsConstants.DifficultItemSortBy.SavedAtDesc
    };

    public GetDifficultItemsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(query => query.PageNumber.HasValue)
            .WithMessage("PageNumber 1 veya daha büyük olmalıdır.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, UserStatisticsConstants.MaxPageSize)
            .When(query => query.PageSize.HasValue)
            .WithMessage($"PageSize 1 ile {UserStatisticsConstants.MaxPageSize} arasında olmalıdır.");

        RuleFor(query => query.Source)
            .Must(value => BeSupportedValue(value, SupportedSources))
            .WithMessage("Source geçerli bir değer olmalıdır. Desteklenen değerler: both, manual, progress.");

        RuleFor(query => query.SortBy)
            .Must(value => BeSupportedValue(value, SupportedSortValues))
            .WithMessage("SortBy geçerli bir değer olmalıdır.");

        RuleFor(query => query.ItemType)
            .Must(value => BeValidEnumValue<LearningItemType>(value))
            .WithMessage("ItemType geçerli bir değer olmalıdır.");

        RuleFor(query => query.LearningStatus)
            .Must(value => BeValidEnumValue<LearningStatus>(value))
            .WithMessage("LearningStatus geçerli bir değer olmalıdır.");
    }

    /// <summary>
    /// Null veya boş filtreyi geçerli kabul eder.
    /// Çünkü filtre gönderilmemiş olabilir.
    /// Doluysa desteklenen değerlerden biri olmalıdır.
    /// </summary>
    private static bool BeSupportedValue(
        string? value,
        IReadOnlyCollection<string> supportedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return supportedValues.Contains(
            value,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Query string'den gelen string değerin enum'a çevrilip çevrilemeyeceğini kontrol eder.
    /// </summary>
    private static bool BeValidEnumValue<TEnum>(
        string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Enum.TryParse<TEnum>(
            value,
            ignoreCase: true,
            out _);
    }
}