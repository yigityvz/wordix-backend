namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence enrichment service sonucunu temsil eder.
/// 
/// Bu result hem dry-run hem gerçek insert modunda kullanılabilir.
/// </summary>
public sealed record ExampleSentenceEnrichmentResult
{
    /// <summary>
    /// Enrichment genel olarak başarılı çalıştı mı?
    /// 
    /// Örneğin language bulunamazsa false olabilir.
    /// Bazı satırların eşleşmemesi ise genel başarısızlık değildir.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// DryRun modunda mı çalışıldı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Parser'dan gelen toplam row sayısı.
    /// </summary>
    public int TotalInputRows { get; init; }

    /// <summary>
    /// Repository'den bulunan toplam Word/Phrase candidate sayısı.
    /// </summary>
    public int CandidateLearningItemCount { get; init; }

    /// <summary>
    /// En az bir Word/Phrase ile eşleşen Tatoeba row sayısı.
    /// </summary>
    public int MatchedRowCount { get; init; }

    /// <summary>
    /// Hiçbir Word/Phrase ile eşleşmeyen Tatoeba row sayısı.
    /// </summary>
    public int NotMatchedRowCount { get; init; }

    /// <summary>
    /// Duplicate veya limit nedeniyle atlanan eşleşme sayısı.
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// DryRun true ise oluşturulabilecek example link sayısıdır.
    /// DryRun false ise gerçek oluşturulan example link sayısıyla aynı olabilir.
    /// </summary>
    public int WouldCreateCount { get; init; }

    /// <summary>
    /// Gerçek oluşturulan Sentence sayısıdır.
    /// DryRun true ise 0 olur.
    /// </summary>
    public int CreatedSentenceCount { get; init; }

    /// <summary>
    /// Gerçek oluşturulan SentenceTranslation sayısıdır.
    /// DryRun true ise 0 olur.
    /// </summary>
    public int CreatedSentenceTranslationCount { get; init; }

    /// <summary>
    /// Gerçek oluşturulan LearningItemExampleSentence bağlantı sayısıdır.
    /// DryRun true ise 0 olur.
    /// </summary>
    public int CreatedExampleLinkCount { get; init; }

    /// <summary>
    /// Enrichment sonucunda oluşturulan veya oluşturulması planlanan örnek item'lardır.
    /// Response sample için kullanılabilir.
    /// </summary>
    public IReadOnlyCollection<ExampleSentenceEnrichmentCreatedItem> SampleItems { get; init; }
        = Array.Empty<ExampleSentenceEnrichmentCreatedItem>();

    /// <summary>
    /// Enrichment sırasında oluşan bilgi/uyarı/hata mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<ExampleSentenceEnrichmentMessage> Messages { get; init; }
        = Array.Empty<ExampleSentenceEnrichmentMessage>();

    /// <summary>
    /// Başarılı result üretmek için helper.
    /// </summary>
    public static ExampleSentenceEnrichmentResult Success(
        bool dryRun,
        int totalInputRows,
        int candidateLearningItemCount,
        int matchedRowCount,
        int notMatchedRowCount,
        int skippedCount,
        int wouldCreateCount,
        int createdSentenceCount,
        int createdSentenceTranslationCount,
        int createdExampleLinkCount,
        IReadOnlyCollection<ExampleSentenceEnrichmentCreatedItem> sampleItems,
        IReadOnlyCollection<ExampleSentenceEnrichmentMessage> messages)
    {
        return new ExampleSentenceEnrichmentResult
        {
            Succeeded = true,
            DryRun = dryRun,
            TotalInputRows = totalInputRows,
            CandidateLearningItemCount = candidateLearningItemCount,
            MatchedRowCount = matchedRowCount,
            NotMatchedRowCount = notMatchedRowCount,
            SkippedCount = skippedCount,
            WouldCreateCount = wouldCreateCount,
            CreatedSentenceCount = createdSentenceCount,
            CreatedSentenceTranslationCount = createdSentenceTranslationCount,
            CreatedExampleLinkCount = createdExampleLinkCount,
            SampleItems = sampleItems,
            Messages = messages
        };
    }

    /// <summary>
    /// Başarısız result üretmek için helper.
    /// </summary>
    public static ExampleSentenceEnrichmentResult Failure(
        bool dryRun,
        int totalInputRows,
        IReadOnlyCollection<ExampleSentenceEnrichmentMessage> messages)
    {
        return new ExampleSentenceEnrichmentResult
        {
            Succeeded = false,
            DryRun = dryRun,
            TotalInputRows = totalInputRows,
            Messages = messages
        };
    }
}