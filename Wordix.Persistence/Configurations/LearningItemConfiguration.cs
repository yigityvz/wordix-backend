using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// LearningItem entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// LearningItem, Word/Phrase/Sentence gibi öğrenilebilir içeriklerin ortak çatısıdır.
/// Quiz, dictionary ve review sistemleri LearningItem üzerinden çalışır.
/// </summary>
public class LearningItemConfiguration : IEntityTypeConfiguration<LearningItem>
{
    public void Configure(EntityTypeBuilder<LearningItem> builder)
    {
        builder.ToTable("LearningItems");

        builder.ConfigureAuditableEntity();

        builder.Property(item => item.ItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.LanguageId)
            .IsRequired();

        builder.Property(item => item.CefrLevel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.DifficultyGroup)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.ContentSource)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.QualityStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.ExternalSourceKey)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(item => item.ImportedAt)
            .IsRequired(false);

        builder.Property(item => item.IsActive)
            .IsRequired();

        // İçerikler sık sık dil ve tip üzerinden aranabilir.
        // Örneğin English + Word içerikleri.
        builder.HasIndex(item => new { item.LanguageId, item.ItemType });

        // Quiz ve öneri sisteminde zorluk filtresi kullanılacağı için index ekliyoruz.
        builder.HasIndex(item => item.DifficultyGroup);

        builder.HasIndex(item => item.IsActive);

        // İçeriğin gerçek kaynağına göre filtreleme/import analizleri için kullanılır.
        // Örnek: CefrJ kaynaklı içerikler, LibreTranslate kaynaklı içerikler.
        builder.HasIndex(item => item.ContentSource);

        // Review/admin ekranlarında kalite durumuna göre filtreleme yapılabilir.
        // Örnek: NeedsReview içerikleri listele.
        builder.HasIndex(item => item.QualityStatus);

        // Import tekrarlarını yakalamak için kullanılabilir.
        // Örnek: CEFR-J source key veya provider external key.
        builder.HasIndex(item => item.ExternalSourceKey);

        // LearningItem bir dile bağlıdır.
        // Dil silinirse ona bağlı içerikleri otomatik silmek istemeyiz.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(item => item.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}