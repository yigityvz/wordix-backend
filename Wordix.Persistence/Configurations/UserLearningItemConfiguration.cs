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
/// </summary>
public class UserLearningItemConfiguration : IEntityTypeConfiguration<UserLearningItem>
{
    public void Configure(EntityTypeBuilder<UserLearningItem> builder)
    {
        builder.ToTable("UserLearningItems");

        builder.ConfigureAuditableEntity();

        builder.Property(item => item.UserProfileId)
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

        // Aynı kullanıcı aynı LearningItem'ı dictionary'ye iki kere ekleyemesin.
        builder.HasIndex(item => new
        {
            item.UserProfileId,
            item.LearningItemId
        })
            .IsUnique();

        // Kullanıcının aktif dictionary kayıtlarını listelemek için faydalı index.
        builder.HasIndex(item => new
        {
            item.UserProfileId,
            item.IsActive
        });

        // Kullanıcının dictionary kayıtları kullanıcı profiline bağlıdır.
        // Kullanıcı fiziksel silinirse dictionary kayıtlarını otomatik silmek istemiyoruz.
        // Öğrenme ve quiz geçmişi korunmalı.
        builder.HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(item => item.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

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