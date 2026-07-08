namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Provider'dan gelen phrase'in global catalog'a kaydedilme sonucudur.
/// 
/// Bu result:
/// - Phrase gerçekten oluşturuldu mu?
/// - Zaten var mıydı?
/// - Oluşan LearningItem/Phrase/Meaning id'leri neler?
/// gibi bilgileri taşır.
/// </summary>
public sealed record ProviderPhraseCreationResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Yeni LearningItem + Phrase + Meaning oluşturuldu mu?
    /// </summary>
    public bool WasCreated { get; init; }

    /// <summary>
    /// Phrase zaten DB'de var olduğu için yeni kayıt oluşturulmadı mı?
    /// </summary>
    public bool AlreadyExists { get; init; }

    /// <summary>
    /// Oluşturulan veya ileride lookup ile bulunabilecek LearningItem id'si.
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

    /// <summary>
    /// Başarısız result üretir.
    /// </summary>
    public static ProviderPhraseCreationResult Failure(
        string normalizedText,
        string message)
    {
        return new ProviderPhraseCreationResult
        {
            Succeeded = false,
            WasCreated = false,
            AlreadyExists = false,
            LearningItemId = null,
            PhraseId = null,
            MeaningId = null,
            NormalizedText = normalizedText,
            Message = message
        };
    }

    /// <summary>
    /// Phrase zaten varsa result üretir.
    /// </summary>
    public static ProviderPhraseCreationResult Existing(
        string normalizedText,
        string message)
    {
        return new ProviderPhraseCreationResult
        {
            Succeeded = true,
            WasCreated = false,
            AlreadyExists = true,
            LearningItemId = null,
            PhraseId = null,
            MeaningId = null,
            NormalizedText = normalizedText,
            Message = message
        };
    }

    /// <summary>
    /// Başarılı oluşturma result'u üretir.
    /// </summary>
    public static ProviderPhraseCreationResult Created(
        Guid learningItemId,
        Guid phraseId,
        Guid meaningId,
        string normalizedText,
        string message)
    {
        return new ProviderPhraseCreationResult
        {
            Succeeded = true,
            WasCreated = true,
            AlreadyExists = false,
            LearningItemId = learningItemId,
            PhraseId = phraseId,
            MeaningId = meaningId,
            NormalizedText = normalizedText,
            Message = message
        };
    }
}