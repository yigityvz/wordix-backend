using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Quiz statistics repository sorgusunda kullanılacak filtre modelidir.
/// 
/// Bu model API request DTO değildir.
/// Request DTO'daki string query parametreleri mapper/handler tarafında enum değerlere çevrilir.
/// Repository bu güçlü tipli filtreyi kullanır.
/// </summary>
public sealed class QuizStatisticsFilter
{
    public UserStatisticsDateRange DateRange { get; init; } = new();

    public QuizType? QuizType { get; init; }

    public QuizSourceType? QuizSourceType { get; init; }

    public QuizContentMode? QuizContentMode { get; init; }

    public DifficultyGroup? DifficultyGroup { get; init; }
}