using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// SentenceTranslation entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// SentenceTranslation, Sentence entity'sinin hedef dildeki çevirilerini tutar.
/// Meaning tablosundan ayrı tutulur çünkü sentence-level translation farklı bir domain kavramıdır.
/// </summary>
public class SentenceTranslationConfiguration : IEntityTypeConfiguration<SentenceTranslation>
{
    public void Configure(EntityTypeBuilder<SentenceTranslation> builder)
    {
        builder.ToTable("SentenceTranslations");

        builder.ConfigureAuditableEntity();

        builder.Property(translation => translation.SourceSentenceId)
            .IsRequired();

        builder.Property(translation => translation.TargetLanguageId)
            .IsRequired();

        builder.Property(translation => translation.TranslatedText)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(translation => translation.NormalizedTranslatedText)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(translation => translation.SourceProvider)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(translation => translation.License)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(translation => translation.IsPrimary)
            .IsRequired();

        builder.Property(translation => translation.DisplayOrder)
            .IsRequired();

        // Aynı sentence için hedef dile göre çevirileri hızlı bulmak için kullanılır.
        builder.HasIndex(translation => new
        {
            translation.SourceSentenceId,
            translation.TargetLanguageId
        });

        // Aynı sentence + hedef dil + aynı normalize çeviri tekrar tekrar eklenmesin.
        builder.HasIndex(translation => new
        {
            translation.SourceSentenceId,
            translation.TargetLanguageId,
            translation.NormalizedTranslatedText
        })
        .IsUnique();

        // Primary çevirileri hızlı bulmak için kullanılır.
        builder.HasIndex(translation => translation.IsPrimary);

        // Provider/import analizleri için kullanılır.
        builder.HasIndex(translation => translation.SourceProvider);

        // Sentence silinirse çevirileri de silinir.
        // Çünkü SentenceTranslation, Sentence olmadan anlamlı değildir.
        builder.HasOne<Sentence>()
            .WithMany()
            .HasForeignKey(translation => translation.SourceSentenceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Hedef dil Language tablosuna bağlıdır.
        // Dil silinirse translation kayıtlarını cascade silmek istemeyiz.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(translation => translation.TargetLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}