using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserProfile entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// UserProfile, Keycloak kullanıcısının Wordix database'indeki karşılığıdır.
/// Keycloak authentication yapar; Wordix ise kullanıcıya ait uygulama içi profil bilgisini tutar.
/// </summary>
public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        // BaseEntity + AuditableEntity alanlarını merkezi helper ile yapılandırıyoruz.
        builder.ConfigureAuditableEntity();

        builder.Property(user => user.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.DisplayName)
            .HasMaxLength(150)
            .IsRequired(false);

        // Enumları int olarak saklıyoruz.
        // Örnek: BasicUser = 1, Admin = 2
        builder.Property(user => user.AccountType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(user => user.NativeLanguageId)
            .IsRequired(false);

        builder.Property(user => user.TargetLanguageId)
            .IsRequired(false);

        builder.Property(user => user.IsActive)
            .IsRequired();

        // Aynı Keycloak kullanıcısı için birden fazla UserProfile oluşmamalı.
        builder.HasIndex(user => user.KeycloakUserId)
            .IsUnique();

        // Aynı email ile birden fazla profil oluşmamalı.
        builder.HasIndex(user => user.Email)
            .IsUnique();

        // Username için unique vermiyoruz.
        // Çünkü bazı sistemlerde username değişebilir veya email ile aynı olabilir.
        // Ama arama performansı için index bırakıyoruz.
        builder.HasIndex(user => user.Username);

        // Kullanıcının ana dili Language tablosuna bağlanır.
        // DeleteBehavior.Restrict:
        // Kullanıcılara bağlı bir dili yanlışlıkla silmeyi engeller.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(user => user.NativeLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kullanıcının hedef dili Language tablosuna bağlanır.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(user => user.TargetLanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}