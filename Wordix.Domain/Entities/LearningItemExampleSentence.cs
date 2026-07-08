using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Bir Word veya Phrase LearningItem ile örnek Sentence arasındaki ilişkiyi temsil eder.
/// 
/// Bu entity neden gerekli?
/// Sentence.LearningItemId farklı bir anlam taşır:
/// - Sentence kendi başına öğrenilecek bir içerikse LearningItemId dolu olur.
/// - Örnek cümle olarak kullanılan Sentence ise LearningItemId null kalabilir.
/// 
/// Ama Word/Phrase için örnek cümle göstermek istediğimizde şu ilişkiye ihtiyacımız var:
/// - abandon kelimesinin örnek cümlesi hangisi?
/// - take care of phrase'inin örnek cümlesi hangisi?
/// 
/// Bu yüzden doğrudan Sentence.LearningItemId kullanmak yerine
/// ayrı bir bağlantı entity'si oluşturuyoruz.
/// </summary>
public class LearningItemExampleSentence : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected LearningItemExampleSentence()
    {
    }

    /// <summary>
    /// Yeni LearningItem örnek cümle bağlantısı oluşturur.
    /// </summary>
    public LearningItemExampleSentence(
        Guid learningItemId,
        Guid sentenceId,
        Guid? sentenceTranslationId = null,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (sentenceId == Guid.Empty)
        {
            throw new ArgumentException("SentenceId boş Guid olamaz.", nameof(sentenceId));
        }

        if (sentenceTranslationId == Guid.Empty)
        {
            throw new ArgumentException(
                "SentenceTranslationId boş Guid olamaz. Null veya geçerli Guid olmalıdır.",
                nameof(sentenceTranslationId));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        LearningItemId = learningItemId;
        SentenceId = sentenceId;
        SentenceTranslationId = sentenceTranslationId;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Örnek cümlenin ait olduğu Word/Phrase LearningItem id değeridir.
    /// 
    /// Not:
    /// Bu genellikle Word veya Phrase tipindeki LearningItem olur.
    /// Sentence tipindeki LearningItem için bu ilişki çoğunlukla kullanılmaz.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Örnek olarak gösterilecek Sentence id değeridir.
    /// 
    /// Bu sentence kendi başına LearningItem olmak zorunda değildir.
    /// </summary>
    public Guid SentenceId { get; private set; }

    /// <summary>
    /// Bu örnek cümle için tercih edilen çeviri id değeridir.
    /// 
    /// Null olabilir.
    /// Çünkü örnek cümle bulunmuş ama çevirisi henüz bulunamamış olabilir.
    /// </summary>
    public Guid? SentenceTranslationId { get; private set; }

    /// <summary>
    /// Bu LearningItem için ana örnek cümle mi?
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Birden fazla örnek cümle varsa gösterim sırasıdır.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Örnek cümleyi primary yapar.
    /// 
    /// Bir LearningItem için tek primary kuralı Application/Persistence tarafında korunacaktır.
    /// </summary>
    public void MarkAsPrimary()
    {
        if (IsPrimary)
        {
            return;
        }

        IsPrimary = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Primary işaretini kaldırır.
    /// </summary>
    public void UnmarkAsPrimary()
    {
        if (!IsPrimary)
        {
            return;
        }

        IsPrimary = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Gösterim sırasını değiştirir.
    /// </summary>
    public void ChangeDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        if (DisplayOrder == displayOrder)
        {
            return;
        }

        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    /// <summary>
    /// Örnek cümleye ait tercih edilen çeviriyi günceller.
    /// </summary>
    public void ChangeSentenceTranslation(Guid? sentenceTranslationId)
    {
        if (sentenceTranslationId == Guid.Empty)
        {
            throw new ArgumentException(
                "SentenceTranslationId boş Guid olamaz. Null veya geçerli Guid olmalıdır.",
                nameof(sentenceTranslationId));
        }

        if (SentenceTranslationId == sentenceTranslationId)
        {
            return;
        }

        SentenceTranslationId = sentenceTranslationId;
        MarkAsUpdated();
    }
}