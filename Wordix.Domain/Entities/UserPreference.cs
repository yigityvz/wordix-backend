using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının uygulama içi tercihlerini tutar.
/// 
/// Bu entity kimlik/profil bilgisi tutmaz.
/// Kimlik yönetimi Keycloak tarafındadır.
/// 
/// UserPreference; quiz, öneri, motivasyon ve ileride kullanıcı ekranından değiştirilebilecek
/// kişiselleştirme ayarlarını temsil eder.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcı tercihleri token içindeki "sub" claiminden gelen KeycloakUserId ile kullanıcıya bağlanır.
/// </summary>
public class UserPreference : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan boş UserPreference oluşturulmasını istemediğimiz için protected bırakıyoruz.
    /// </summary>
    protected UserPreference()
    {
    }

    /// <summary>
    /// Yeni kullanıcı için varsayılan tercih kaydı oluşturur.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelir.
    /// - Bu preference kaydının hangi Keycloak kullanıcısına ait olduğunu belirtir.
    /// - Backend tarafından üretilen UserProfileId değildir.
    /// 
    /// Not:
    /// Bu kayıt artık /api/profile/me çağrısında otomatik oluşturulmaz.
    /// İleride preference endpointleri geldiğinde kullanıcı ayarları ilk kez okunurken
    /// veya güncellenirken oluşturulabilir.
    /// </summary>
    public UserPreference(string keycloakUserId)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("KeycloakUserId boş olamaz.", nameof(keycloakUserId));
        }

        KeycloakUserId = keycloakUserId.Trim();
    }

    /// <summary>
    /// Bu tercih kaydının ait olduğu Keycloak kullanıcısının id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden gelir.
    /// Wordix backend ayrıca UserProfileId/UserId üretmediği için
    /// kullanıcı tercihleri bu alan üzerinden filtrelenir.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

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