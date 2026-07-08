namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// User statistics sorgularında kullanılacak normalize edilmiş tarih aralığı modelidir.
/// 
/// Bu model API DTO değildir.
/// Handler ve repository arasında tarih filtresini taşır.
/// 
/// API tarafında nullable DateTime gelir.
/// Handler tarafında bu model ile net FromUtc/ToUtc değerlerine dönüştürülür.
/// </summary>
public sealed class UserStatisticsDateRange
{
    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}