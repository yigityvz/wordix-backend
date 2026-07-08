using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// ProviderRequestLog entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu tablo dış provider çağrılarını takip eder.
/// Örneğin:
/// - Azure Translator çağrısı başarılı mı?
/// - Çağrı kaç ms sürdü?
/// - Cache'den mi döndü?
/// - Hangi ImportJob ile ilişkili?
/// - Hangi kullanıcı provider çağrısı tetikledi?
/// </summary>
public sealed class ProviderRequestLogConfiguration
    : IEntityTypeConfiguration<ProviderRequestLog>
{
    public void Configure(EntityTypeBuilder<ProviderRequestLog> builder)
    {
        builder.ToTable("ProviderRequestLogs");

        builder.ConfigureAuditableEntity();

        builder.Property(log => log.ImportJobId)
            .IsRequired(false);

        builder.Property(log => log.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(log => log.LearningItemId)
            .IsRequired(false);

        builder.Property(log => log.ProviderType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(log => log.ProviderName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(log => log.OperationName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(log => log.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(log => log.RequestKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(log => log.NormalizedInput)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(log => log.SourceLanguageCode)
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(log => log.TargetLanguageCode)
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(log => log.WasServedFromCache)
            .IsRequired();

        builder.Property(log => log.DurationMs)
            .IsRequired(false);

        builder.Property(log => log.HttpStatusCode)
            .IsRequired(false);

        builder.Property(log => log.ErrorCode)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(log => log.ErrorMessage)
            .HasMaxLength(1000)
            .IsRequired(false);

        // Bir import job içinde oluşan provider çağrılarını görmek için.
        builder.HasIndex(log => log.ImportJobId);

        // Kullanıcı bazlı provider kullanım analizi için.
        builder.HasIndex(log => log.KeycloakUserId);

        // Provider adına göre analiz:
        // AzureTranslator kaç kez çağrılmış?
        builder.HasIndex(log => log.ProviderName);

        // Provider türüne göre analiz:
        // Translation / Dictionary / Import çağrıları.
        builder.HasIndex(log => log.ProviderType);

        // Başarılı/hatalı/cache'den dönen çağrıları filtrelemek için.
        builder.HasIndex(log => log.Status);

        // Aynı request key üzerinden logları bulmak için.
        builder.HasIndex(log => log.RequestKey);

        // Cache kullanım analizi için.
        builder.HasIndex(log => log.WasServedFromCache);

        // HTTP provider hata analizleri için.
        builder.HasIndex(log => log.HttpStatusCode);

        // Zaman bazlı provider kullanım raporları için.
        builder.HasIndex(log => log.CreatedAt);

        // Provider + operation + status kombinasyonu operasyon dashboard için faydalıdır.
        builder.HasIndex(log => new
        {
            log.ProviderName,
            log.OperationName,
            log.Status,
            log.CreatedAt
        });

        // ProviderRequestLog bir ImportJob ile ilişkili olabilir.
        // ImportJob silinirse logların otomatik silinmesini istemiyoruz.
        // Bu kayıtlar operasyonel audit/veri izleme için kalmalı.
        builder.HasOne<ImportJob>()
            .WithMany()
            .HasForeignKey(log => log.ImportJobId)
            .OnDelete(DeleteBehavior.NoAction);

        // Provider çağrısı bir LearningItem ile ilişkili olabilir.
        // LearningItem fiziksel silinirse provider logların silinmesini istemiyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(log => log.LearningItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}