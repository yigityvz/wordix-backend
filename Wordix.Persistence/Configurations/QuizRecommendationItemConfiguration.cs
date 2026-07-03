using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// QuizRecommendationItem entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu configuration ne yapar?
/// - Tablo adını belirler.
/// - AuditableEntity alanlarını yapılandırır.
/// - Enum alanlarını int olarak saklar.
/// - QuizSessionId, QuizQuestionId ve LearningItemId için index oluşturur.
/// - Sistem önerisi analizleri için gerekli ilişkileri tanımlar.
/// 
/// Önemli:
/// Bu tablo user-owned değildir; ownership QuizSession üzerinden kontrol edilir.
/// Yani:
/// QuizRecommendationItem -> QuizSession -> KeycloakUserId
/// zinciri kullanılır.
/// </summary>
public sealed class QuizRecommendationItemConfiguration
    : IEntityTypeConfiguration<QuizRecommendationItem>
{
    public void Configure(EntityTypeBuilder<QuizRecommendationItem> builder)
    {
        builder.ToTable("QuizRecommendationItems");

        builder.ConfigureAuditableEntity();

        builder.Property(item => item.QuizSessionId)
            .IsRequired();

        builder.Property(item => item.QuizQuestionId)
            .IsRequired();

        builder.Property(item => item.LearningItemId)
            .IsRequired();

        builder.Property(item => item.RecommendationReason)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.DifficultyGroup)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(item => item.WasAnsweredCorrectly)
            .IsRequired(false);

        builder.Property(item => item.WasAddedToDictionary)
            .IsRequired();

        // Bir quiz session içindeki önerileri hızlı bulmak için.
        builder.HasIndex(item => item.QuizSessionId);

        // Submit answer sırasında cevaplanan question'ın recommendation kaydı var mı diye bakacağız.
        builder.HasIndex(item => item.QuizQuestionId);

        // Analytics için:
        // Hangi LearningItem kaç kez önerilmiş?
        builder.HasIndex(item => item.LearningItemId);

        // Aynı QuizQuestion için birden fazla recommendation kaydı oluşmasını istemiyoruz.
        // Çünkü bir soru ya sistem önerisidir ya değildir.
        builder.HasIndex(item => item.QuizQuestionId)
            .IsUnique();

        // Recommendation kayıtları quiz session geçmişine bağlıdır.
        //
        // Fiziksel delete Wordix'te normal akış değildir.
        // Bu yüzden NoAction kullanıyoruz.
        // Böylece yanlışlıkla quiz geçmişi silinmeye çalışılırsa analytics kayıtları da korunur.
        builder.HasOne<QuizSession>()
            .WithMany()
            .HasForeignKey(item => item.QuizSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        // QuizQuestion ile ilişki kuruyoruz.
        //
        // Cascade kullanmıyoruz.
        // Çünkü QuizQuestion zaten QuizSession'a bağlı ve SQL Server'da çoklu cascade path riskinden kaçınmak istiyoruz.
        builder.HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(item => item.QuizQuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Önerilen global LearningItem'dır.
        //
        // LearningItem fiziksel silinirse geçmiş recommendation kaydının silinmesini istemiyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(item => item.LearningItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}