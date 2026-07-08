using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Import/enrichment işlemlerinin takip edildiği ana job kaydını temsil eder.
/// 
/// Bu entity neden gerekli?
/// - Şu anda import işlemlerini Swagger üzerinden manuel çalıştırıyoruz.
/// - Bir importun ne zaman başladığını, kaç satır işlediğini,
///   kaç kayıt oluşturduğunu ve hata alıp almadığını takip etmek istiyoruz.
/// - Production ortamında importlar genelde background job, admin panel veya deployment task ile çalıştırılır.
/// - Bu entity ileride ImportJob ekranı, retry mekanizması ve detaylı import raporları için temel oluşturur.
/// 
/// Örnek job'lar:
/// - CEFR-J word import
/// - Octanove word import
/// - Kaikki meaning enrichment
/// - Tatoeba example sentence enrichment
/// - Runtime provider lookup tracking
/// </summary>
public class ImportJob : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// Domain tarafında boş ImportJob oluşturulmasını istemiyoruz.
    /// EF Core ise database'den okurken bu constructor'ı kullanabilir.
    /// </summary>
    protected ImportJob()
    {
    }

    /// <summary>
    /// Yeni import job kaydı oluşturur.
    /// 
    /// triggeredByKeycloakUserId neden nullable?
    /// - Admin endpointinden manuel çalıştırılırsa token içindeki KeycloakUserId tutulabilir.
    /// - Background worker veya sistem job'ı çalıştırırsa kullanıcı olmayabilir.
    /// 
    /// dryRun neden burada tutuluyor?
    /// - Aynı import işlemi dry-run veya gerçek insert olarak çalışabilir.
    /// - Raporlama sırasında hangi job'ın gerçekten DB'ye yazdığını bilmek gerekir.
    /// </summary>
    public ImportJob(
        ImportJobType jobType,
        string sourceName,
        string? sourceVersion = null,
        string? sourceFileName = null,
        bool dryRun = true,
        string? triggeredByKeycloakUserId = null)
    {
        if (!Enum.IsDefined(jobType) || jobType == ImportJobType.Unknown)
        {
            throw new ArgumentException("Geçerli bir ImportJobType değeri verilmelidir.", nameof(jobType));
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new ArgumentException("SourceName boş olamaz.", nameof(sourceName));
        }

        JobType = jobType;
        Status = ImportJobStatus.Pending;

        SourceName = sourceName.Trim();

        SourceVersion = string.IsNullOrWhiteSpace(sourceVersion)
            ? null
            : sourceVersion.Trim();

        SourceFileName = string.IsNullOrWhiteSpace(sourceFileName)
            ? null
            : sourceFileName.Trim();

        DryRun = dryRun;

        TriggeredByKeycloakUserId = string.IsNullOrWhiteSpace(triggeredByKeycloakUserId)
            ? null
            : triggeredByKeycloakUserId.Trim();
    }

    /// <summary>
    /// Job türüdür.
    /// 
    /// Örnek:
    /// WordListImport, MeaningEnrichment, ExampleSentenceEnrichment.
    /// </summary>
    public ImportJobType JobType { get; private set; }

    /// <summary>
    /// Job'ın güncel çalışma durumudur.
    /// 
    /// Örnek:
    /// Pending, Running, Completed, Failed.
    /// </summary>
    public ImportJobStatus Status { get; private set; }

    /// <summary>
    /// Import/enrichment kaynağının adıdır.
    /// 
    /// Örnek:
    /// CEFR-J, Octanove, WiktionaryKaikki, Tatoeba, AzureTranslator.
    /// </summary>
    public string SourceName { get; private set; } = string.Empty;

    /// <summary>
    /// Kaynak versiyon bilgisidir.
    /// 
    /// Örnek:
    /// CEFR-J için 1.5,
    /// Octanove için 1.0.
    /// 
    /// Her kaynakta versiyon olmayabileceği için nullable tutulur.
    /// </summary>
    public string? SourceVersion { get; private set; }

    /// <summary>
    /// Import sırasında kullanılan dosya adı veya ana kaynak dosya adıdır.
    /// 
    /// Örnek:
    /// cefrj_words.csv,
    /// links.csv,
    /// eng_sentences.tsv.
    /// 
    /// Bir job birden fazla dosya kullanıyorsa burada ana dosya veya özet isim tutulabilir.
    /// Detaylı metadata ileride ayrı JSON/metadata alanına taşınabilir.
    /// </summary>
    public string? SourceFileName { get; private set; }

    /// <summary>
    /// Job'ı tetikleyen Keycloak kullanıcısının id değeridir.
    /// 
    /// Null olabilir.
    /// Çünkü bazı importlar sistem/background worker tarafından tetiklenebilir.
    /// </summary>
    public string? TriggeredByKeycloakUserId { get; private set; }

    /// <summary>
    /// Job dry-run modunda mı çalıştı?
    /// 
    /// Dry-run modunda DB'ye gerçek içerik insert edilmez,
    /// sadece kaç kayıt oluşturulabileceği raporlanır.
    /// </summary>
    public bool DryRun { get; private set; }

    /// <summary>
    /// Job'ın başladığı UTC zamandır.
    /// 
    /// Pending durumda null olabilir.
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// Job'ın tamamlandığı, hata aldığı veya iptal edildiği UTC zamandır.
    /// 
    /// Job hâlâ çalışıyorsa null kalır.
    /// </summary>
    public DateTime? FinishedAt { get; private set; }

    /// <summary>
    /// Provider/parser tarafından görülen toplam input satır sayısıdır.
    /// 
    /// Örnek:
    /// Tatoeba provider 100 row parse ettiyse TotalRows = 100 olabilir.
    /// </summary>
    public int TotalRows { get; private set; }

    /// <summary>
    /// Başarıyla parse edilen/işlenen satır sayısıdır.
    /// </summary>
    public int ProcessedRows { get; private set; }

    /// <summary>
    /// Oluşturulan kayıt sayısıdır.
    /// 
    /// Örnek:
    /// CEFR importta oluşturulan Word sayısı,
    /// Tatoeba enrichment'te oluşturulan example link sayısı.
    /// </summary>
    public int CreatedCount { get; private set; }

    /// <summary>
    /// Güncellenen kayıt sayısıdır.
    /// 
    /// İlk fazlarda 0 kalabilir.
    /// İleride enrichment/review akışlarında kullanılabilir.
    /// </summary>
    public int UpdatedCount { get; private set; }

    /// <summary>
    /// Duplicate, limit veya filtre nedeniyle atlanan kayıt sayısıdır.
    /// </summary>
    public int SkippedCount { get; private set; }

    /// <summary>
    /// Hata alan kayıt sayısıdır.
    /// </summary>
    public int FailedCount { get; private set; }

    /// <summary>
    /// Job başarısız olduysa hata mesajı burada tutulur.
    /// 
    /// Not:
    /// Çok uzun stack trace yerine kullanıcı/admin tarafından anlaşılabilir kısa hata mesajı tutulmalıdır.
    /// Detaylı teknik log ileride ayrı log sisteminde tutulabilir.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Job hakkında kısa özet veya not bilgisidir.
    /// 
    /// Örnek:
    /// "MaxCreatedItems limit reached."
    /// "Dry-run completed successfully."
    /// </summary>
    public string? SummaryMessage { get; private set; }

    /// <summary>
    /// Job'ı Running durumuna geçirir.
    /// </summary>
    public void MarkAsRunning(DateTime? startedAtUtc = null)
    {
        if (Status == ImportJobStatus.Running)
        {
            return;
        }

        if (Status is ImportJobStatus.Completed or ImportJobStatus.Failed or ImportJobStatus.Cancelled)
        {
            throw new InvalidOperationException("Tamamlanmış, hata almış veya iptal edilmiş job tekrar Running yapılamaz.");
        }

        Status = ImportJobStatus.Running;
        StartedAt = startedAtUtc ?? DateTime.UtcNow;

        MarkAsUpdated();
    }

    /// <summary>
    /// Job ilerleme sayaçlarını günceller.
    /// 
    /// Bu method job çalışırken ara ara çağrılabilir.
    /// Negatif değer kabul edilmez.
    /// </summary>
    public void UpdateProgress(
        int totalRows,
        int processedRows,
        int createdCount,
        int updatedCount,
        int skippedCount,
        int failedCount)
    {
        if (totalRows < 0)
        {
            throw new ArgumentException("TotalRows negatif olamaz.", nameof(totalRows));
        }

        if (processedRows < 0)
        {
            throw new ArgumentException("ProcessedRows negatif olamaz.", nameof(processedRows));
        }

        if (createdCount < 0)
        {
            throw new ArgumentException("CreatedCount negatif olamaz.", nameof(createdCount));
        }

        if (updatedCount < 0)
        {
            throw new ArgumentException("UpdatedCount negatif olamaz.", nameof(updatedCount));
        }

        if (skippedCount < 0)
        {
            throw new ArgumentException("SkippedCount negatif olamaz.", nameof(skippedCount));
        }

        if (failedCount < 0)
        {
            throw new ArgumentException("FailedCount negatif olamaz.", nameof(failedCount));
        }

        TotalRows = totalRows;
        ProcessedRows = processedRows;
        CreatedCount = createdCount;
        UpdatedCount = updatedCount;
        SkippedCount = skippedCount;
        FailedCount = failedCount;

        MarkAsUpdated();
    }

    /// <summary>
    /// Job'ı başarıyla tamamlandı olarak işaretler.
    /// 
    /// Complete sırasında sayaçları da son değerleriyle güncelliyoruz.
    /// </summary>
    public void MarkAsCompleted(
        int totalRows,
        int processedRows,
        int createdCount,
        int updatedCount,
        int skippedCount,
        int failedCount,
        string? summaryMessage = null,
        DateTime? finishedAtUtc = null)
    {
        if (Status is ImportJobStatus.Failed or ImportJobStatus.Cancelled)
        {
            throw new InvalidOperationException("Hata almış veya iptal edilmiş job Completed yapılamaz.");
        }

        UpdateProgress(
            totalRows,
            processedRows,
            createdCount,
            updatedCount,
            skippedCount,
            failedCount);

        Status = ImportJobStatus.Completed;
        FinishedAt = finishedAtUtc ?? DateTime.UtcNow;
        ErrorMessage = null;

        SummaryMessage = string.IsNullOrWhiteSpace(summaryMessage)
            ? null
            : summaryMessage.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Job'ı hata aldı olarak işaretler.
    /// </summary>
    public void MarkAsFailed(
        string errorMessage,
        int totalRows = 0,
        int processedRows = 0,
        int createdCount = 0,
        int updatedCount = 0,
        int skippedCount = 0,
        int failedCount = 0,
        DateTime? finishedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("ErrorMessage boş olamaz.", nameof(errorMessage));
        }

        UpdateProgress(
            totalRows,
            processedRows,
            createdCount,
            updatedCount,
            skippedCount,
            failedCount);

        Status = ImportJobStatus.Failed;
        FinishedAt = finishedAtUtc ?? DateTime.UtcNow;

        ErrorMessage = errorMessage.Trim();
        SummaryMessage = null;

        MarkAsUpdated();
    }

    /// <summary>
    /// Job'ı iptal edildi olarak işaretler.
    /// </summary>
    public void MarkAsCancelled(
        string? summaryMessage = null,
        DateTime? finishedAtUtc = null)
    {
        if (Status == ImportJobStatus.Completed)
        {
            throw new InvalidOperationException("Tamamlanmış job iptal edilemez.");
        }

        Status = ImportJobStatus.Cancelled;
        FinishedAt = finishedAtUtc ?? DateTime.UtcNow;

        SummaryMessage = string.IsNullOrWhiteSpace(summaryMessage)
            ? null
            : summaryMessage.Trim();

        MarkAsUpdated();
    }
}