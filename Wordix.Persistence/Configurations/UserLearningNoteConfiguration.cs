using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wordix.Domain.Entities;

namespace Wordix.Persistence.Configurations;

/// <summary>
/// UserLearningNote entity'sinin EF Core tablo yapılandırmasıdır.
/// 
/// Bu configuration ne yapar?
/// - Tablo adını belirler.
/// - Primary key tanımlar.
/// - NoteText alanına zorunluluk ve uzunluk sınırı koyar.
/// - UserLearningItem ilişkisini kurar.
/// - UserLearningItemId index'i ekler.
/// 
/// Neden Persistence katmanında?
/// - EF Core ve database detayları Domain katmanına ait değildir.
/// - Domain entity sadece iş kuralını ve state'i temsil eder.
/// </summary>
public sealed class UserLearningNoteConfiguration
    : IEntityTypeConfiguration<UserLearningNote>
{
    /// <summary>
    /// Kullanıcı notu için maksimum metin uzunluğu.
    /// 
    /// İlk prototip için 2000 karakter yeterlidir.
    /// İleride kullanıcı deneyimine göre artırılabilir.
    /// </summary>
    private const int NoteTextMaxLength = 2_000;

    public void Configure(EntityTypeBuilder<UserLearningNote> builder)
    {
        builder.ToTable("UserLearningNotes");

        builder.HasKey(note => note.Id);

        builder.Property(note => note.UserLearningItemId)
            .IsRequired();

        builder.Property(note => note.NoteText)
            .IsRequired()
            .HasMaxLength(NoteTextMaxLength);

        builder.HasIndex(note => note.UserLearningItemId);

        // UserLearningNote doğrudan KeycloakUserId tutmaz.
        // Ownership şu zincirle doğrulanır:
        // UserLearningNote -> UserLearningItem -> KeycloakUserId
        builder.HasOne<UserLearningItem>()
            .WithMany()
            .HasForeignKey(note => note.UserLearningItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}