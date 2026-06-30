using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının kendi dictionary'sine kaydettiği öğrenilebilir içeriği temsil eder.
/// 
/// Bu entity doğrudan Word'e değil LearningItem'a bağlıdır.
/// Böylece kullanıcı ileride Word, Phrase veya Sentence kaydedebilir.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Bu entity, kullanıcıyı token içindeki "sub" claiminden gelen KeycloakUserId ile ilişkilendirir.
/// </summary>
public class UserLearningItem : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan bilinçsiz boş nesne oluşturulmasını engellemek için protected bırakıyoruz.
    /// </summary>
    protected UserLearningItem()
    {
    }

    /// <summary>
    /// Kullanıcı için yeni dictionary kaydı oluşturur.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelir.
    /// - Dictionary kaydının hangi Keycloak kullanıcısına ait olduğunu belirtir.
    /// - Backend tarafından üretilen UserProfileId değildir.
    /// </summary>
    public UserLearningItem(
        string keycloakUserId,
        Guid learningItemId,
        Guid? selectedMeaningId = null,
        Guid? sourceLookupHistoryId = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("KeycloakUserId boş olamaz.", nameof(keycloakUserId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        KeycloakUserId = keycloakUserId.Trim();
        LearningItemId = learningItemId;
        SelectedMeaningId = selectedMeaningId;
        SourceLookupHistoryId = sourceLookupHistoryId;
        SavedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Dictionary kaydının ait olduğu Keycloak kullanıcısının id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden gelir.
    /// Wordix backend ayrıca UserProfileId/UserId üretmediği için
    /// kullanıcıya ait dictionary kayıtları bu alan üzerinden filtrelenir.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

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