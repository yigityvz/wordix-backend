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
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile tablosu üzerinden kullanıcı sahipliği kurmaz.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - QuizSessions tablosu kullanıcıyı doğrudan KeycloakUserId ile ilişkilendirir.
/// </summary>
public class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
{
    public void Configure(EntityTypeBuilder<QuizSession> builder)
    {
        builder.ToTable("QuizSessions");

        builder.ConfigureAuditableEntity();

        // KeycloakUserId, JWT token içindeki "sub" claiminden gelen kullanıcı id değeridir.
        // Backend bu kullanıcı için ayrıca UserProfileId/UserId üretmez.
        // Bu alan dış identity provider id'si olduğu için string olarak tutulur.
        builder.Property(session => session.KeycloakUserId)
            .HasMaxLength(100)
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
        // Eski yapı UserProfileId ile çalışıyordu.
        // Yeni yapı KeycloakUserId ile çalışır.
        builder.HasIndex(session => session.KeycloakUserId);

        // Kullanıcının quiz geçmişini tarih sırasına göre çekmek için faydalı index.
        // Örnek sorgu:
        // WHERE KeycloakUserId = '...' ORDER BY StartedAt DESC
        builder.HasIndex(session => new
        {
            session.KeycloakUserId,
            session.StartedAt
        });

        // Devam eden quizleri bulmak için kullanılabilir.
        builder.HasIndex(session => session.Status);

        // Önemli:
        // Burada artık UserProfile foreign key ilişkisi yok.
        // Çünkü Keycloak kullanıcısı bizim SQL database'imizde bir tablo olarak tutulmuyor.
        // KeycloakUserId dış identity provider id'si olarak saklanır.
    }
}