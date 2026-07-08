using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// ImportJob entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu tablo import/enrichment süreçlerinin operasyonel takibini yapar.
/// Örneğin:
/// - CEFR word import kaç satır işledi?
/// - Tatoeba enrichment kaç example link oluşturdu?
/// - Job dry-run mıydı?
/// - Job hata aldı mı?
/// </summary>
public sealed class ImportJobConfiguration : IEntityTypeConfiguration<ImportJob>
{
    public void Configure(EntityTypeBuilder<ImportJob> builder)
    {
        builder.ToTable("ImportJobs");

        builder.ConfigureAuditableEntity();

        builder.Property(job => job.JobType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(job => job.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(job => job.SourceName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(job => job.SourceVersion)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(job => job.SourceFileName)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(job => job.TriggeredByKeycloakUserId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(job => job.DryRun)
            .IsRequired();

        builder.Property(job => job.StartedAt)
            .IsRequired(false);

        builder.Property(job => job.FinishedAt)
            .IsRequired(false);

        builder.Property(job => job.TotalRows)
            .IsRequired();

        builder.Property(job => job.ProcessedRows)
            .IsRequired();

        builder.Property(job => job.CreatedCount)
            .IsRequired();

        builder.Property(job => job.UpdatedCount)
            .IsRequired();

        builder.Property(job => job.SkippedCount)
            .IsRequired();

        builder.Property(job => job.FailedCount)
            .IsRequired();

        builder.Property(job => job.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(job => job.SummaryMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Admin panelde job türüne göre filtreleme için.
        builder.HasIndex(job => job.JobType);

        // Running/Pending/Failed jobları hızlı bulmak için.
        builder.HasIndex(job => job.Status);

        // Kaynak bazlı import geçmişini görmek için.
        builder.HasIndex(job => job.SourceName);

        // DryRun ve gerçek insert joblarını ayırmak için.
        builder.HasIndex(job => job.DryRun);

        // Job geçmişini tarihe göre listelemek için.
        builder.HasIndex(job => job.CreatedAt);

        // Belirli bir kullanıcının başlattığı importları görmek için.
        builder.HasIndex(job => job.TriggeredByKeycloakUserId);

        // Tip + durum + tarih kombinasyonu admin/diagnostic ekranlarında sık kullanılabilir.
        builder.HasIndex(job => new
        {
            job.JobType,
            job.Status,
            job.CreatedAt
        });
    }
}