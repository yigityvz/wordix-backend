using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// LearningItemExampleSentence entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// Bu tablo ne işe yarar?
/// - Word/Phrase LearningItem ile örnek Sentence arasındaki ilişkiyi kurar.
/// - Sentence'in kendisi LearningItem olmak zorunda değildir.
/// - Aynı sentence birden fazla kelime/phrase için örnek olarak kullanılabilir.
/// 
/// Örnek:
/// LearningItem: abandon
/// Sentence: He decided to abandon the project.
/// SentenceTranslation: Projeyi terk etmeye karar verdi.
/// </summary>
public sealed class LearningItemExampleSentenceConfiguration
    : IEntityTypeConfiguration<LearningItemExampleSentence>
{
    public void Configure(EntityTypeBuilder<LearningItemExampleSentence> builder)
    {
        builder.ToTable("LearningItemExampleSentences");

        builder.ConfigureAuditableEntity();

        builder.Property(example => example.LearningItemId)
            .IsRequired();

        builder.Property(example => example.SentenceId)
            .IsRequired();

        builder.Property(example => example.SentenceTranslationId)
            .IsRequired(false);

        builder.Property(example => example.IsPrimary)
            .IsRequired();

        builder.Property(example => example.DisplayOrder)
            .IsRequired();

        // Bir LearningItem'ın örnek cümlelerini sıralı çekmek için ana index.
        builder.HasIndex(example => new
        {
            example.LearningItemId,
            example.DisplayOrder
        });

        // Aynı LearningItem + Sentence ilişkisi tekrar tekrar eklenmesin.
        builder.HasIndex(example => new
        {
            example.LearningItemId,
            example.SentenceId
        })
        .IsUnique();

        // Bir LearningItem için tek primary örnek cümle olsun.
        //
        // SQL Server filtered unique index:
        // IsPrimary = 1 olan kayıtlarda LearningItemId tekil olmalıdır.
        // IsPrimary = 0 olan kayıtlarda birden fazla örnek cümle olabilir.
        builder.HasIndex(example => example.LearningItemId)
            .IsUnique()
            .HasFilter("[IsPrimary] = 1");

        // Sentence üzerinden geriye doğru örnek bağlantılarını bulmak için.
        builder.HasIndex(example => example.SentenceId);

        // Belirli bir translation'ın hangi örnek bağlantılarında kullanıldığını bulmak için.
        builder.HasIndex(example => example.SentenceTranslationId);

        // LearningItem fiziksel silinirse example bağlantıları ne olsun?
        //
        // Wordix'te LearningItem fiziksel silme normal akış değildir.
        // Analytics ve geçmiş kayıtları korumak için genelde IsActive false yapılır.
        //
        // Bu yüzden burada Cascade yerine NoAction kullanıyoruz.
        // Ayrıca SQL Server multiple cascade path riskini azaltır.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(example => example.LearningItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Sentence silinirse example bağlantısının otomatik cascade ile silinmesini istemiyoruz.
        // Fiziksel delete zaten normal akış değil.
        builder.HasOne<Sentence>()
            .WithMany()
            .HasForeignKey(example => example.SentenceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Translation opsiyoneldir.
        // Bir example sentence önce çevirisiz gelebilir, sonra translation eklenebilir.
        builder.HasOne<SentenceTranslation>()
            .WithMany()
            .HasForeignKey(example => example.SentenceTranslationId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}