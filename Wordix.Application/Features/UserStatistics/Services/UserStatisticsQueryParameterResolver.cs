using Wordix.Application.Common.Constants;
using Wordix.Application.Features.UserStatistics.Models;
using Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;
using Wordix.Application.Features.UserStatistics.Queries.GetQuizStatistics;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Services;

/// <summary>
/// UserStatistics query parametrelerini repository'nin kullanacağı güçlü typed filtre modellerine dönüştürür.
/// 
/// Bu class neden var?
/// - Controller query string alır.
/// - Query class string/null değerler taşır.
/// - Repository ise enum, date range, page number gibi normalize edilmiş değerlerle çalışmalıdır.
/// - Bu dönüşümü handler içine gömersek handler gereksiz kalabalıklaşır.
/// 
/// Bu class Application katmanındadır.
/// DbContext bilmez.
/// HttpContext bilmez.
/// Sadece query parametrelerini normalize eder.
/// </summary>
public static class UserStatisticsQueryParameterResolver
{
    /// <summary>
    /// GetQuizStatisticsQuery değerlerini QuizStatisticsFilter modeline dönüştürür.
    /// 
    /// Tarih aralığı gönderilmezse son 30 gün varsayılır.
    /// </summary>
    public static QuizStatisticsFilter ResolveQuizStatisticsFilter(
        GetQuizStatisticsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var dateRange = ResolveDateRange(
            query.FromUtc,
            query.ToUtc);

        return new QuizStatisticsFilter
        {
            DateRange = dateRange,
            QuizType = ParseNullableEnum<QuizType>(query.QuizType),
            QuizSourceType = ParseNullableEnum<QuizSourceType>(query.QuizSourceType),
            QuizContentMode = ParseNullableEnum<QuizContentMode>(query.QuizContentMode),
            DifficultyGroup = ParseNullableEnum<DifficultyGroup>(query.DifficultyGroup)
        };
    }

    /// <summary>
    /// GetDifficultItemsQuery değerlerini DifficultItemsFilter modeline dönüştürür.
    /// 
    /// Null gelen pagination değerleri güvenli defaultlarla doldurulur.
    /// </summary>
    public static DifficultItemsFilter ResolveDifficultItemsFilter(
        GetDifficultItemsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return new DifficultItemsFilter
        {
            PageNumber = query.PageNumber ?? UserStatisticsConstants.DefaultPageNumber,
            PageSize = query.PageSize ?? UserStatisticsConstants.DefaultPageSize,
            Source = ResolveDifficultItemSource(query.Source),
            SortBy = ResolveDifficultItemSortBy(query.SortBy),
            ItemType = ParseNullableEnum<LearningItemType>(query.ItemType),
            LearningStatus = ParseNullableEnum<LearningStatus>(query.LearningStatus)
        };
    }

    /// <summary>
    /// Quiz statistics için tarih aralığını normalize eder.
    /// 
    /// Kurallar:
    /// - ToUtc null ise DateTime.UtcNow kullanılır.
    /// - FromUtc null ise ToUtc - 30 gün kullanılır.
    /// - DateTimeKind UTC değilse UTC kabul edilerek işaretlenir.
    /// </summary>
    private static UserStatisticsDateRange ResolveDateRange(
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        var resolvedToUtc = NormalizeUtc(
            toUtc ?? DateTime.UtcNow);

        var resolvedFromUtc = NormalizeUtc(
            fromUtc ?? resolvedToUtc.AddDays(-UserStatisticsConstants.DefaultDateRangeDays));

        return new UserStatisticsDateRange
        {
            FromUtc = resolvedFromUtc,
            ToUtc = resolvedToUtc
        };
    }

    /// <summary>
    /// String enum değerini nullable enum değerine çevirir.
    /// 
    /// Validator bu değerin geçerli olup olmadığını önceden kontrol eder.
    /// Burada yine de güvenli şekilde TryParse kullanıyoruz.
    /// </summary>
    private static TEnum? ParseNullableEnum<TEnum>(
        string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(
                value.Trim(),
                ignoreCase: true,
                out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"{typeof(TEnum).Name} için geçersiz değer: {value}");
    }

    /// <summary>
    /// Difficult item source değerini sabitlerdeki standart karşılığına normalize eder.
    /// 
    /// Neden?
    /// - Validator case-insensitive kabul eder.
    /// - Repository switch-case sabit string değerleriyle çalışır.
    /// - Bu yüzden burada exact constant değerine çeviriyoruz.
    /// </summary>
    private static string ResolveDifficultItemSource(
        string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return UserStatisticsConstants.DifficultItemSources.Both;
        }

        if (source.Equals(
                UserStatisticsConstants.DifficultItemSources.Manual,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSources.Manual;
        }

        if (source.Equals(
                UserStatisticsConstants.DifficultItemSources.Progress,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSources.Progress;
        }

        return UserStatisticsConstants.DifficultItemSources.Both;
    }

    /// <summary>
    /// Difficult item sort değerini sabitlerdeki standart karşılığına normalize eder.
    /// </summary>
    private static string ResolveDifficultItemSortBy(
        string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return UserStatisticsConstants.DifficultItemSortBy.ConfidenceAsc;
        }

        if (sortBy.Equals(
                UserStatisticsConstants.DifficultItemSortBy.WrongCountDesc,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSortBy.WrongCountDesc;
        }

        if (sortBy.Equals(
                UserStatisticsConstants.DifficultItemSortBy.ConsecutiveWrongDesc,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSortBy.ConsecutiveWrongDesc;
        }

        if (sortBy.Equals(
                UserStatisticsConstants.DifficultItemSortBy.NextReviewAsc,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSortBy.NextReviewAsc;
        }

        if (sortBy.Equals(
                UserStatisticsConstants.DifficultItemSortBy.SavedAtDesc,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserStatisticsConstants.DifficultItemSortBy.SavedAtDesc;
        }

        return UserStatisticsConstants.DifficultItemSortBy.ConfidenceAsc;
    }

    /// <summary>
    /// DateTime değerini UTC kabul ederek normalize eder.
    /// 
    /// SQL tarafında tarihleri UTC tuttuğumuz için filtreleri de UTC kabul ediyoruz.
    /// </summary>
    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}