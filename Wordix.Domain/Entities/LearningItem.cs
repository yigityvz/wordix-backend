using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Word, Phrase ve Sentence gibi öğrenilebilir içeriklerin ortak çatısıdır.
/// 
/// İlk prototipte sadece Word aktif kullanılacaktır.
/// Ancak quiz, dictionary, review ve analytics sistemleri doğrudan Word'e değil,
/// LearningItem'a bağlı ilerleyecektir.
/// 
/// Bu sayede ileride Phrase veya Sentence eklendiğinde mevcut quiz/dictionary yapısını
/// baştan değiştirmek zorunda kalmayız.
/// </summary>
public class LearningItem : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// Dışarıdan boş LearningItem oluşturulmasını istemiyoruz.
    /// </summary>
    protected LearningItem()
    {
    }

    /// <summary>
    /// Yeni bir öğrenilebilir içerik oluşturur.
    /// 
    /// İlk prototipte ItemType genellikle Word olacaktır.
    /// </summary>
    public LearningItem(
        LearningItemType itemType,
        Guid languageId,
        CefrLevel cefrLevel,
        DifficultyGroup difficultyGroup,
        LearningItemSourceType sourceType)
    {
        if (languageId == Guid.Empty)
        {
            throw new ArgumentException("LanguageId boş Guid olamaz.", nameof(languageId));
        }

        ItemType = itemType;
        LanguageId = languageId;
        CefrLevel = cefrLevel;
        DifficultyGroup = difficultyGroup;
        SourceType = sourceType;
    }

    /// <summary>
    /// Öğrenilebilir içeriğin tipidir.
    /// Örnek:
    /// Word, Phrase, Sentence
    /// </summary>
    public LearningItemType ItemType { get; private set; }

    /// <summary>
    /// İçeriğin hangi dile ait olduğunu gösterir.
    /// Örneğin İngilizce kelime için English language Id tutulur.
    /// </summary>
    public Guid LanguageId { get; private set; }

    /// <summary>
    /// İçeriğin CEFR seviyesidir.
    /// Örnek: A1, A2, B1, B2, C1, C2
    /// </summary>
    public CefrLevel CefrLevel { get; private set; } = CefrLevel.Unknown;

    /// <summary>
    /// Kullanıcıya daha sade gösterilecek zorluk grubudur.
    /// Örnek: Beginner, Intermediate, Hard
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; private set; } = DifficultyGroup.Unknown;

    /// <summary>
    /// Bu içeriğin sisteme hangi yolla eklendiğini gösterir.
    /// Örnek:
    /// - SystemSeed
    /// - UserLookup
    /// - Provider
    /// - Import
    /// - AdminCreated
    /// </summary>
    public LearningItemSourceType SourceType { get; private set; } = LearningItemSourceType.Unknown;

    /// <summary>
    /// İçerik uygulamada aktif kullanılabilir mi?
    /// 
    /// Örneğin bir kelime sistemde durabilir ama admin tarafından geçici olarak pasif yapılabilir.
    /// </summary>
    public bool IsActive { get; private set; } = true;


    /// <summary>
    /// İçeriğin seviye ve zorluk bilgisini günceller.
    /// 
    /// Örneğin provider'dan gelen içerik sonradan admin tarafından daha doğru seviyeye çekilebilir.
    /// </summary>
    public void ChangeLevel(CefrLevel cefrLevel, DifficultyGroup difficultyGroup)
    {
        CefrLevel = cefrLevel;
        DifficultyGroup = difficultyGroup;

        MarkAsUpdated();
    }

    /// <summary>
    /// İçeriği uygulamada aktif hale getirir.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// İçeriği uygulamada pasif hale getirir.
    /// 
    /// Bu fiziksel silme değildir.
    /// Analytics ve geçmiş kayıtlar korunur.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsUpdated();
    }
}