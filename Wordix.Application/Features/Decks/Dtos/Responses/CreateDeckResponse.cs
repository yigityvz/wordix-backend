namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Deck başarıyla oluşturulduğunda dönecek response modelidir.
/// </summary>
public sealed class CreateDeckResponse
{
    /// <summary>
    /// Oluşturulan deck id değeridir.
    /// </summary>
    public Guid DeckId { get; init; }

    /// <summary>
    /// Deck adıdır.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş deck adıdır.
    /// </summary>
    public string NormalizedName { get; init; } = string.Empty;

    /// <summary>
    /// Deck açıklamasıdır.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Oluşturulma zamanı.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Deck aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}