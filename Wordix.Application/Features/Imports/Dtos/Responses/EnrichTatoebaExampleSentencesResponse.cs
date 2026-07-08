namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Tatoeba example sentence enrichment sonucudur.
/// 
/// Bu response hem provider parse sonucunu hem de enrichment sonucunu raporlar.
/// </summary>
public sealed record EnrichTatoebaExampleSentencesResponse
{

    /// <summary>
    /// Bu enrichment işlemini takip eden ImportJob id değeridir.
    /// </summary>
    public Guid? ImportJobId { get; init; }

    /// <summary>
    /// Tatoeba parser provider genel olarak başarılı çalıştı mı?
    /// </summary>
    public bool ProviderSucceeded { get; init; }

    /// <summary>
    /// Provider tarafından parse edilen toplam row sayısıdır.
    /// </summary>
    public int ProviderParsedRows { get; init; }

    /// <summary>
    /// Provider'ın ürettiği hata/uyarı mesajı sayısıdır.
    /// </summary>
    public int ProviderErrorCount { get; init; }

    /// <summary>
    /// Provider hata/uyarı mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<string> ProviderMessages { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Enrichment service genel olarak başarılı çalıştı mı?
    /// </summary>
    public bool EnrichmentSucceeded { get; init; }

    /// <summary>
    /// Enrichment dry-run modunda mı çalıştı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// Enrichment service'e gönderilen toplam row sayısıdır.
    /// </summary>
    public int TotalInputRows { get; init; }

    /// <summary>
    /// DB'de bulunan toplam Word/Phrase candidate sayısıdır.
    /// </summary>
    public int CandidateLearningItemCount { get; init; }

    /// <summary>
    /// En az bir Word/Phrase ile eşleşen Tatoeba row sayısıdır.
    /// </summary>
    public int MatchedRowCount { get; init; }

    /// <summary>
    /// Hiçbir Word/Phrase ile eşleşmeyen Tatoeba row sayısıdır.
    /// </summary>
    public int NotMatchedRowCount { get; init; }

    /// <summary>
    /// Duplicate veya limit nedeniyle atlanan eşleşme sayısıdır.
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// DryRun true ise oluşturulabilecek example link sayısıdır.
    /// DryRun false ise oluşturulması denenen/oluşturulan link sayısını temsil eder.
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
    /// Enrichment sonucunda oluşturulan veya oluşturulması planlanan sample item'lardır.
    /// </summary>
    public IReadOnlyCollection<EnrichTatoebaExampleSentenceSampleResponse> SampleItems { get; init; }
        = Array.Empty<EnrichTatoebaExampleSentenceSampleResponse>();

    /// <summary>
    /// Enrichment sırasında oluşan bilgi/uyarı/hata mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<EnrichTatoebaExampleSentenceMessageResponse> EnrichmentMessages { get; init; }
        = Array.Empty<EnrichTatoebaExampleSentenceMessageResponse>();
}