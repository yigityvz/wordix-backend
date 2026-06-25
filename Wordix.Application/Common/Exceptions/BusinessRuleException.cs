namespace Wordix.Application.Common.Exceptions;

/// <summary>
/// İş kuralı ihlallerinde kullanılan exception'dır.
/// 
/// Örnek:
/// - Kullanıcı aynı kelimeyi dictionary'ye ikinci kez kaydetmeye çalışır.
/// - Tamamlanmış quiz tekrar cevaplanmaya çalışılır.
/// - Aktif olmayan LearningItem quiz'e dahil edilmeye çalışılır.
/// </summary>
public sealed class BusinessRuleException : Exception
{
    /// <summary>
    /// Frontend veya log tarafında iş kuralını daha net ayırt etmek için opsiyonel kod.
    /// Örnek:
    /// - DICTIONARY_ITEM_ALREADY_EXISTS
    /// - QUIZ_ALREADY_COMPLETED
    /// </summary>
    public string? RuleCode { get; }

    /// <summary>
    /// Sadece mesaj ile business exception oluşturur.
    /// </summary>
    public BusinessRuleException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Mesaj ve ruleCode ile business exception oluşturur.
    /// </summary>
    public BusinessRuleException(string message, string ruleCode)
        : base(message)
    {
        RuleCode = ruleCode;
    }
}