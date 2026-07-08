namespace Wordix.Application.Features.UserStatistics.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary itemları üzerindeki confidence score dağılımını temsil eder.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/confidence-distribution
/// 
/// Bu response frontend'de grafik/chart çizmek için uygundur.
/// </summary>
public sealed class ConfidenceScoreDistributionResponse
{
    public int TotalItemCount { get; init; }

    public double AverageConfidenceScore { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }

    public IReadOnlyCollection<ConfidenceScoreBucketResponse> Buckets { get; init; }
        = Array.Empty<ConfidenceScoreBucketResponse>();
}

/// <summary>
/// Confidence score dağılımındaki tek bir bucket satırıdır.
/// </summary>
public sealed class ConfidenceScoreBucketResponse
{
    public string Label { get; init; } = string.Empty;

    public int MinScore { get; init; }

    public int MaxScore { get; init; }

    public int ItemCount { get; init; }

    /// <summary>
    /// Bu bucket'ın toplam aktif itemlar içindeki yüzdesidir.
    /// 
    /// Örnek:
    /// 25.50
    /// </summary>
    public double Percentage { get; init; }
}