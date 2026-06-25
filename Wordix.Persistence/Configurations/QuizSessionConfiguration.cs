using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// QuizSession entity'sinin veritabanı tablo ayarlarını yapar.
/// 
/// QuizSession, kullanıcının başlattığı quiz oturumunu temsil eder.
/// Bir quiz oturumu içinde birden fazla soru bulunabilir.
/// </summary>
public class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
{
    public void Configure(EntityTypeBuilder<QuizSession> builder)
    {
        builder.ToTable("QuizSessions");

        builder.ConfigureAuditableEntity();

        builder.Property(session => session.UserProfileId)
            .IsRequired();

        builder.Property(session => session.QuizType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(session => session.QuizSourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(session => session.QuizContentMode)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(session => session.DifficultyGroup)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(session => session.DeckId)
            .IsRequired(false);

        builder.Property(session => session.IncludeSystemRecommendations)
            .IsRequired();

        builder.Property(session => session.QuestionCount)
            .IsRequired();

        builder.Property(session => session.StartedAt)
            .IsRequired();

        builder.Property(session => session.CompletedAt)
            .IsRequired(false);

        builder.Property(session => session.Status)
            .HasConversion<int>()
            .IsRequired();

        // Kullanıcının quiz geçmişini listelemek için kullanılır.
        builder.HasIndex(session => session.UserProfileId);

        // Kullanıcının quiz geçmişini tarih sırasına göre çekmek için faydalı index.
        builder.HasIndex(session => new
        {
            session.UserProfileId,
            session.StartedAt
        });

        // Devam eden quizleri bulmak için kullanılabilir.
        builder.HasIndex(session => session.Status);

        // QuizSession kullanıcıya bağlıdır.
        // Kullanıcı fiziksel silinirse quiz geçmişinin otomatik silinmesini istemiyoruz.
        // Normal uygulama akışında kullanıcı pasifleştirilecek, fiziksel silinmeyecek.
        builder.HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(session => session.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}