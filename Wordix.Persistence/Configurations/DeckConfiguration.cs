using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Deck entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Deck kullanıcıya ait çalışma koleksiyonudur.
/// Ownership KeycloakUserId ile yapılır.
/// </summary>
public class DeckConfiguration : IEntityTypeConfiguration<Deck>
{
    public void Configure(EntityTypeBuilder<Deck> builder)
    {
        builder.ToTable("Decks");

        builder.ConfigureAuditableEntity();

        builder.Property(deck => deck.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(deck => deck.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(deck => deck.NormalizedName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(deck => deck.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(deck => deck.IsActive)
            .IsRequired();

        // Kullanıcının kendi decklerini listelemek için temel index.
        builder.HasIndex(deck => deck.KeycloakUserId);

        // Aynı kullanıcı aynı aktif deck adını ikinci kez oluşturamasın.
        //
        // Neden NormalizedName?
        // "Software English", " software english " ve "SOFTWARE ENGLISH"
        // aynı deck adı kabul edilsin.
        //
        // Neden filtered index?
        // İleride soft delete geldiğinde kullanıcı aynı deck adını tekrar kullanabilsin.
        builder.HasIndex(deck => new
        {
            deck.KeycloakUserId,
            deck.NormalizedName
        })
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Aktif deck sorgularında yardımcı olur.
        builder.HasIndex(deck => deck.IsActive);

        // Önemli:
        // Burada UserProfile foreign key yok.
        // Keycloak kullanıcısı Wordix database'inde tablo olarak tutulmaz.
    }
}