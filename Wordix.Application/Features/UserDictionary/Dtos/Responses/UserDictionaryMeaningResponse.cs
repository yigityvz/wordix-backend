namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary listesindeki bir learning item'a ait anlam bilgisini temsil eder.
/// 
/// Bu response neden var?
/// - Bir LearningItem'ın birden fazla Meaning kaydı olabilir.
/// - Kullanıcı dictionary ekranında seçili/ana anlamı görebilmeli.
/// - İleride detay endpointinde tüm anlamlar da gösterilebilir.
/// </summary>
public sealed class UserDictionaryMeaningResponse
{
    /// <summary>
    /// Meaning entity id değeridir.
    /// </summary>
    public Guid MeaningId { get; init; }

    /// <summary>
    /// Hedef dildeki anlam/çeviri bilgisidir.
    /// 
    /// Örnek:
    /// achieve → başarmak
    /// study → çalışmak
    /// </summary>
    public string Translation { get; init; } = string.Empty;

    /// <summary>
    /// Kısa tanım bilgisidir.
    /// İlk prototipte null olabilir.
    /// </summary>
    public string? Definition { get; init; }

    /// <summary>
    /// Kelime türüdür.
    /// Örnek:
    /// verb, noun, adjective
    /// </summary>
    public string? PartOfSpeech { get; init; }

    /// <summary>
    /// Bu anlam primary meaning mi?
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Listeleme sırası.
    /// </summary>
    public int DisplayOrder { get; init; }
}
