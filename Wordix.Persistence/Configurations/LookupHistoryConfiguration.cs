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
/// </summary>
public class LookupHistoryConfiguration : IEntityTypeConfiguration<LookupHistory>
{
    public void Configure(EntityTypeBuilder<LookupHistory> builder)
    {
        builder.ToTable("LookupHistories");

        builder.ConfigureAuditableEntity();

        builder.Property(lookup => lookup.UserProfileId)
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
        builder.HasIndex(lookup => lookup.UserProfileId);

        // Admin analytics tarafında en çok aranan normalize metinleri bulmak için faydalı index.
        builder.HasIndex(lookup => lookup.NormalizedQueryText);

        // Lookup istatistiklerinde input tipi sık kullanılabilir.
        // Örneğin Word aramaları, Phrase aramaları, Sentence aramaları.
        builder.HasIndex(lookup => lookup.InputType);

        // Provider kullanılan aramaları analiz etmek için index ekliyoruz.
        builder.HasIndex(lookup => lookup.ProviderType);

        // LookupHistory kullanıcıya bağlıdır.
        // Kullanıcı fiziksel silinirse lookup geçmişinin otomatik silinmesini istemiyoruz.
        // Analytics verisi kaybolmasın diye Restrict kullanıyoruz.
        builder.HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(lookup => lookup.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

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