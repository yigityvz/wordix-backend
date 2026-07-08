namespace Wordix.Application.Features.AdminAnalytics.Services;

/// <summary>
/// Admin işlemlerini audit log olarak kaydetmek için kullanılan servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Admin analytics endpointleri sistem genelindeki hassas verileri gösterir.
/// - Hangi admin kullanıcının hangi admin aksiyonunu çalıştırdığını kayıt altına almak isteriz.
/// - Handler'lar doğrudan AdminActionLog entity oluşturma detayını bilmesin.
/// - Admin audit davranışı tek bir servis üzerinden yönetilsin.
/// 
/// Bu interface Application katmanındadır.
/// Çünkü admin action log yazma davranışı bir use-case destek davranışıdır.
/// </summary>
public interface IAdminActionLogService
{
    /// <summary>
    /// Geçerli admin kullanıcının yaptığı işlemi AdminActionLogs tablosuna kaydeder.
    /// 
    /// actionType:
    /// - Teknik action adıdır.
    /// - Örnek: ViewedAdminDashboard, ViewedTopSearches.
    /// 
    /// description:
    /// - Okunabilir açıklamadır.
    /// - Örnek: Admin viewed dashboard analytics.
    /// 
    /// relatedEntityType / relatedEntityId:
    /// - İşlem belirli bir entity ile ilişkiliyse doldurulur.
    /// - Dashboard görüntüleme gibi genel işlemlerde null kalabilir.
    /// </summary>
    Task LogCurrentAdminActionAsync(
        string actionType,
        string description,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default);
}