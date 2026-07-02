using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserLearningFlag entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu configuration ne yapar?
/// - Tablo adını belirler.
/// - Primary key tanımlar.
/// - UserLearningItem ilişkisini kurar.
/// - FlagType alanını zorunlu yapar.
/// - Aynı UserLearningItem için aynı FlagType'ın tekrar eklenmesini engeller.
/// 
/// Önemli unique kural:
/// UserLearningItemId + FlagType unique olmalıdır.
/// 
/// Böylece kullanıcı aynı item'ı iki kez Favorite veya iki kez Difficult yapamaz.
/// </summary>
public sealed class UserLearningFlagConfiguration
    : IEntityTypeConfiguration<UserLearningFlag>
{
    public void Configure(EntityTypeBuilder<UserLearningFlag> builder)
    {
        builder.ToTable("UserLearningFlags");

        builder.HasKey(flag => flag.Id);

        builder.Property(flag => flag.UserLearningItemId)
            .IsRequired();

        builder.Property(flag => flag.FlagType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(flag => flag.CreatedAt)
            .IsRequired();

        builder.HasIndex(flag => flag.UserLearningItemId);

        builder.HasIndex(flag => new
        {
            flag.UserLearningItemId,
            flag.FlagType
        })
            .IsUnique();

        // UserLearningFlag doğrudan KeycloakUserId tutmaz.
        // Ownership şu zincirle doğrulanır:
        // UserLearningFlag -> UserLearningItem -> KeycloakUserId
        builder.HasOne<UserLearningItem>()
            .WithMany()
            .HasForeignKey(flag => flag.UserLearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}