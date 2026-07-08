using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Admin kullanıcının sistem içinde yaptığı önemli işlemlerin audit log kaydını temsil eder.
/// 
/// Bu entity neden gerekli?
/// - Admin analytics endpointleri sistem genelindeki hassas verileri gösterir.
/// - Hangi adminin hangi admin ekranına veya operasyonel veriye eriştiğini bilmek isteriz.
/// - İleride import başlatma, content review, system setting değiştirme gibi işlemler de loglanabilir.
/// 
/// Önemli mimari karar:
/// - Backend UserProfile kullanmaz.
/// - Admin kimliği de Keycloak token içindeki sub claiminden gelen AdminKeycloakUserId ile tutulur.
/// - Bu tablo user-owned kullanıcı verisi değildir; admin audit/operation log tablosudur.
/// </summary>
public class AdminActionLog : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan bilinçsiz boş nesne oluşturulmasını engellemek için protected bırakıyoruz.
    /// </summary>
    protected AdminActionLog()
    {
    }

    /// <summary>
    /// Yeni admin action log kaydı oluşturur.
    /// </summary>
    /// <param name="adminKeycloakUserId">
    /// İşlemi yapan admin kullanıcının Keycloak sub id değeridir.
    /// </param>
    /// <param name="actionType">
    /// Yapılan işlemin kısa teknik adıdır.
    /// Örnek: ViewedAdminDashboard, ViewedTopSearches.
    /// </param>
    /// <param name="description">
    /// Admin işleminin okunabilir açıklamasıdır.
    /// </param>
    /// <param name="relatedEntityType">
    /// İşlem belirli bir entity ile ilişkiliyse entity tipi.
    /// Örnek: ImportJob, LearningItem.
    /// </param>
    /// <param name="relatedEntityId">
    /// İşlem belirli bir entity ile ilişkiliyse entity id değeri.
    /// </param>
    public AdminActionLog(
        string adminKeycloakUserId,
        string actionType,
        string description,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(adminKeycloakUserId))
        {
            throw new ArgumentException(
                "AdminKeycloakUserId boş olamaz.",
                nameof(adminKeycloakUserId));
        }

        if (string.IsNullOrWhiteSpace(actionType))
        {
            throw new ArgumentException(
                "ActionType boş olamaz.",
                nameof(actionType));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Description boş olamaz.",
                nameof(description));
        }

        if (relatedEntityId == Guid.Empty)
        {
            throw new ArgumentException(
                "RelatedEntityId boş Guid olamaz. Null veya geçerli Guid olmalıdır.",
                nameof(relatedEntityId));
        }

        AdminKeycloakUserId = adminKeycloakUserId.Trim();
        ActionType = actionType.Trim();
        Description = description.Trim();

        RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType)
            ? null
            : relatedEntityType.Trim();

        RelatedEntityId = relatedEntityId;
    }

    /// <summary>
    /// İşlemi yapan admin kullanıcının Keycloak user id değeridir.
    /// 
    /// Bu değer JWT token içindeki sub claiminden gelir.
    /// Wordix backend admin için ayrıca UserProfileId üretmez.
    /// </summary>
    public string AdminKeycloakUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Yapılan admin işleminin teknik adıdır.
    /// 
    /// Neden string?
    /// - Admin action türleri ileride çok artabilir.
    /// - Her yeni action için enum migration zorunluluğu istemiyoruz.
    /// - String + constants yaklaşımı bu audit senaryosu için daha esnektir.
    /// 
    /// Örnek:
    /// ViewedAdminDashboard
    /// ViewedTopSearches
    /// ViewedProviderStats
    /// StartedImportJob
    /// </summary>
    public string ActionType { get; private set; } = string.Empty;

    /// <summary>
    /// İşlemin okunabilir açıklamasıdır.
    /// 
    /// Örnek:
    /// Admin viewed provider statistics.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// İşlem belirli bir entity ile ilişkiliyse entity tipidir.
    /// 
    /// Örnek:
    /// ImportJob
    /// LearningItem
    /// ProviderRequestLog
    /// 
    /// Analytics dashboard görüntüleme gibi genel işlemlerde null olabilir.
    /// </summary>
    public string? RelatedEntityType { get; private set; }

    /// <summary>
    /// İşlem belirli bir entity ile ilişkiliyse entity id değeridir.
    /// 
    /// Genel dashboard görüntüleme gibi işlemlerde null olabilir.
    /// </summary>
    public Guid? RelatedEntityId { get; private set; }
}