using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence enrichment sırasında Tatoeba cümleleriyle eşleştirilebilecek
/// Word/Phrase LearningItem adayını temsil eder.
/// 
/// Bu model entity değildir.
/// Persistence katmanındaki özel repository sorgusundan Application katmanına dönen
/// sade veri taşıma modelidir.
/// 
/// Neden gerekli?
/// - Tatoeba cümlesinin içinde hangi Word/Phrase geçiyor bunu bulmamız gerekir.
/// - Bunun için LearningItem + Word/Phrase verisini birlikte bilmeliyiz.
/// - Application katmanı DbContext veya join detayı bilmemelidir.
/// </summary>
public sealed class ExampleSentenceLearningItemCandidate
{
    /// <summary>
    /// Global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Aday içerik Word mü Phrase mi?
    /// </summary>
    public LearningItemType ItemType { get; init; }

    /// <summary>
    /// Word veya Phrase id değeridir.
    /// 
    /// Word adayıysa WordId,
    /// Phrase adayıysa PhraseId burada tutulur.
    /// </summary>
    public Guid ContentId { get; init; }

    /// <summary>
    /// Kullanıcıya gösterilecek Word/Phrase metnidir.
    /// 
    /// Örnek:
    /// sleep
    /// give up
    /// take care of
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Eşleştirme için normalize edilmiş Word/Phrase metnidir.
    /// 
    /// Örnek:
    /// Sleep -> sleep
    /// Give Up -> give up
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// LearningItem'ın kaynak dil id değeridir.
    /// </summary>
    public Guid LanguageId { get; init; }

    /// <summary>
    /// LearningItem'ın kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string LanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// İçeriğin gerçek kaynağıdır.
    /// 
    /// Örnek:
    /// CefrJ, Octanove, AzureTranslator.
    /// </summary>
    public ContentSource ContentSource { get; init; }

    /// <summary>
    /// İçeriğin kalite durumudur.
    /// </summary>
    public ContentQualityStatus QualityStatus { get; init; }
}