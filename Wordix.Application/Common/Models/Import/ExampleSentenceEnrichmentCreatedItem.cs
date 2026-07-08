using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence enrichment sırasında oluşturulan
/// veya dry-run modunda oluşturulması planlanan tek bir bağlantıyı temsil eder.
/// 
/// Bu model API response tarafında sample olarak da kullanılabilir.
/// Entity değildir.
/// </summary>
public sealed record ExampleSentenceEnrichmentCreatedItem
{
    /// <summary>
    /// Örnek cümlenin bağlandığı LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// LearningItem tipi.
    /// 
    /// Örnek:
    /// Word, Phrase
    /// </summary>
    public LearningItemType ItemType { get; init; }

    /// <summary>
    /// Eşleşen Word/Phrase metnidir.
    /// 
    /// Örnek:
    /// sleep
    /// give up
    /// </summary>
    public string MatchedText { get; init; } = string.Empty;

    /// <summary>
    /// Eşleşen normalize Word/Phrase metnidir.
    /// </summary>
    public string NormalizedMatchedText { get; init; } = string.Empty;

    /// <summary>
    /// Tatoeba kaynak cümle external id değeridir.
    /// </summary>
    public string SourceSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Tatoeba hedef/çeviri cümle external id değeridir.
    /// </summary>
    public string TargetSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// İngilizce kaynak cümle metnidir.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Türkçe çeviri cümle metnidir.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Oluşturulan veya mevcut kullanılan Sentence id değeridir.
    /// 
    /// DryRun modunda null olabilir.
    /// </summary>
    public Guid? SentenceId { get; init; }

    /// <summary>
    /// Oluşturulan veya mevcut kullanılan SentenceTranslation id değeridir.
    /// 
    /// DryRun modunda null olabilir.
    /// </summary>
    public Guid? SentenceTranslationId { get; init; }

    /// <summary>
    /// Oluşturulan LearningItemExampleSentence id değeridir.
    /// 
    /// DryRun modunda null olabilir.
    /// </summary>
    public Guid? LearningItemExampleSentenceId { get; init; }

    /// <summary>
    /// Bu işlemde yeni Sentence oluşturuldu mu?
    /// </summary>
    public bool CreatedSentence { get; init; }

    /// <summary>
    /// Bu işlemde yeni SentenceTranslation oluşturuldu mu?
    /// </summary>
    public bool CreatedSentenceTranslation { get; init; }

    /// <summary>
    /// Bu işlemde yeni LearningItemExampleSentence bağlantısı oluşturuldu mu?
    /// </summary>
    public bool CreatedExampleLink { get; init; }
}