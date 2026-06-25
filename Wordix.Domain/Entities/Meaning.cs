using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Word veya Phrase gibi LearningItem içeriklerinin anlamlarını temsil eder.
/// 
/// Bir LearningItem'ın birden fazla Meaning kaydı olabilir.
/// Örneğin "perfect" kelimesi:
/// - mükemmel
/// - kusursuz
/// - harika
/// anlamlarına sahip olabilir.
/// </summary>
public class Meaning : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected Meaning()
    {
    }


    /// <summary>
    /// Yeni anlam kaydı oluşturur.
    /// </summary>
    public Meaning(
        Guid learningItemId,
        Guid targetLanguageId,
        string meaningText,
        string? shortDefinition = null,
        string? partOfSpeech = null,
        string? category = null,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (targetLanguageId == Guid.Empty)
        {
            throw new ArgumentException("TargetLanguageId boş Guid olamaz.", nameof(targetLanguageId));
        }

        if (string.IsNullOrWhiteSpace(meaningText))
        {
            throw new ArgumentException("Anlam metni boş olamaz.", nameof(meaningText));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        LearningItemId = learningItemId;
        TargetLanguageId = targetLanguageId;
        MeaningText = meaningText.Trim();
        ShortDefinition = string.IsNullOrWhiteSpace(shortDefinition) ? null : shortDefinition.Trim();
        PartOfSpeech = string.IsNullOrWhiteSpace(partOfSpeech) ? null : partOfSpeech.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Anlamın hangi LearningItem'a ait olduğunu gösterir.
    /// 
    /// Meaning doğrudan Word'e değil LearningItem'a bağlıdır.
    /// Böylece ileride Phrase anlamları da aynı tabloyla tutulabilir.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Anlamın hedef dilini gösterir.
    /// Örneğin İngilizce kelimenin Türkçe anlamı için Turkish language Id tutulur.
    /// </summary>
    public Guid TargetLanguageId { get; private set; }

    /// <summary>
    /// Kullanıcıya gösterilecek anlam metnidir.
    /// Örnek: başarmak, geliştirmek, mükemmel
    /// </summary>
    public string MeaningText { get; private set; } = string.Empty;

    /// <summary>
    /// Kısa açıklama veya tanım bilgisidir.
    /// İlk prototipte boş kalabilir.
    /// </summary>
    public string? ShortDefinition { get; private set; }

    /// <summary>
    /// Anlamın kelime türüyle ilişkili açıklamasıdır.
    /// Örnek: verb, noun, adjective
    /// 
    /// İlk prototipte string tutuyoruz.
    /// İleride enum veya ayrı tabloya dönüştürülebilir.
    /// </summary>
    public string? PartOfSpeech { get; private set; }

    /// <summary>
    /// Anlam kategorisidir.
    /// Örnek: daily, business, software, formal
    /// İlk prototipte opsiyonel tutulur.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Bu anlam ana anlam mı?
    /// 
    /// Örneğin "achieve" için "başarmak" ana anlam olabilir.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Anlamların kullanıcıya hangi sırayla gösterileceğini belirler.
    /// </summary>
    public int DisplayOrder { get; private set; }

 
    /// <summary>
    /// Anlam içeriğini günceller.
    /// </summary>
    public void UpdateMeaning(
        string meaningText,
        string? shortDefinition,
        string? partOfSpeech,
        string? category)
    {
        if (string.IsNullOrWhiteSpace(meaningText))
        {
            throw new ArgumentException("Anlam metni boş olamaz.", nameof(meaningText));
        }

        MeaningText = meaningText.Trim();
        ShortDefinition = string.IsNullOrWhiteSpace(shortDefinition) ? null : shortDefinition.Trim();
        PartOfSpeech = string.IsNullOrWhiteSpace(partOfSpeech) ? null : partOfSpeech.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Bu anlamı ana anlam olarak işaretler.
    /// 
    /// Not:
    /// Bir LearningItem için sadece bir Meaning primary olmalı.
    /// Bu kuralı ileride Application/Persistence tarafında kontrol edeceğiz.
    /// </summary>
    public void MarkAsPrimary()
    {
        IsPrimary = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Bu anlamın ana anlam işaretini kaldırır.
    /// </summary>
    public void UnmarkAsPrimary()
    {
        IsPrimary = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Görüntüleme sırasını değiştirir.
    /// </summary>
    public void ChangeDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }
}