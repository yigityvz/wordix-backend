using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının uygulama içi tercihlerini tutar.
/// 
/// UserProfile kimlik/profil bilgisini temsil eder.
/// UserPreference ise quiz, öneri ve motivasyon gibi kişiselleştirme ayarlarını temsil eder.
/// </summary>
public class UserPreference : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected UserPreference()
    {
    }

    /// <summary>
    /// Yeni kullanıcı için varsayılan tercih kaydı oluşturur.
    /// 
    /// Bu kayıt genelde UserProfile ilk oluşturulduğunda beraber oluşturulur.
    /// </summary>
    public UserPreference(Guid userProfileId)
    {
        if (userProfileId == Guid.Empty)
        {
            throw new ArgumentException("UserProfileId boş Guid olamaz.", nameof(userProfileId));
        }

        UserProfileId = userProfileId;
    }

    /// <summary>
    /// Bu tercih kaydının hangi kullanıcıya ait olduğunu gösterir.
    /// </summary>
    public Guid UserProfileId { get; private set; }

    /// <summary>
    /// Kullanıcının varsayılan quiz tipi.
    /// 
    /// Örneğin kullanıcı genelde Writing quiz tercih ediyorsa burada tutulabilir.
    /// İlk prototipte Test quiz kullanılacak olsa da yapı genişlemeye hazırdır.
    /// </summary>
    public QuizType DefaultQuizType { get; private set; } = QuizType.Test;

    /// <summary>
    /// Kullanıcının varsayılan zorluk tercihi.
    /// </summary>
    public DifficultyGroup DefaultDifficultyGroup { get; private set; } = DifficultyGroup.Beginner;

    /// <summary>
    /// Quizlerde sistem önerisi içerikler kullanılsın mı?
    /// 
    /// İlk prototipte kapalı tutulabilir.
    /// İleride sistem önerili quiz modülü geldiğinde aktif kullanılacaktır.
    /// </summary>
    public bool IncludeSystemRecommendations { get; private set; }

    /// <summary>
    /// Kullanıcıya uygulama içi motivasyon mesajları gösterilsin mi?
    /// </summary>
    public bool MotivationMessagesEnabled { get; private set; } = true;

    /// <summary>
    /// Kullanıcının varsayılan quiz soru sayısı tercihidir.
    /// </summary>
    public int PreferredQuestionCount { get; private set; } = 10;


    /// <summary>
    /// Kullanıcının varsayılan quiz ayarlarını günceller.
    /// </summary>
    public void ChangeQuizDefaults(
        QuizType defaultQuizType,
        DifficultyGroup defaultDifficultyGroup,
        int preferredQuestionCount)
    {
        if (preferredQuestionCount <= 0)
        {
            throw new ArgumentException("Soru sayısı 0'dan büyük olmalıdır.", nameof(preferredQuestionCount));
        }

        DefaultQuizType = defaultQuizType;
        DefaultDifficultyGroup = defaultDifficultyGroup;
        PreferredQuestionCount = preferredQuestionCount;

        MarkAsUpdated();
    }

    /// <summary>
    /// Sistem önerilerinin quizlere dahil edilip edilmeyeceğini değiştirir.
    /// </summary>
    public void SetSystemRecommendations(bool includeSystemRecommendations)
    {
        IncludeSystemRecommendations = includeSystemRecommendations;
        MarkAsUpdated();
    }

    /// <summary>
    /// Motivasyon mesajlarını açar veya kapatır.
    /// </summary>
    public void SetMotivationMessages(bool enabled)
    {
        MotivationMessagesEnabled = enabled;
        MarkAsUpdated();
    }
}