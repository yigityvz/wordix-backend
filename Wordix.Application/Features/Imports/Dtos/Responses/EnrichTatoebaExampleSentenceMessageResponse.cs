namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Tatoeba enrichment sırasında oluşan bilgi/uyarı/hata mesajı response modelidir.
/// </summary>
public sealed record EnrichTatoebaExampleSentenceMessageResponse
{
    /// <summary>
    /// Mesaj tipi.
    /// 
    /// Örnek:
    /// Info, Warning, Error
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Mesaj kodu.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// İnsan tarafından okunabilir mesaj metni.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// İlgili Tatoeba source sentence external id değeri.
    /// </summary>
    public string? SourceSentenceExternalId { get; init; }

    /// <summary>
    /// İlgili Tatoeba target sentence external id değeri.
    /// </summary>
    public string? TargetSentenceExternalId { get; init; }

    /// <summary>
    /// İlgili LearningItem id değeri.
    /// </summary>
    public Guid? LearningItemId { get; init; }
}