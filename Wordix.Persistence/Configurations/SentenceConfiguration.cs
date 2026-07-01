using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// Sentence entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Faz 19 kararı:
/// - Her sentence lookup sonucu database'e yazılmaz.
/// - Kullanıcı sentence'i dictionary'ye kaydetmek isterse kalıcı Sentence oluşur.
/// - İleride import/example sentence kayıtları LearningItem olmadan da tutulabilir.
/// </summary>
public class SentenceConfiguration : IEntityTypeConfiguration<Sentence>
{
    public void Configure(EntityTypeBuilder<Sentence> builder)
    {
        builder.ToTable("Sentences");

        builder.ConfigureAuditableEntity();

        builder.Property(sentence => sentence.LearningItemId)
            .IsRequired(false);

        builder.Property(sentence => sentence.LanguageId)
            .IsRequired();

        builder.Property(sentence => sentence.Text)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(sentence => sentence.NormalizedText)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(sentence => sentence.ExternalSentenceId)
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(sentence => sentence.SourceProvider)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(sentence => sentence.License)
            .HasMaxLength(200)
            .IsRequired(false);

        // Bir Sentence öğrenilebilir içerik olarak kullanılıyorsa
        // en fazla bir LearningItem'a bağlı olmalıdır.
        //
        // LearningItemId nullable olduğu için SQL Server tarafında unique index,
        // birden fazla null değerine izin verme konusunda sorun çıkarabilir.
        // Bu yüzden filtered unique index kullanıyoruz.
        builder.HasIndex(sentence => sentence.LearningItemId)
            .IsUnique()
            .HasFilter("[LearningItemId] IS NOT NULL");

        // Aynı dilde aynı normalize sentence metnini tekrar tekrar oluşturmak istemiyoruz.
        //
        // Örnek:
        // LanguageId = English
        // NormalizedText = "i want to improve my english"
        //
        // Bu ikili global sentence havuzunda tek olmalıdır.
        builder.HasIndex(sentence => new
        {
            sentence.LanguageId,
            sentence.NormalizedText
        })
            .IsUnique();

        // Provider/import kaynaklarına göre filtreleme ve analiz için kullanılır.
        builder.HasIndex(sentence => sentence.SourceProvider);

        // Dış provider id'si varsa ileride import tekrarlarını yakalamak için işe yarar.
        builder.HasIndex(sentence => sentence.ExternalSentenceId);

        // Sentence, opsiyonel olarak LearningItem'a bağlanır.
        //
        // LearningItem silinirse bağlı learnable sentence de silinebilir.
        // Ancak LearningItemId null olan example/import sentence kayıtları bu ilişkiden etkilenmez.
        builder.HasOne<LearningItem>()
            .WithOne()
            .HasForeignKey<Sentence>(sentence => sentence.LearningItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sentence'in kaynak dili Language tablosuna bağlıdır.
        // Dil silinirse sentence kayıtlarını cascade silmek istemeyiz.
        builder.HasOne<Language>()
            .WithMany()
            .HasForeignKey(sentence => sentence.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}