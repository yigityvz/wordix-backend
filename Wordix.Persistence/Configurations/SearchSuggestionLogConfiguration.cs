using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;
using Wordix.Persistence.Configurations.Common;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// SearchSuggestionLog entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu configuration ne yapar?
/// - Tablo adını belirler.
/// - KeycloakUserId uzunluğunu sınırlar.
/// - RecommendationReason enum değerini int olarak saklar.
/// - Suggestion analytics için indexler oluşturur.
/// 
/// Bu tablo user-owned çalışır.
/// Ownership doğrudan KeycloakUserId üzerinden yapılır.
/// </summary>
public sealed class SearchSuggestionLogConfiguration
    : IEntityTypeConfiguration<SearchSuggestionLog>
{
    public void Configure(EntityTypeBuilder<SearchSuggestionLog> builder)
    {
        builder.ToTable("SearchSuggestionLogs");

        builder.ConfigureAuditableEntity();

        builder.Property(log => log.KeycloakUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.LearningItemId)
            .IsRequired();

        builder.Property(log => log.QuizSessionId)
            .IsRequired(false);

        builder.Property(log => log.QuizRecommendationItemId)
            .IsRequired(false);

        builder.Property(log => log.SuggestionReason)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(log => log.WasAccepted)
            .IsRequired();

        builder.Property(log => log.WasSaved)
            .IsRequired();

        // Kullanıcının öneri geçmişini çekmek için ana index.
        builder.HasIndex(log => log.KeycloakUserId);

        // Kullanıcının öneri geçmişini tarihe göre analiz etmek için.
        builder.HasIndex(log => new
        {
            log.KeycloakUserId,
            log.CreatedAt
        });

        // Belirli bir item kaç kez önerilmiş?
        builder.HasIndex(log => log.LearningItemId);

        // Quiz recommendation item üzerinden log bulmak için.
        builder.HasIndex(log => log.QuizRecommendationItemId);

        // Aynı recommendation item için tek suggestion log olsun.
        //
        // QuizRecommendationItemId nullable olduğu için SQL Server null değerlere izin verir.
        // Dolu olduğu durumda duplicate oluşmasını engeller.
        builder.HasIndex(log => log.QuizRecommendationItemId)
            .IsUnique()
            .HasFilter("[QuizRecommendationItemId] IS NOT NULL");

        // LearningItem geçmişi korunmalıdır.
        // Bu yüzden cascade delete kullanmıyoruz.
        builder.HasOne<LearningItem>()
            .WithMany()
            .HasForeignKey(log => log.LearningItemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Suggestion log bir quiz session üzerinden oluşmuş olabilir.
        // QuizSession fiziksel silinirse logların otomatik silinmesini istemiyoruz.
        builder.HasOne<QuizSession>()
            .WithMany()
            .HasForeignKey(log => log.QuizSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Suggestion log bir QuizRecommendationItem ile ilişkili olabilir.
        // Analytics kaydı olduğu için cascade delete tercih etmiyoruz.
        builder.HasOne<QuizRecommendationItem>()
            .WithMany()
            .HasForeignKey(log => log.QuizRecommendationItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}