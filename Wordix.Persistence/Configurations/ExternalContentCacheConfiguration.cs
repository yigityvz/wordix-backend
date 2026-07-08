using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// ExternalContentCache entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu tablo dış provider sonuçlarını cache'lemek için kullanılır.
/// Örneğin:
/// - Azure Translator sonucu
/// - ileride başka provider response'ları
/// 
/// Cache sayesinde aynı input için tekrar tekrar provider çağrısı yapmayız.
/// </summary>
public sealed class ExternalContentCacheConfiguration
    : IEntityTypeConfiguration<ExternalContentCache>
{
    public void Configure(EntityTypeBuilder<ExternalContentCache> builder)
    {
        builder.ToTable("ExternalContentCaches");

        builder.ConfigureAuditableEntity();

        builder.Property(cache => cache.ProviderType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cache => cache.ProviderName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(cache => cache.OperationName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(cache => cache.CacheKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(cache => cache.NormalizedInput)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(cache => cache.SourceLanguageCode)
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(cache => cache.TargetLanguageCode)
            .HasMaxLength(10)
            .IsRequired(false);

        // Provider payload JSON olduğu için uzun olabilir.
        // İlk aşamada nvarchar(max) kullanıyoruz.
        // Daha sonra SQL Server JSON index / computed column gerekirse ayrıca ele alınabilir.
        builder.Property(cache => cache.CachedPayloadJson)
            .IsRequired();

        builder.Property(cache => cache.ContentSource)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cache => cache.QualityStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cache => cache.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(cache => cache.ExpiresAt)
            .IsRequired(false);

        builder.Property(cache => cache.LastAccessedAt)
            .IsRequired(false);

        builder.Property(cache => cache.HitCount)
            .IsRequired();

        // Hesaplanan domain property olduğu için database'e map edilmez.
        builder.Ignore(cache => cache.IsUsable);

        // Aynı cache key için tek cache kaydı olmalı.
        builder.HasIndex(cache => cache.CacheKey)
            .IsUnique();

        // Provider bazlı cache kayıtlarını analiz etmek için.
        builder.HasIndex(cache => cache.ProviderName);

        builder.HasIndex(cache => cache.ProviderType);

        // Aktif/expired/disabled cache kayıtlarını filtrelemek için.
        builder.HasIndex(cache => cache.Status);

        // Süresi dolan cache kayıtlarını bulmak için.
        builder.HasIndex(cache => cache.ExpiresAt);

        // Belirli input için cache aramak için.
        builder.HasIndex(cache => cache.NormalizedInput);

        // Dil çifti bazlı cache aramaları için.
        builder.HasIndex(cache => new
        {
            cache.SourceLanguageCode,
            cache.TargetLanguageCode
        });

        // En sık kullanılan cache kayıtlarını analiz etmek için.
        builder.HasIndex(cache => cache.HitCount);

        // Provider + operation + cache key lookup senaryoları için.
        builder.HasIndex(cache => new
        {
            cache.ProviderName,
            cache.OperationName,
            cache.CacheKey
        });
    }
}