using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Common;

namespace Wordix.Persistence.Configurations.Common;

/// <summary>
/// EF Core entity configuration dosyalarında tekrar eden ortak ayarları toplar.
/// 
/// Amaç:
/// - Her configuration dosyasında Id, CreatedAt, UpdatedAt ayarlarını tekrar yazmamak.
/// - Configuration dosyalarını küçük ve okunabilir tutmak.
/// - Program.cs ve DbContext'i şişirmemek.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// BaseEntity'den gelen ortak Id ayarını yapar.
    /// 
    /// ValueGeneratedNever kullanmamızın sebebi:
    /// Id değerini database değil, domain entity constructor'ı Guid.NewGuid() ile üretir.
    /// Böylece entity database'e gitmeden önce de benzersiz kimliğe sahip olur.
    /// </summary>
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedNever();
    }

    /// <summary>
    /// AuditableEntity'den gelen CreatedAt ve UpdatedAt alanlarını yapılandırır.
    /// 
    /// CreatedAt zorunludur.
    /// UpdatedAt opsiyoneldir çünkü yeni oluşturulan kayıt henüz güncellenmemiş olabilir.
    /// </summary>
    public static void ConfigureAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.ConfigureBaseEntity();

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired(false);
    }
}