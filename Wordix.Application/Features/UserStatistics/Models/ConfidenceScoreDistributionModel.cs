namespace Wordix.Application.Features.UserStatistics.Models;

/// <summary>
/// Kullanıcının confidence score dağılımını repository'den handler'a taşıyan internal modeldir.
/// 
/// Bu model API response değildir.
/// Mapper içinde ConfidenceScoreDistributionResponse'a dönüştürülür.
/// </summary>
public sealed class ConfidenceScoreDistributionModel
{
    public int TotalItemCount { get; init; }

    public double AverageConfidenceScore { get; init; }

    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;

    public IReadOnlyCollection<ConfidenceScoreBucketModel> Buckets { get; init; }
        = Array.Empty<ConfidenceScoreBucketModel>();
}