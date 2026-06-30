using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserLearningItem entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// UserLearningItem, kullanıcının kendi dictionary'sine kaydettiği LearningItem kayıtlarını tutar.
/// Bu yapı sadece Word için değil, ileride Phrase ve Sentence için de çalışacaktır.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile tablosu üzerinden kullanıcı sahipliği kurmaz.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - UserLearningItems tablosu kullanıcıyı doğrudan KeycloakUserId ile ilişkilendirir.
/// </summary>
public class UserLearningItemConfiguration : IEntityTypeConfiguration<UserLearningItem>
{
    public void Configure(EntityTypeBuilder<UserLearningItem> builder)
    {
        builder.ToTable("UserLearningItems");

        builder.ConfigureAuditableEntity();

        // KeycloakUserId, JWT token içindeki "sub" claiminden gelen kullanıcı id değeridir.
        // Backend bu kullanıcı için ayrıca UserProfileId/UserId üretmez.
        // Bu alan dış identity provider id'si olduğu için string olarak tutulur.
        builder.Property(item => item.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.LearningItemId)
            .IsRequired();

        builder.Property(item => item.SelectedMeaningId)
            .IsRequired(false);

        builder.Property(item => item.SourceLookupHistoryId)
            .IsRequired(false);

        builder.Property(item => item.SavedAt)
            .IsRequired();

        builder.Property(item => item.IsActive)
            .IsRequired();

        // Aynı Keycloak kullanıcısı aynı LearningItem'ı dictionary'ye iki kere ekleyemesin.
        //
        // Eski yapı:
        // UserProfileId + LearningItemId
        //
        // Yeni yapı:
        // KeycloakUserId + LearningItemId
        builder.HasIndex(item => new
        {
            item.KeycloakUserId,
            item.LearningItemId
        })
            .IsUnique();

        // Kullanıcının aktif dictionary kayıtlarını listelemek için faydalı index.
        // Dictionary ekranı bu index üzerinden hızlı filtreleme yapabilir.
        builder.HasIndex(item => new
        {
            item.KeycloakUserId,
            item.IsActive
        });

        // Önemli:
        // Burada artık UserProfile foreign key ilişkisi yok.
        // Çünkü Keycloak kullanıcısı bizim SQL database'imizde bir tablo olarak tutulmuyor.
        // KeycloakUserId dış identity provider id'si olarak saklanır.

        // Dictionary kaydı LearningItem'a bağlıdır.
        // LearningItem silinirse kullanıcının dictionary geçmişinin otomatik silinmesini istemiyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(item => item.LearningItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kullanıcı belirli bir anlam seçmiş olabilir.
        // Meaning silinirse dictionary kaydını silmek istemeyiz.
        builder.HasOne<Meaning>()
            .WithMany()
            .HasForeignKey(item => item.SelectedMeaningId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dictionary kaydı bir lookup sonucundan gelmiş olabilir.
        // LookupHistory silinirse dictionary kaydını silmek istemeyiz.
        builder.HasOne<LookupHistory>()
            .WithMany()
            .HasForeignKey(item => item.SourceLookupHistoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}