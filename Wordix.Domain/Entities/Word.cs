using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sistemdeki global kelime kaydını temsil eder.
/// 
/// Word kullanıcıya özel değildir.
/// Aynı kelime her kullanıcı için tekrar tekrar oluşturulmaz.
/// Kullanıcı kelimeyi kendi dictionary'sine eklediğinde UserLearningItem oluşur.
/// </summary>
public class Word : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected Word()
    {
    }


    /// <summary>
    /// Yeni global kelime kaydı oluşturur.
    /// </summary>
    public Word(
        Guid learningItemId,
        string text,
        string normalizedText,
        string? partOfSpeech = null,
        string? pronunciation = null)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Kelime metni boş olamaz.", nameof(text));
        }

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new ArgumentException("Normalize edilmiş kelime metni boş olamaz.", nameof(normalizedText));
        }

        LearningItemId = learningItemId;
        Text = text.Trim();
        NormalizedText = normalizedText.Trim().ToLowerInvariant();
        PartOfSpeech = string.IsNullOrWhiteSpace(partOfSpeech) ? null : partOfSpeech.Trim();
        Pronunciation = string.IsNullOrWhiteSpace(pronunciation) ? null : pronunciation.Trim();
    }

    /// <summary>
    /// Bu kelimenin bağlı olduğu LearningItem Id'sidir.
    /// 
    /// Word entity'si detay tablodur.
    /// Quiz ve dictionary gibi ortak mekanizmalar LearningItem üzerinden çalışır.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Kelimenin kullanıcıya gösterilecek orijinal halidir.
    /// Örnek: Achieve, perfect, improve
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Arama ve unique kontrol için normalize edilmiş kelime değeridir.
    /// 
    /// Örnek:
    /// " Achieve " → "achieve"
    /// </summary>
    public string NormalizedText { get; private set; } = string.Empty;

    /// <summary>
    /// Kelimenin türüdür.
    /// Örnek:
    /// noun, verb, adjective
    /// 
    /// İlk prototipte string tutuyoruz.
    /// İleride ihtiyaç olursa PartOfSpeech enum veya ayrı entity yapılabilir.
    /// </summary>
    public string? PartOfSpeech { get; private set; }

    /// <summary>
    /// Telaffuz bilgisidir.
    /// Örnek:
    /// /əˈtʃiːv/
    /// </summary>
    public string? Pronunciation { get; private set; }

    
    /// <summary>
    /// Kelimenin açıklayıcı bilgilerini günceller.
    /// 
    /// Text ve NormalizedText burada güncellenmiyor.
    /// Çünkü kelimenin ana metnini değiştirmek daha hassas bir işlemdir.
    /// </summary>
    public void UpdateDetails(string? partOfSpeech, string? pronunciation)
    {
        PartOfSpeech = string.IsNullOrWhiteSpace(partOfSpeech) ? null : partOfSpeech.Trim();
        Pronunciation = string.IsNullOrWhiteSpace(pronunciation) ? null : pronunciation.Trim();

        MarkAsUpdated();
    }
}