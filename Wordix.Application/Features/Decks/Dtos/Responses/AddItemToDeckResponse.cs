namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Deck'e item başarıyla eklendiğinde dönecek response modelidir.
/// </summary>
public sealed class AddItemToDeckResponse
{
    /// <summary>
    /// DeckItem entity id değeridir.
    /// </summary>
    public Guid DeckItemId { get; init; }

    /// <summary>
    /// Item'ın eklendiği deck id değeridir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Deck'e eklenen kullanıcının dictionary item id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Item'ın deck'e eklendiği zaman.
    /// </summary>
    public DateTime AddedAt { get; init; }
}