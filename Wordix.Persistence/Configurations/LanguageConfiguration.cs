using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Language entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Language, sistemde desteklenen dilleri tutar.
/// Örnek:
/// - en / English / English
/// - tr / Turkish / Türkçe
/// </summary>
public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");

        builder.ConfigureAuditableEntity();

        builder.Property(language => language.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(language => language.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(language => language.NativeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(language => language.IsActive)
            .IsRequired();

        // Aynı dil kodu birden fazla kez eklenmemeli.
        // Örneğin iki tane "en" kaydı oluşmasını engeller.
        builder.HasIndex(language => language.Code)
            .IsUnique();

        builder.HasIndex(language => language.IsActive);
    }
}