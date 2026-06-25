using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserLearningProgress entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// UserLearningProgress, kullanıcının dictionary'sine kaydettiği bir içerikteki
/// güncel öğrenme durumunu temsil eder.
/// 
/// Bu tablo quiz/review algoritmasının temel kaynaklarından biridir.
/// </summary>
public class UserLearningProgressConfiguration : IEntityTypeConfiguration<UserLearningProgress>
{
    public void Configure(EntityTypeBuilder<UserLearningProgress> builder)
    {
        builder.ToTable("UserLearningProgresses");

        builder.ConfigureAuditableEntity();

        builder.Property(progress => progress.UserLearningItemId)
            .IsRequired();

        builder.Property(progress => progress.LearningStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(progress => progress.CorrectCount)
            .IsRequired();

        builder.Property(progress => progress.WrongCount)
            .IsRequired();

        builder.Property(progress => progress.ConsecutiveCorrectCount)
            .IsRequired();

        builder.Property(progress => progress.ConsecutiveWrongCount)
            .IsRequired();

        builder.Property(progress => progress.LearningConfidenceScore)
            .IsRequired();

        builder.Property(progress => progress.RepetitionLevel)
            .IsRequired();

        builder.Property(progress => progress.NextReviewDate)
            .IsRequired(false);

        builder.Property(progress => progress.LastReviewedAt)
            .IsRequired(false);

        // Her dictionary item için yalnızca bir progress kaydı olmalı.
        builder.HasIndex(progress => progress.UserLearningItemId)
            .IsUnique();

        // Review zamanı gelen içerikleri bulmak için kullanılabilir.
        builder.HasIndex(progress => progress.NextReviewDate);

        // Kullanıcının öğrenme durumuna göre filtreleme yapılırken faydalı olur.
        builder.HasIndex(progress => progress.LearningStatus);

        // UserLearningProgress, UserLearningItem olmadan anlamlı değildir.
        // UserLearningItem fiziksel silinirse progress de silinebilir.
        // Normal akışta zaten fiziksel silme yerine IsActive false yapılacak.
        builder.HasOne<UserLearningItem>()
            .WithOne()
            .HasForeignKey<UserLearningProgress>(progress => progress.UserLearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}