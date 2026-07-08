namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Confidence score dağılımındaki tek bir bucket için internal modeldir.
/// 
/// Bu model API response değildir.
/// Mapper içinde ConfidenceScoreBucketResponse'a dönüştürülür.
/// </summary>
public sealed class ConfidenceScoreBucketModel
{
    public string Label { get; init; } = string.Empty;

    public int MinScore { get; init; }

    public int MaxScore { get; init; }

    public int ItemCount { get; init; }

    public double Percentage { get; init; }
}