namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Deck'ten item çıkarıldığında dönecek response modelidir.
/// </summary>
public sealed class RemoveItemFromDeckResponse
{
    /// <summary>
    /// İşlem yapılan deck id değeridir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Deck'ten çıkarılan UserLearningItem id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Silinen DeckItem id değeridir.
    /// </summary>
    public Guid RemovedDeckItemId { get; init; }

    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Removed { get; init; }
}