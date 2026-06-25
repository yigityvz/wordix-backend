using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// LearningProgressHistory entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// LearningProgressHistory, UserLearningProgress değişimlerini geçmiş olarak saklar.
/// Bu sayede kullanıcının öğrenme skorunun ve durumunun zaman içinde nasıl değiştiği analiz edilebilir.
/// </summary>
public class LearningProgressHistoryConfiguration : IEntityTypeConfiguration<LearningProgressHistory>
{
    public void Configure(EntityTypeBuilder<LearningProgressHistory> builder)
    {
        builder.ToTable("LearningProgressHistories");

        // LearningProgressHistory BaseEntity'den gelir.
        // AuditableEntity kullanmadık çünkü history kayıtları güncellenmesi beklenen kayıtlar değildir.
        builder.ConfigureBaseEntity();

        builder.Property(history => history.UserLearningProgressId)
            .IsRequired();

        builder.Property(history => history.OldLearningStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.NewLearningStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(history => history.OldConfidenceScore)
            .IsRequired();

        builder.Property(history => history.NewConfidenceScore)
            .IsRequired();

        builder.Property(history => history.ChangeReason)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(history => history.CreatedAt)
            .IsRequired();

        // Bir progress kaydının geçmişini kronolojik listelemek için faydalı index.
        builder.HasIndex(history => new
        {
            history.UserLearningProgressId,
            history.CreatedAt
        });

        // History, progress kaydına bağlıdır.
        // Progress fiziksel silinirse history de silinebilir.
        // Normal uygulamada progress fiziksel silinmeyeceği için veri kaybı beklenmez.
        builder.HasOne<UserLearningProgress>()
            .WithMany()
            .HasForeignKey(history => history.UserLearningProgressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}