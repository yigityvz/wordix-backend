using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Bir deck içindeki tek bir dictionary item bağlantısını temsil eder.
/// 
/// Önemli karar:
/// DeckItem doğrudan LearningItemId tutmaz.
/// DeckItem, UserLearningItemId tutar.
/// 
/// Neden?
/// - Deck kullanıcıya ait kişisel koleksiyondur.
/// - UserLearningItem zaten kullanıcının dictionary kaydıdır.
/// - Böylece deck'e sadece kullanıcının kendi dictionary itemları eklenebilir.
/// - Ownership kontrolü daha güvenli olur.
/// - İleride note/flag/progress/statistics akışları UserLearningItem üzerinden bağlanabilir.
/// 
/// Zincir:
/// DeckItem -> UserLearningItem -> LearningItem -> Word/Phrase/Sentence
/// </summary>
public class DeckItem : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected DeckItem()
    {
    }

    /// <summary>
    /// Deck'e yeni bir UserLearningItem bağlantısı ekler.
    /// </summary>
    public DeckItem(
        Guid deckId,
        Guid userLearningItemId)
    {
        if (deckId == Guid.Empty)
        {
            throw new ArgumentException("DeckId boş olamaz.", nameof(deckId));
        }

        if (userLearningItemId == Guid.Empty)
        {
            throw new ArgumentException("UserLearningItemId boş olamaz.", nameof(userLearningItemId));
        }

        DeckId = deckId;
        UserLearningItemId = userLearningItemId;
        AddedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Bu item'ın bağlı olduğu deck id değeridir.
    /// </summary>
    public Guid DeckId { get; private set; }

    /// <summary>
    /// Deck'e eklenen kullanıcının dictionary item id değeridir.
    /// 
    /// Bu global LearningItemId değildir.
    /// UserLearningItemId değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; private set; }

    /// <summary>
    /// Item'ın deck'e eklendiği zamandır.
    /// 
    /// UTC tutulur.
    /// Frontend gerekirse lokal saate çevirir.
    /// </summary>
    public DateTime AddedAt { get; private set; }
}