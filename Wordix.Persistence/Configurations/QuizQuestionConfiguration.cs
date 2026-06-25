using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// QuizQuestion entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// QuizQuestion, bir quiz oturumunda sorulan tek bir soruyu temsil eder.
/// Soru doğrudan Word'e değil LearningItem'a bağlıdır.
/// Böylece ileride Phrase ve Sentence soruları da aynı yapı ile desteklenebilir.
/// </summary>
public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");

        builder.ConfigureAuditableEntity();

        builder.Property(question => question.QuizSessionId)
            .IsRequired();

        builder.Property(question => question.LearningItemId)
            .IsRequired();

        builder.Property(question => question.QuestionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(question => question.QuestionText)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(question => question.CorrectAnswer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(question => question.DisplayOrder)
            .IsRequired();

        builder.Property(question => question.IsSystemRecommended)
            .IsRequired();

        // Aynı quiz içinde aynı sıra numarası iki kere kullanılmasın.
        builder.HasIndex(question => new
        {
            question.QuizSessionId,
            question.DisplayOrder
        })
        .IsUnique();

        // LearningItem üzerinden soru geçmişi analizleri yapılabilir.
        // Örneğin en çok yanlış yapılan içerikler.
        builder.HasIndex(question => question.LearningItemId);

        // QuizQuestion, QuizSession olmadan anlamlı değildir.
        // QuizSession fiziksel silinirse soruları da silinebilir.
        builder.HasOne<QuizSession>()
            .WithMany()
            .HasForeignKey(question => question.QuizSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizQuestion, hangi içerikten üretildiğini LearningItem ile bilir.
        // LearningItem fiziksel silinirse geçmiş quiz sorularının otomatik silinmesini istemiyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(question => question.LearningItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}