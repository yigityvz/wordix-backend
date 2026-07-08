namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence enrichment sırasında oluşan bilgi/uyarı mesajını temsil eder.
/// 
/// Bu model neden var?
/// - Büyük importlarda her satır başarılı olmayabilir.
/// - Bazı satırlar eşleşmeyebilir.
/// - Bazı satırlar duplicate olabilir.
/// - Bazı satırlar validation nedeniyle atlanabilir.
/// 
/// Bunları sadece string listesiyle taşımak yerine küçük bir modelle taşımak
/// ileride ImportJob loglarına geçmeyi kolaylaştırır.
/// </summary>
public sealed record ExampleSentenceEnrichmentMessage
{
    /// <summary>
    /// Mesaj tipi.
    /// 
    /// Örnek:
    /// Info, Warning, Error
    /// </summary>
    public string Type { get; init; } = "Info";

    /// <summary>
    /// Mesaj kodu.
    /// 
    /// Örnek:
    /// NO_MATCH_FOUND
    /// DUPLICATE_EXAMPLE_LINK
    /// LANGUAGE_NOT_FOUND
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// İnsan tarafından okunabilir mesaj metni.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// İlgili Tatoeba source sentence external id değeri.
    /// Her mesaj için dolu olmak zorunda değildir.
    /// </summary>
    public string? SourceSentenceExternalId { get; init; }

    /// <summary>
    /// İlgili Tatoeba target sentence external id değeri.
    /// Her mesaj için dolu olmak zorunda değildir.
    /// </summary>
    public string? TargetSentenceExternalId { get; init; }

    /// <summary>
    /// İlgili LearningItem id değeri.
    /// Her mesaj için dolu olmak zorunda değildir.
    /// </summary>
    public Guid? LearningItemId { get; init; }
}