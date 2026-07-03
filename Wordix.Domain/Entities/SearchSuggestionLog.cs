using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sistemin kullanıcıya gösterdiği öneri davranışını loglar.
/// 
/// Bu entity neden var?
/// - Sistem önerileri sadece quiz içinde kullanılmayabilir.
/// - İleride lookup ekranında, dashboard'da veya review ekranında da öneri gösterilebilir.
/// - Bu log, önerinin kabul edilip edilmediğini ve dictionary'ye kaydedilip kaydedilmediğini analiz etmeyi sağlar.
/// 
/// Faz 23'te kullanım:
/// - Quiz içine sistem önerisi eklendiğinde SearchSuggestionLog oluşturulur.
/// - Kullanıcı öneriyi dictionary'ye eklerse WasAccepted ve WasSaved true yapılır.
/// 
/// Önemli:
/// Bu tablo KeycloakUserId ile user-owned çalışır.
/// UserProfileId kullanılmaz.
/// </summary>
public class SearchSuggestionLog : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected SearchSuggestionLog()
    {
    }

    /// <summary>
    /// Yeni suggestion log kaydı oluşturur.
    /// </summary>
    public SearchSuggestionLog(
        string keycloakUserId,
        Guid learningItemId,
        RecommendationReason suggestionReason,
        Guid? quizSessionId = null,
        Guid? quizRecommendationItemId = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException(
                "KeycloakUserId boş olamaz.",
                nameof(keycloakUserId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "LearningItemId boş Guid olamaz.",
                nameof(learningItemId));
        }

        if (!Enum.IsDefined(suggestionReason))
        {
            throw new ArgumentException(
                "Geçersiz RecommendationReason değeri.",
                nameof(suggestionReason));
        }

        KeycloakUserId = keycloakUserId.Trim();
        LearningItemId = learningItemId;
        SuggestionReason = suggestionReason;
        QuizSessionId = quizSessionId;
        QuizRecommendationItemId = quizRecommendationItemId;
    }

    /// <summary>
    /// Önerinin gösterildiği Keycloak kullanıcısıdır.
    /// 
    /// JWT token içindeki sub claiminden gelen KeycloakUserId tutulur.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Önerilen global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Öneri quiz session içinde gösterildiyse ilgili session id değeridir.
    /// Lookup/dashboard önerilerinde null olabilir.
    /// </summary>
    public Guid? QuizSessionId { get; private set; }

    /// <summary>
    /// Öneri quiz recommendation item kaydıyla ilişkiliyse onun id değeridir.
    /// 
    /// Faz 23 quiz önerileri için dolu tutulabilir.
    /// </summary>
    public Guid? QuizRecommendationItemId { get; private set; }

    /// <summary>
    /// Bu öneri kullanıcıya neden gösterildi?
    /// </summary>
    public RecommendationReason SuggestionReason { get; private set; }

    /// <summary>
    /// Kullanıcı öneriyi kabul etti mi?
    /// 
    /// Faz 23'te "kabul" davranışı:
    /// - Kullanıcının öneriyi dictionary'ye eklemek istemesi.
    /// </summary>
    public bool WasAccepted { get; private set; }

    /// <summary>
    /// Kullanıcı öneriyi dictionary'sine kaydetti mi?
    /// </summary>
    public bool WasSaved { get; private set; }

    /// <summary>
    /// Log kaydını belirli bir QuizRecommendationItem ile ilişkilendirir.
    /// 
    /// Bazı akışlarda log önce, recommendation item sonra oluşursa kullanılabilir.
    /// Faz 23'te büyük ihtimalle constructor üzerinden dolu geçeceğiz.
    /// </summary>
    public void AttachQuizRecommendationItem(Guid quizRecommendationItemId)
    {
        if (quizRecommendationItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "QuizRecommendationItemId boş Guid olamaz.",
                nameof(quizRecommendationItemId));
        }

        QuizRecommendationItemId = quizRecommendationItemId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Kullanıcının öneriyi kabul ettiğini işaretler.
    /// </summary>
    public void MarkAsAccepted()
    {
        if (WasAccepted)
        {
            return;
        }

        WasAccepted = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Kullanıcının öneriyi dictionary'sine kaydettiğini işaretler.
    /// 
    /// Kaydetme aynı zamanda kabul anlamına da gelir.
    /// </summary>
    public void MarkAsSaved()
    {
        var changed = false;

        if (!WasAccepted)
        {
            WasAccepted = true;
            changed = true;
        }

        if (!WasSaved)
        {
            WasSaved = true;
            changed = true;
        }

        if (changed)
        {
            MarkAsUpdated();
        }
    }
}