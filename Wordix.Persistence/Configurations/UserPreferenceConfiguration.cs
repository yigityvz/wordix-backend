using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserPreference entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// UserPreference, kullanıcının quiz ve uygulama tercihlerini tutar.
/// UserProfile şişmesin diye ayrı tablo olarak tasarlanmıştır.
/// </summary>
public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreferences");

        builder.ConfigureAuditableEntity();

        builder.Property(preference => preference.UserProfileId)
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

        // Her kullanıcı için bir tercih kaydı olmasını istiyoruz.
        builder.HasIndex(preference => preference.UserProfileId)
            .IsUnique();

        // UserPreference, UserProfile'a bağlıdır.
        // UserProfile silinirse preference kaydı da silinebilir.
        // Çünkü preference tek başına anlamlı değildir.
        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<UserPreference>(preference => preference.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}