namespace Wordix.Application.Features.Decks.Dtos.Requests;

/// <summary>
/// Bir deck içine kullanıcının dictionary itemını eklemek için kullanılan request modelidir.
/// 
/// Önemli:
/// Buradaki id global LearningItemId değildir.
/// UserLearningItemId değeridir.
/// </summary>
public sealed class AddItemToDeckRequest
{
    /// <summary>
    /// Kullanıcının dictionary item id değeridir.
    /// 
    /// DeckItem doğrudan LearningItemId değil, UserLearningItemId üzerinden çalışır.
    /// Böylece sadece kullanıcının kendi dictionary'sindeki itemlar deck'e eklenebilir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }
}