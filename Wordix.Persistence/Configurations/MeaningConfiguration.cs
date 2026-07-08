using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Meaning entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Meaning, Word veya ileride Phrase içeriklerinin anlamlarını tutar.
/// Doğrudan Word'e değil LearningItem'a bağlıdır.
/// Böylece aynı Meaning altyapısı Word ve Phrase için ortak kullanılabilir.
/// </summary>
public class MeaningConfiguration : IEntityTypeConfiguration<Meaning>
{
    public void Configure(EntityTypeBuilder<Meaning> builder)
    {
        builder.ToTable("Meanings");

        builder.ConfigureAuditableEntity();

        builder.Property(meaning => meaning.LearningItemId)
            .IsRequired();

        builder.Property(meaning => meaning.TargetLanguageId)
            .IsRequired();

        builder.Property(meaning => meaning.MeaningText)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(meaning => meaning.ShortDefinition)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(meaning => meaning.PartOfSpeech)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(meaning => meaning.Category)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(meaning => meaning.IsPrimary)
            .IsRequired();

        builder.Property(meaning => meaning.DisplayOrder)
            .IsRequired();

        builder.Property(meaning => meaning.ContentSource)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(meaning => meaning.QualityStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(meaning => meaning.SourceProvider)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(meaning => meaning.License)
            .HasMaxLength(200)
            .IsRequired(false);

        // Bir LearningItem'ın anlamları genelde hedef dil ve görüntüleme sırasına göre çekilecek.
        builder.HasIndex(meaning => new
        {
            meaning.LearningItemId,
            meaning.TargetLanguageId,
            meaning.DisplayOrder
        });

        // Primary anlam sorguları için faydalı index.
        builder.HasIndex(meaning => new
        {
            meaning.LearningItemId,
            meaning.TargetLanguageId,
            meaning.IsPrimary
        });

        // Anlamların kaynağına göre analiz yapılabilir.
        // Örnek: WiktionaryKaikki'den gelen meaning kayıtları.
        builder.HasIndex(meaning => meaning.ContentSource);

        // Admin/review ekranında NeedsReview anlamları listelemek için kullanılır.
        builder.HasIndex(meaning => meaning.QualityStatus);

        // Provider bazlı analiz için kullanılır.
        // Örnek: Kaikki, LibreTranslate.
        builder.HasIndex(meaning => meaning.SourceProvider);

        // Meaning, LearningItem'a bağlıdır.
        // LearningItem silinirse Meaning kayıtları da silinebilir.
        // Çünkü anlam, içerik olmadan tek başına anlamlı değildir.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(meaning => meaning.LearningItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Meaning hedef dile bağlıdır.
        // Dil silinirse anlamların otomatik silinmesini istemeyiz.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(meaning => meaning.TargetLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}