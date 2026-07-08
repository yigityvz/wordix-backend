namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Kaikki/Wiktionary meaning enrichment endpoint response modelidir.
/// 
/// Bu response hem parser sonucunu hem de database enrichment sonucunu birlikte raporlar.
/// </summary>
public sealed record EnrichKaikkiMeaningsResponse
{
    /// <summary>
    /// Bu enrichment işlemini takip eden ImportJob id değeridir.
    /// </summary>
    public Guid? ImportJobId { get; init; }


    /// <summary>
    /// Parser genel olarak başarılı çalıştı mı?
    /// </summary>
    public bool ProviderSucceeded { get; init; }

    /// <summary>
    /// Kaikki parser tarafından üretilen toplam meaning row sayısıdır.
    /// </summary>
    public int ProviderParsedRows { get; init; }

    /// <summary>
    /// Parser tarafında oluşan hata/uyarı sayısıdır.
    /// </summary>
    public int ProviderErrorCount { get; init; }

    /// <summary>
    /// Enrichment işlemi dryRun olarak mı çalıştı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Enrichment service'e gönderilen toplam meaning row sayısıdır.
    /// </summary>
    public int TotalInputRows { get; init; }

    /// <summary>
    /// DB'deki Word/LearningItem ile eşleşen meaning row sayısıdır.
    /// </summary>
    public int MatchedWordCount { get; init; }

    /// <summary>
    /// DB'de karşılığı bulunamayan meaning row sayısıdır.
    /// </summary>
    public int NotMatchedCount { get; init; }

    /// <summary>
    /// Phrase candidate olduğu için bu fazda atlanan satır sayısıdır.
    /// </summary>
    public int SkippedPhraseCandidateCount { get; init; }

    /// <summary>
    /// Aynı meaning zaten var olduğu için atlanan satır sayısıdır.
    /// </summary>
    public int SkippedExistingMeaningCount { get; init; }

    /// <summary>
    /// Aynı input içinde duplicate olduğu için atlanan satır sayısıdır.
    /// </summary>
    public int SkippedDuplicateInputCount { get; init; }

    /// <summary>
    /// DryRun aktifken eklenecek meaning sayısıdır.
    /// </summary>
    public int WouldCreateCount { get; init; }

    /// <summary>
    /// Gerçekten database'e eklenen meaning sayısıdır.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Hatalı/geçersiz row sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// Parser ve enrichment mesajlarının sınırlı listesidir.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; }
        = Array.Empty<string>();
}