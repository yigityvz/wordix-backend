using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Phrase entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Phrase, sistemin global kalıp ifade havuzudur.
/// Kullanıcıya özel değildir.
/// Kullanıcı phrase'i dictionary'sine eklediğinde UserLearningItem oluşur.
/// 
/// Örnek:
/// - give up
/// - look after
/// - by the way
/// - take care of
/// </summary>
public class PhraseConfiguration : IEntityTypeConfiguration<Phrase>
{
    public void Configure(EntityTypeBuilder<Phrase> builder)
    {
        builder.ToTable("Phrases");

        builder.ConfigureAuditableEntity();

        builder.Property(phrase => phrase.LearningItemId)
            .IsRequired();

        builder.Property(phrase => phrase.Text)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(phrase => phrase.NormalizedText)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(phrase => phrase.PhraseType)
            .HasConversion<int>()
            .IsRequired();

        // Her Phrase kaydı bir LearningItem detay kaydıdır.
        // Bir LearningItem'a birden fazla Phrase bağlanmamalıdır.
        builder.HasIndex(phrase => phrase.LearningItemId)
            .IsUnique();

        // Lookup sırasında normalize edilmiş metinle arama yapacağız.
        // Örneğin kullanıcı " Give Up " yazsa bile "give up" üzerinden aranır.
        builder.HasIndex(phrase => phrase.NormalizedText);

        // Phrase türüne göre ileride filtreleme yapılabilir.
        // Örneğin sadece PhrasalVerb quizleri veya sadece Idiom listesi.
        builder.HasIndex(phrase => phrase.PhraseType);

        // Phrase, LearningItem'a birebir bağlıdır.
        // LearningItem silinirse Phrase de silinebilir.
        // Çünkü Phrase, LearningItem olmadan anlamlı değildir.
        builder.HasOne<LearningItem>()
            .WithOne()
            .HasForeignKey<Phrase>(phrase => phrase.LearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}