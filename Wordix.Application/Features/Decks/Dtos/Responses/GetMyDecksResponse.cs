namespace Wordix.Application.Features.Decks.Dtos.Responses;

/// <summary>
/// Kullanıcının deck listesini dönen response modelidir.
/// </summary>
public sealed class GetMyDecksResponse
{
    /// <summary>
    /// Kullanıcının toplam aktif deck sayısıdır.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Kullanıcının deck listesi.
    /// </summary>
    public IReadOnlyCollection<DeckSummaryResponse> Decks { get; init; }
        = Array.Empty<DeckSummaryResponse>();
}