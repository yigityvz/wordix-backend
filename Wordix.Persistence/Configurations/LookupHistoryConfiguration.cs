using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// LookupHistory entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// LookupHistory, kullanıcının yaptığı arama/lookup işlemlerini kayıt altına alır.
/// Bu kayıtlar ileride:
/// - en çok aranan kelimeler,
/// - provider kullanım istatistikleri,
/// - database'de bulunamayan aramalar,
/// - kullanıcının arama davranışı
/// gibi analytics ihtiyaçları için kullanılacaktır.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile tablosu üzerinden kullanıcı sahipliği kurmaz.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - LookupHistory tablosu kullanıcıyı doğrudan KeycloakUserId ile ilişkilendirir.
/// </summary>
public class LookupHistoryConfiguration : IEntityTypeConfiguration<LookupHistory>
{
    public void Configure(EntityTypeBuilder<LookupHistory> builder)
    {
        builder.ToTable("LookupHistories");

        builder.ConfigureAuditableEntity();

        // KeycloakUserId, JWT token içindeki "sub" claiminden gelen kullanıcı id değeridir.
        // Backend bu kullanıcı için ayrıca UserProfileId/UserId üretmez.
        // Bu alan dış identity provider id'si olduğu için string olarak tutulur.
        builder.Property(lookup => lookup.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(lookup => lookup.QueryText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(lookup => lookup.NormalizedQueryText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(lookup => lookup.InputType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lookup => lookup.SourceLanguageId)
            .IsRequired();

        builder.Property(lookup => lookup.TargetLanguageId)
            .IsRequired();

        builder.Property(lookup => lookup.LearningItemId)
            .IsRequired(false);

        builder.Property(lookup => lookup.WasFoundInDatabase)
            .IsRequired();

        builder.Property(lookup => lookup.WasCreatedFromProvider)
            .IsRequired();

        builder.Property(lookup => lookup.ProviderType)
            .HasConversion<int>()
            .IsRequired(false);

        builder.Property(lookup => lookup.ProviderName)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(lookup => lookup.ResultCount)
            .IsRequired();

        // Kullanıcının geçmiş aramalarını listelemek için faydalı index.
        // Artık filtreleme UserProfileId ile değil, KeycloakUserId ile yapılır.
        builder.HasIndex(lookup => lookup.KeycloakUserId);

        // Admin analytics tarafında en çok aranan normalize metinleri bulmak için faydalı index.
        builder.HasIndex(lookup => lookup.NormalizedQueryText);

        // Lookup istatistiklerinde input tipi sık kullanılabilir.
        // Örneğin Word aramaları, Phrase aramaları, Sentence aramaları.
        builder.HasIndex(lookup => lookup.InputType);

        // Provider kullanılan aramaları analiz etmek için index ekliyoruz.
        builder.HasIndex(lookup => lookup.ProviderType);

        // Önemli:
        // Burada artık UserProfile foreign key ilişkisi yok.
        // Çünkü Keycloak kullanıcısı bizim SQL database'imizde bir tablo olarak tutulmuyor.
        // KeycloakUserId dış identity provider id'si olarak saklanır.

        // Kaynak dil ilişkisi.
        // Dil silinirse geçmiş lookup kayıtlarının silinmesini istemeyiz.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(lookup => lookup.SourceLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Hedef dil ilişkisi.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(lookup => lookup.TargetLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lookup sonucu bir LearningItem'a bağlanmış olabilir.
        // LearningItem silinirse geçmiş lookup kayıtlarının otomatik silinmesini istemiyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(lookup => lookup.LearningItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}