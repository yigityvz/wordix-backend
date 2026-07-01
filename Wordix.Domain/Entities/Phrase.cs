using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sistemdeki global phrase / kalıp ifade kaydını temsil eder.
/// 
/// Phrase kullanıcıya özel değildir.
/// Aynı phrase her kullanıcı için tekrar tekrar oluşturulmaz.
/// Kullanıcı phrase'i kendi dictionary'sine eklediğinde UserLearningItem oluşur.
/// 
/// Örnek phrase'ler:
/// - give up
/// - look after
/// - by the way
/// - take care of
/// </summary>
public class Phrase : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// Domain tarafında dışarıdan boş Phrase oluşturulmasını istemiyoruz.
    /// EF Core ise entity materialization sırasında bu constructor'ı kullanabilir.
    /// </summary>
    protected Phrase()
    {
    }

    /// <summary>
    /// Yeni global phrase kaydı oluşturur.
    /// 
    /// Phrase, Word gibi doğrudan kullanıcıya bağlı değildir.
    /// Önce global içerik havuzuna eklenir.
    /// Kullanıcı bunu kaydetmek isterse UserLearningItem ile kendi dictionary'sine bağlanır.
    /// </summary>
    public Phrase(
        Guid learningItemId,
        string text,
        string normalizedText,
        PhraseType phraseType = PhraseType.Unknown)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Phrase metni boş olamaz.", nameof(text));
        }

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new ArgumentException("Normalize edilmiş phrase metni boş olamaz.", nameof(normalizedText));
        }

        LearningItemId = learningItemId;

        // Kullanıcıya gösterilecek orijinal metni trimleyerek saklıyoruz.
        Text = text.Trim();

        // Arama ve unique kontrol için normalize edilmiş metni küçük harfe çeviriyoruz.
        NormalizedText = normalizedText.Trim().ToLowerInvariant();

        PhraseType = phraseType;
    }

    /// <summary>
    /// Bu phrase'in bağlı olduğu LearningItem Id'sidir.
    /// 
    /// Phrase entity'si detay tablodur.
    /// Quiz, dictionary, progress ve analytics gibi ortak mekanizmalar
    /// doğrudan Phrase'e değil LearningItem'a bağlanır.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Phrase'in kullanıcıya gösterilecek orijinal halidir.
    /// 
    /// Örnek:
    /// Give up, look after, by the way
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Arama ve unique kontrol için normalize edilmiş phrase değeridir.
    /// 
    /// Örnek:
    /// " Give Up " → "give up"
    /// </summary>
    public string NormalizedText { get; private set; } = string.Empty;

    /// <summary>
    /// Phrase'in türüdür.
    /// 
    /// Örnek:
    /// PhrasalVerb, Idiom, Expression, Collocation
    /// 
    /// İlk aşamada provider/import sistemi her phrase için bunu net bilemeyebilir.
    /// Bu yüzden Unknown değeri güvenli default olarak kullanılabilir.
    /// </summary>
    public PhraseType PhraseType { get; private set; } = PhraseType.Unknown;

    /// <summary>
    /// Phrase türünü günceller.
    /// 
    /// Örneğin provider ilk başta Unknown getirmiş olabilir.
    /// Daha sonra admin veya import doğrulama süreci bu phrase'i
    /// PhrasalVerb, Idiom veya Expression olarak işaretleyebilir.
    /// </summary>
    public void ChangePhraseType(PhraseType phraseType)
    {
        if (PhraseType == phraseType)
        {
            return;
        }

        PhraseType = phraseType;
        MarkAsUpdated();
    }
}