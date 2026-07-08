namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Meaning enrichment işleminin sonucunu temsil eder.
/// 
/// Bu result modeli database'e kaç kayıt eklendiğini,
/// kaç kaydın atlandığını ve kaç eşleşme bulunamadığını raporlar.
/// </summary>
public sealed record MeaningEnrichmentResult
{
    /// <summary>
    /// İşlem dryRun olarak mı çalıştı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Enrichment service'e gelen toplam meaning row sayısıdır.
    /// </summary>
    public int TotalInputRows { get; init; }

    /// <summary>
    /// DB'deki Word/LearningItem ile eşleşen meaning row sayısıdır.
    /// </summary>
    public int MatchedWordCount { get; init; }

    /// <summary>
    /// DB'de karşılığı bulunamayan meaning row sayısıdır.
    /// 
    /// Örnek:
    /// Kaikki'de var ama bizim CEFR-J/Octanove imported word havuzumuzda yok.
    /// </summary>
    public int NotMatchedCount { get; init; }

    /// <summary>
    /// Phrase candidate olduğu için bu fazda atlanan satır sayısıdır.
    /// 
    /// Çünkü phrase import ayrı fazda yapılacak.
    /// </summary>
    public int SkippedPhraseCandidateCount { get; init; }

    /// <summary>
    /// Aynı meaning zaten var olduğu için atlanan satır sayısıdır.
    /// </summary>
    public int SkippedExistingMeaningCount { get; init; }

    /// <summary>
    /// Aynı input içinde tekrar ettiği için atlanan satır sayısıdır.
    /// </summary>
    public int SkippedDuplicateInputCount { get; init; }

    /// <summary>
    /// DryRun aktifken eklenecek meaning sayısıdır.
    /// 
    /// DryRun = true ise CreatedCount 0 olur,
    /// WouldCreateCount ise potansiyel insert sayısını gösterir.
    /// </summary>
    public int WouldCreateCount { get; init; }

    /// <summary>
    /// Gerçekten database'e eklenen Meaning sayısıdır.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Hatalı veya geçersiz meaning row sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// İşlem sırasında oluşan sınırlı mesaj listesidir.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; }
        = Array.Empty<string>();
}