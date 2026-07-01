namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Deck listeleme ekranında dönecek kısa deck bilgisidir.
/// 
/// GET /api/decks endpointinde kullanılır.
/// </summary>
public sealed class DeckSummaryResponse
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
    /// Duplicate kontrolü için kullanılır; frontend için bilgi amaçlı dönebilir.
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
    /// Deck oluşturulma zamanı.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Deck son güncellenme zamanı.
    /// Nullable olabilir.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Deck aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}