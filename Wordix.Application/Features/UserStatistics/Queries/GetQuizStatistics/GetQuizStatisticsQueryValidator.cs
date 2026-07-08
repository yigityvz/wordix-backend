using FluentValidation;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Queries.GetQuizStatistics;

/// <summary>
/// GetQuizStatisticsQuery validation kurallarıdır.
/// 
/// Controller tarih veya enum kontrolü yapmaz.
/// Query MediatR pipeline'a girdiğinde FluentValidation çalışır.
/// </summary>
public sealed class GetQuizStatisticsQueryValidator
    : AbstractValidator<GetQuizStatisticsQuery>
{
    public GetQuizStatisticsQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue ||
                           !query.ToUtc.HasValue ||
                           query.FromUtc.Value <= query.ToUtc.Value)
            .WithMessage("FromUtc, ToUtc değerinden büyük olamaz.");

        RuleFor(query => query.QuizType)
            .Must(value => BeValidEnumValue<QuizType>(value))
            .WithMessage("QuizType geçerli bir değer olmalıdır.");

        RuleFor(query => query.QuizSourceType)
            .Must(value => BeValidEnumValue<QuizSourceType>(value))
            .WithMessage("QuizSourceType geçerli bir değer olmalıdır.");

        RuleFor(query => query.QuizContentMode)
            .Must(value => BeValidEnumValue<QuizContentMode>(value))
            .WithMessage("QuizContentMode geçerli bir değer olmalıdır.");

        RuleFor(query => query.DifficultyGroup)
            .Must(value => BeValidEnumValue<DifficultyGroup>(value))
            .WithMessage("DifficultyGroup geçerli bir değer olmalıdır.");
    }

    /// <summary>
    /// Query string'den gelen string değerin ilgili enum'a çevrilip çevrilemeyeceğini kontrol eder.
    /// 
    /// Null veya boş değer geçerlidir.
    /// Çünkü filtre gönderilmemiş olabilir.
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