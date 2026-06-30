using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserPreference entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// UserPreference, kullanıcının quiz, öneri, motivasyon ve uygulama tercihlerini tutar.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile tablosu üzerinden kullanıcı sahipliği kurmaz.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - UserPreferences tablosu kullanıcıyı doğrudan KeycloakUserId ile ilişkilendirir.
/// </summary>
public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.ConfigureAuditableEntity();

        // KeycloakUserId, JWT token içindeki "sub" claiminden gelen kullanıcı id değeridir.
        // Backend bu kullanıcı için ayrıca UserProfileId/UserId üretmez.
        // Bu alan dış identity provider id'si olduğu için string olarak tutulur.
        builder.Property(preference => preference.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(preference => preference.DefaultQuizType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(preference => preference.DefaultDifficultyGroup)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(preference => preference.IncludeSystemRecommendations)
            .IsRequired();

        builder.Property(preference => preference.MotivationMessagesEnabled)
            .IsRequired();

        builder.Property(preference => preference.PreferredQuestionCount)
            .IsRequired();

        // Her Keycloak kullanıcısı için bir tercih kaydı olmasını istiyoruz.
        //
        // Eski yapı:
        // UserProfileId unique
        //
        // Yeni yapı:
        // KeycloakUserId unique
        builder.HasIndex(preference => preference.KeycloakUserId)
            .IsUnique();

        // Önemli:
        // Burada artık UserProfile foreign key ilişkisi yok.
        // Çünkü Keycloak kullanıcısı bizim SQL database'imizde bir tablo olarak tutulmuyor.
        // KeycloakUserId dış identity provider id'si olarak saklanır.
    }
}