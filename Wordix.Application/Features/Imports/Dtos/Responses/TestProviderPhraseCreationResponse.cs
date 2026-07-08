namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Provider-created phrase test endpoint response modelidir.
/// 
/// Bu response, global catalog'a phrase oluşturma işleminin sonucunu gösterir.
/// </summary>
public sealed record TestProviderPhraseCreationResponse
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Yeni kayıt oluşturuldu mu?
    /// </summary>
    public bool WasCreated { get; init; }

    /// <summary>
    /// Phrase zaten mevcut olduğu için yeni kayıt oluşturulmadı mı?
    /// </summary>
    public bool AlreadyExists { get; init; }

    /// <summary>
    /// Oluşturulan LearningItem id'si.
    /// </summary>
    public Guid? LearningItemId { get; init; }

    /// <summary>
    /// Oluşturulan Phrase id'si.
    /// </summary>
    public Guid? PhraseId { get; init; }

    /// <summary>
    /// Oluşturulan Meaning id'si.
    /// </summary>
    public Guid? MeaningId { get; init; }

    /// <summary>
    /// Normalize edilmiş phrase metni.
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// İşlem mesajı.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}