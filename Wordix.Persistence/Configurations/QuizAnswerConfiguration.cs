using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// QuizAnswer entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// QuizAnswer, kullanıcının bir quiz sorusuna verdiği cevabı temsil eder.
/// Cevabın doğru/yanlış sonucu, cevap metni ve cevap süresi burada tutulur.
/// </summary>
public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("QuizAnswers");

        // QuizAnswer BaseEntity'den gelir.
        // AuditableEntity kullanmadık çünkü cevap kaydı oluşturulduktan sonra güncellenmesi beklenmez.
        // AnsweredAt zaten cevabın verildiği zamanı tutar.
        builder.ConfigureBaseEntity();

        builder.Property(answer => answer.QuizQuestionId)
            .IsRequired();

        builder.Property(answer => answer.UserProfileId)
            .IsRequired();

        builder.Property(answer => answer.SelectedQuizOptionId)
            .IsRequired(false);

        builder.Property(answer => answer.UserAnswer)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(answer => answer.CorrectAnswer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(answer => answer.AnswerResult)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(answer => answer.ResponseTimeMilliseconds)
            .IsRequired();

        builder.Property(answer => answer.AnsweredAt)
            .IsRequired();

        builder.Property(answer => answer.AddedToDictionaryBecauseWrong)
            .IsRequired();

        // Bir soruya verilen cevapları bulmak için kullanılır.
        builder.HasIndex(answer => answer.QuizQuestionId);

        // Kullanıcının cevap geçmişini analiz etmek için kullanılır.
        builder.HasIndex(answer => answer.UserProfileId);

        // Kullanıcının zaman içindeki cevaplarını analiz etmek için faydalı index.
        builder.HasIndex(answer => new
        {
            answer.UserProfileId,
            answer.AnsweredAt
        });

        // Doğru/yanlış analizleri için kullanılabilir.
        builder.HasIndex(answer => answer.AnswerResult);

        // QuizAnswer, QuizQuestion'a bağlıdır.
        // Soru fiziksel silinirse cevap da silinebilir.
        // Normal uygulama akışında quiz geçmişi fiziksel silinmeyecektir.
        builder.HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(answer => answer.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cevabı veren kullanıcıdır.
        // Kullanıcı fiziksel silinirse cevapların otomatik silinmesini istemiyoruz.
        // Analytics geçmişi korunmalı.
        builder.HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(answer => answer.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Test quizlerde seçilen option bilgisidir.
        // Writing quizlerde null olabilir.
        // Option silinirse cevap silinmesin diye Restrict kullanıyoruz.
        builder.HasOne<QuizOption>()
            .WithMany()
            .HasForeignKey(answer => answer.SelectedQuizOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}