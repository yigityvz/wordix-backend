using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// AdminActionLog entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu configuration ne yapar?
/// - Tablo adını belirler.
/// - AuditableEntity alanlarını yapılandırır.
/// - String alanların maksimum uzunluklarını belirler.
/// - Admin audit ve analytics sorguları için indexler oluşturur.
/// 
/// Bu tablo neden önemli?
/// - Admin analytics endpointleri sistem genelindeki hassas verileri gösterir.
/// - Hangi adminin hangi veriye ne zaman eriştiğini audit edebilmek isteriz.
/// </summary>
public sealed class AdminActionLogConfiguration
    : IEntityTypeConfiguration<AdminActionLog>
{
    public void Configure(EntityTypeBuilder<AdminActionLog> builder)
    {
        builder.ToTable("AdminActionLogs");

        builder.ConfigureAuditableEntity();

        builder.Property(log => log.AdminKeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.ActionType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(log => log.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(log => log.RelatedEntityType)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(log => log.RelatedEntityId)
            .IsRequired(false);

        // Belirli bir adminin yaptığı işlemleri tarih sırasıyla görmek için.
        builder.HasIndex(log => new
        {
            log.AdminKeycloakUserId,
            log.CreatedAt
        });

        // Hangi action ne kadar kullanılmış veya kimler tarafından tetiklenmiş analizleri için.
        builder.HasIndex(log => new
        {
            log.ActionType,
            log.CreatedAt
        });

        // Belirli bir entity ile ilgili admin işlemlerini bulmak için.
        // Örnek:
        // Bir ImportJob üzerinde hangi admin işlemleri yapılmış?
        builder.HasIndex(log => new
        {
            log.RelatedEntityType,
            log.RelatedEntityId
        });

        // Admin audit ekranında son işlemleri hızlı listelemek için.
        builder.HasIndex(log => log.CreatedAt);
    }
}