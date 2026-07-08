namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Meaning enrichment sırasında DB'de bulunan Word + LearningItem eşleşmesini temsil eder.
/// 
/// Bu model neden var?
/// - Enrichment service, Kaikki'den gelen "abandon -> terk etmek" satırını
///   DB'deki hangi LearningItem'a bağlayacağını bilmelidir.
/// - Application katmanı DbContext bilmez.
/// - Persistence repository'si DB join işlemini yapar ve sonucu bu sade modele çevirir.
/// </summary>
public sealed record MeaningEnrichmentWordMatch
{
    /// <summary>
    /// Meaning entity'sinin bağlanacağı LearningItem id'sidir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eşleşen Word entity id'sidir.
    /// Debug ve ileride logging için tutulur.
    /// </summary>
    public Guid WordId { get; init; }

    /// <summary>
    /// Word.NormalizedText değeridir.
    /// Kaikki MeaningImportRow.NormalizedSourceText ile eşleşir.
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Word.PartOfSpeech değeridir.
    /// İleride aynı kelime farklı POS ayrımı gerektiğinde kullanılabilir.
    /// </summary>
    public string? PartOfSpeech { get; init; }

    /// <summary>
    /// LearningItem.LanguageId değeridir.
    /// </summary>
    public Guid LanguageId { get; init; }
}