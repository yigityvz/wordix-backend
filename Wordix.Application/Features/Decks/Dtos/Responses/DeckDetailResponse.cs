namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Tek bir deck'in detay response modelidir.
/// 
/// GET /api/decks/{id} endpointinde kullanılır.
/// </summary>
public sealed class DeckDetailResponse
{
    /// <summary>
    /// Deck id değeridir.
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
    /// Deck içindeki item sayısıdır.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Deck içindeki itemlar.
    /// </summary>
    public IReadOnlyCollection<DeckItemResponse> Items { get; init; }
        = Array.Empty<DeckItemResponse>();

    /// <summary>
    /// Deck oluşturulma zamanı.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Deck son güncellenme zamanı.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Deck aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}