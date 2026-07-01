using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// DeckItem entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// DeckItem, Deck ile UserLearningItem arasındaki ilişkiyi temsil eder.
/// </summary>
public class DeckItemConfiguration : IEntityTypeConfiguration<DeckItem>
{
    public void Configure(EntityTypeBuilder<DeckItem> builder)
    {
        builder.ToTable("DeckItems");

        builder.ConfigureAuditableEntity();

        builder.Property(deckItem => deckItem.DeckId)
            .IsRequired();

        builder.Property(deckItem => deckItem.UserLearningItemId)
            .IsRequired();

        builder.Property(deckItem => deckItem.AddedAt)
            .IsRequired();

        // Aynı UserLearningItem aynı deck'e ikinci kez eklenemesin.
        builder.HasIndex(deckItem => new
        {
            deckItem.DeckId,
            deckItem.UserLearningItemId
        })
            .IsUnique();

        // Belirli bir deck içindeki itemları listelemek için kullanılır.
        builder.HasIndex(deckItem => deckItem.DeckId);

        // Bir UserLearningItem hangi decklerde var sorusu için ileride faydalı olabilir.
        builder.HasIndex(deckItem => deckItem.UserLearningItemId);

        // Deck silinirse DeckItem kayıtları da silinsin.
        //
        // Faz 20'de Deck delete endpointi yok.
        // Ama ileride fiziksel deck silme yapılırsa orphan DeckItem kalmamalı.
        builder.HasOne<Deck>()
            .WithMany()
            .HasForeignKey(deckItem => deckItem.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserLearningItem silinirse DeckItem ilişkisi de silinsin.
        //
        // Not:
        // Şu an UserLearningItem soft-delete gibi IsActive ile yönetiliyor.
        // Fiziksel silme yapılırsa ilişki de temizlenir.
        builder.HasOne<UserLearningItem>()
            .WithMany()
            .HasForeignKey(deckItem => deckItem.UserLearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}