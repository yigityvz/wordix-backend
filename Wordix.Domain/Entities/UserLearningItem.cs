using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının kendi dictionary'sine kaydettiği öğrenilebilir içeriği temsil eder.
/// 
/// Bu entity doğrudan Word'e değil LearningItem'a bağlıdır.
/// Böylece kullanıcı ileride Word, Phrase veya Sentence kaydedebilir.
/// </summary>
public class UserLearningItem : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected UserLearningItem()
    {
    }

    /// <summary>
    /// Kullanıcı için yeni dictionary kaydı oluşturur.
    /// </summary>
    public UserLearningItem(
        Guid userProfileId,
        Guid learningItemId,
        Guid? selectedMeaningId = null,
        Guid? sourceLookupHistoryId = null)
    {
        if (userProfileId == Guid.Empty)
        {
            throw new ArgumentException("UserProfileId boş Guid olamaz.", nameof(userProfileId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        UserProfileId = userProfileId;
        LearningItemId = learningItemId;
        SelectedMeaningId = selectedMeaningId;
        SourceLookupHistoryId = sourceLookupHistoryId;
        SavedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// İçeriği kaydeden kullanıcı profil Id'sidir.
    /// </summary>
    public Guid UserProfileId { get; private set; }

    /// <summary>
    /// Kullanıcının kaydettiği LearningItem Id'sidir.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Kullanıcının özellikle seçtiği anlam Id'sidir.
    /// 
    /// Nullable olmasının sebebi:
    /// Kullanıcı bir kelimeyi genel olarak kaydedebilir,
    /// belirli bir anlam seçmeyebilir.
    /// </summary>
    public Guid? SelectedMeaningId { get; private set; }

    /// <summary>
    /// Bu dictionary kaydının hangi lookup sonucundan geldiğini gösterir.
    /// 
    /// Nullable olmasının sebebi:
    /// İçerik ileride sistem önerisi, admin eklemesi veya başka akıştan da kaydedilebilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; private set; }

    /// <summary>
    /// Kullanıcının bu içeriği dictionary'sine kaydettiği zamandır.
    /// </summary>
    public DateTime SavedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Kullanıcının dictionary kaydı aktif mi?
    /// 
    /// Kullanıcı dictionary'den kaldırırsa kaydı fiziksel silmek yerine pasif yapabiliriz.
    /// Böylece geçmiş quiz/progress verileri korunur.
    /// </summary>
    public bool IsActive { get; private set; } = true;


    /// <summary>
    /// Kullanıcının seçtiği anlamı değiştirir.
    /// 
    /// Örneğin bir kelimenin birden fazla anlamı varsa kullanıcı ana çalışacağı anlamı seçebilir.
    /// </summary>
    public void ChangeSelectedMeaning(Guid? selectedMeaningId)
    {
        SelectedMeaningId = selectedMeaningId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Dictionary kaydını pasif hale getirir.
    /// 
    /// Fiziksel silme yapılmaz.
    /// Çünkü quiz geçmişi ve progress kayıtları korunmalıdır.
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

    /// <summary>
    /// Daha önce pasif hale getirilmiş dictionary kaydını tekrar aktif yapar.
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
}