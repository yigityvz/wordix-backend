namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Eksik Word meaninglerini Azure Translator ile doldurma işleminin response modelidir.
/// </summary>
public sealed record AzureMissingMeaningBackfillResponse
{
    /// <summary>
    /// Bu backfill işlemini takip eden ImportJob id değeridir.
    /// </summary>
    public Guid? ImportJobId { get; init; }

    /// <summary>
    /// İşlem dryRun olarak mı çalıştı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// DB'de bulunan toplam eksik meaning sahibi Word sayısıdır.
    /// </summary>
    public int MissingWordCount { get; init; }

    /// <summary>
    /// Bu çalıştırmada işleme alınan Word sayısıdır.
    /// MaxItems ile sınırlanır.
    /// </summary>
    public int ProcessedCount { get; init; }

    /// <summary>
    /// DryRun true iken oluşturulabilecek meaning sayısıdır.
    /// </summary>
    public int WouldCreateCount { get; init; }

    /// <summary>
    /// Gerçekten oluşturulan meaning sayısıdır.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Azure başarısız olduğu için veya geçersiz sonuç döndüğü için atlanan kayıt sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// Translation sonucu kolon limitini aştığı için atlanan kayıt sayısıdır.
    /// </summary>
    public int SkippedLongTranslationCount { get; init; }

    /// <summary>
    /// İşlem mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; } = Array.Empty<string>();
}