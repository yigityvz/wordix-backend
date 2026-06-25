using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// QuizOption entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// QuizOption, çoktan seçmeli quizlerde bir soruya ait seçenekleri temsil eder.
/// İlk prototipte Test quiz için aktif kullanılacaktır.
/// </summary>
public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("QuizOptions");

        // QuizOption BaseEntity'den gelir.
        // CreatedAt/UpdatedAt kullanmıyoruz çünkü seçenekler snapshot gibi oluşturulur.
        builder.ConfigureBaseEntity();

        builder.Property(option => option.QuizQuestionId)
            .IsRequired();

        builder.Property(option => option.OptionText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(option => option.IsCorrect)
            .IsRequired();

        builder.Property(option => option.DisplayOrder)
            .IsRequired();

        // Bir sorunun seçeneklerini sırasıyla listelemek için kullanılır.
        // Aynı soru içinde aynı DisplayOrder tekrar etmesin diye unique yaptık.
        builder.HasIndex(option => new
        {
            option.QuizQuestionId,
            option.DisplayOrder
        })
        .IsUnique();

        // Doğru seçeneği bulmak için kullanılır.
        builder.HasIndex(option => new
        {
            option.QuizQuestionId,
            option.IsCorrect
        });

        // QuizOption, QuizQuestion olmadan anlamlı değildir.
        // Soru fiziksel silinirse seçenekleri de silinebilir.
        builder.HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(option => option.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}