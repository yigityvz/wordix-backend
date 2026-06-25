namespace Wordix.Application.Common.Exceptions;

/// <summary>
/// İstenen kaydın sistemde bulunamadığı durumlarda kullanılan exception'dır.
/// 
/// Örnek kullanım:
/// - UserProfile bulunamadı.
/// - LearningItem bulunamadı.
/// - QuizSession bulunamadı.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Bulunamayan kaynağın adıdır.
    /// Örnek:
    /// - UserProfile
    /// - LearningItem
    /// - QuizSession
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Aranan kaydın anahtar değeridir.
    /// Örnek:
    /// - Guid id
    /// - email
    /// - normalized word text
    /// </summary>
    public object? ResourceKey { get; }

    /// <summary>
    /// Sadece message ile exception oluşturmak için kullanılır.
    /// </summary>
    public NotFoundException(string message)
        : base(message)
    {
        ResourceName = string.Empty;
    }

    /// <summary>
    /// Kaynak adı ve aranan değer ile daha açıklayıcı exception oluşturur.
    /// </summary>
    public NotFoundException(string resourceName, object? resourceKey)
        : base($"{resourceName} was not found. Key: {resourceKey}")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }
}