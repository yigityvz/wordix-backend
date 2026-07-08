namespace Wordix.Application.Common.Constants;

/// <summary>
/// Admin analytics modülünde kullanılan sabit değerleri merkezi olarak tutar.
/// 
/// Bu dosya neden var?
/// - Limit değerleri controller/handler içinde dağılmasın.
/// - AdminActionLog action type stringleri magic string olarak yazılmasın.
/// - İleride admin analytics davranışı değişirse sabitler tek yerden yönetilsin.
/// 
/// Bu class Application katmanındadır.
/// Çünkü bu değerler use-case davranışını ve API query limitlerini ilgilendirir.
/// </summary>
public static class AdminAnalyticsConstants
{
    /// <summary>
    /// Liste endpointleri için varsayılan kayıt sayısı.
    /// 
    /// Örnek:
    /// Top searches endpointi limit gönderilmezse 20 kayıt döner.
    /// </summary>
    public const int DefaultLimit = 20;

    /// <summary>
    /// Liste endpointleri için maksimum kayıt sayısı.
    /// 
    /// Neden sınır var?
    /// - Admin endpointleri büyük tablolarda çalışacak.
    /// - Tek istekte binlerce kayıt döndürmek istemeyiz.
    /// - Production ortamında performans ve güvenlik için limit zorunludur.
    /// </summary>
    public const int MaxLimit = 100;

    /// <summary>
    /// Tarih aralığı verilmezse varsayılan olarak son kaç günün verisi analiz edilecek?
    /// 
    /// İlk fazda 30 gün mantıklı bir varsayılandır.
    /// İleride SystemSetting ile yönetilebilir hale getirilebilir.
    /// </summary>
    public const int DefaultDateRangeDays = 30;

    /// <summary>
    /// AdminActionLog.ActionType için kullanılan sabit action isimleri.
    /// 
    /// Neden enum değil string constants?
    /// - Admin action türleri ileride çok artabilir.
    /// - Her yeni action için domain enum + migration istemiyoruz.
    /// - String constants yaklaşımı audit log için daha esnek.
    /// </summary>
    public static class ActionTypes
    {
        public const string ViewedAdminDashboard = "ViewedAdminDashboard";
        public const string ViewedTopSearches = "ViewedTopSearches";
        public const string ViewedTopSavedItems = "ViewedTopSavedItems";
        public const string ViewedMostWrongItems = "ViewedMostWrongItems";
        public const string ViewedProviderStats = "ViewedProviderStats";
    }

    /// <summary>
    /// AdminActionLog.Description için okunabilir açıklama sabitleri.
    /// </summary>
    public static class Descriptions
    {
        public const string ViewedAdminDashboard = "Admin viewed dashboard analytics.";
        public const string ViewedTopSearches = "Admin viewed top searched items analytics.";
        public const string ViewedTopSavedItems = "Admin viewed top saved learning items analytics.";
        public const string ViewedMostWrongItems = "Admin viewed most wrong learning items analytics.";
        public const string ViewedProviderStats = "Admin viewed provider statistics analytics.";
    }

    /// <summary>
    /// AdminActionLog.RelatedEntityType için kullanılabilecek ortak entity isimleri.
    /// 
    /// Dashboard gibi genel görüntülemelerde RelatedEntityType null kalabilir.
    /// Belirli entity bazlı admin aksiyonlarında bu sabitler kullanılabilir.
    /// </summary>
    public static class RelatedEntityTypes
    {
        public const string LearningItem = "LearningItem";
        public const string ImportJob = "ImportJob";
        public const string ProviderRequestLog = "ProviderRequestLog";
        public const string ExternalContentCache = "ExternalContentCache";
    }
}