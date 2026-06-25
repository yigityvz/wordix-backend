using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Word entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Word, sistemin global kelime havuzudur.
/// Kullanıcıya özel değildir.
/// Kullanıcı kelimeyi dictionary'sine eklediğinde UserLearningItem oluşur.
/// </summary>
public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");

        builder.ConfigureAuditableEntity();

        builder.Property(word => word.LearningItemId)
            .IsRequired();

        builder.Property(word => word.Text)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(word => word.NormalizedText)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(word => word.PartOfSpeech)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(word => word.Pronunciation)
            .HasMaxLength(100)
            .IsRequired(false);

        // Her Word kaydı bir LearningItem detay kaydıdır.
        // Bir LearningItem'a birden fazla Word bağlanmamalıdır.
        builder.HasIndex(word => word.LearningItemId)
            .IsUnique();

        // Lookup sırasında normalize edilmiş metinle arama yapacağız.
        builder.HasIndex(word => word.NormalizedText);

        // Word, LearningItem'a birebir bağlıdır.
        // LearningItem silinirse Word de silinebilir.
        // Çünkü Word, LearningItem olmadan anlamlı değildir.
        builder.HasOne<LearningItem>()
            .WithOne()
            .HasForeignKey<Word>(word => word.LearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}