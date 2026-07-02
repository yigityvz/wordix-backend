using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının kendi dictionary item'ına eklediği kişisel notu temsil eder.
/// 
/// Bu entity global LearningItem'a bağlı değildir.
/// Çünkü not kullanıcıya özeldir.
/// 
/// Doğru ilişki:
/// UserLearningItem -> UserLearningNote
/// 
/// Örnek:
/// Kullanıcı "achieve" kelimesi için şunu yazabilir:
/// "Bunu hedefe ulaşmak gibi düşüneceğim."
/// 
/// Neden ayrı entity?
/// - Bir UserLearningItem için birden fazla not tutulabilir.
/// - Notların CreatedAt / UpdatedAt bilgisi ayrı takip edilebilir.
/// - İleride not arama, not geçmişi veya analytics eklemek kolaylaşır.
/// </summary>
public class UserLearningNote : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan boş ve geçersiz note oluşturulmasını istemediğimiz için protected bırakıyoruz.
    /// </summary>
    protected UserLearningNote()
    {
    }

    /// <summary>
    /// Yeni kullanıcı notu oluşturur.
    /// 
    /// userLearningItemId:
    /// - Kullanıcının kişisel dictionary item kaydıdır.
    /// - Global LearningItemId değildir.
    /// 
    /// noteText:
    /// - Kullanıcının yazdığı kişisel nottur.
    /// </summary>
    public UserLearningNote(
        Guid userLearningItemId,
        string noteText)
    {
        if (userLearningItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserLearningItemId boş Guid olamaz.",
                nameof(userLearningItemId));
        }

        if (string.IsNullOrWhiteSpace(noteText))
        {
            throw new ArgumentException(
                "NoteText boş olamaz.",
                nameof(noteText));
        }

        UserLearningItemId = userLearningItemId;
        NoteText = noteText.Trim();
    }

    /// <summary>
    /// Notun bağlı olduğu kullanıcı dictionary item id değeridir.
    /// 
    /// Ownership doğrudan burada KeycloakUserId tutularak değil,
    /// UserLearningItem -> KeycloakUserId zinciriyle doğrulanır.
    /// </summary>
    public Guid UserLearningItemId { get; private set; }

    /// <summary>
    /// Kullanıcının yazdığı kişisel not metnidir.
    /// </summary>
    public string NoteText { get; private set; } = string.Empty;

    /// <summary>
    /// Mevcut not metnini günceller.
    /// 
    /// Bu method neden entity içinde?
    /// - NoteText entity'nin kendi state'idir.
    /// - Geçersiz boş not oluşmasını entity seviyesinde de engelleriz.
    /// - UpdatedAt değerini AuditableEntity üzerinden güncel tutarız.
    /// </summary>
    public void UpdateText(string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
        {
            throw new ArgumentException(
                "NoteText boş olamaz.",
                nameof(noteText));
        }

        var normalizedNoteText = noteText.Trim();

        if (string.Equals(
                NoteText,
                normalizedNoteText,
                StringComparison.Ordinal))
        {
            return;
        }

        NoteText = normalizedNoteText;

        MarkAsUpdated();
    }
}