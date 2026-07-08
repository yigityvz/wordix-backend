namespace Wordix.Application.Common.Constants;

/// <summary>
/// User statistics ve dashboard endpointlerinde kullanılan sabit değerleri merkezi olarak tutar.
/// 
/// Bu class neden var?
/// - PageSize limitleri controller/validator/repository içinde dağılmasın.
/// - Difficult item filtre değerleri magic string olarak kullanılmasın.
/// - Confidence score bucket aralıkları tek yerden yönetilsin.
/// 
/// Bu class Application katmanındadır.
/// Çünkü bu değerler API/use-case davranışını ilgilendirir.
/// </summary>
public static class UserStatisticsConstants
{
    /// <summary>
    /// Sayfalı endpointlerde varsayılan sayfa numarası.
    /// </summary>
    public const int DefaultPageNumber = 1;

    /// <summary>
    /// Sayfalı endpointlerde varsayılan sayfa boyutu.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Sayfalı endpointlerde izin verilen maksimum sayfa boyutu.
    /// 
    /// Production mantığı:
    /// Tek request ile binlerce kayıt döndürmek istemiyoruz.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Quiz statistics için varsayılan tarih aralığı.
    /// 
    /// Eğer fromUtc/toUtc gönderilmezse son 30 gün analiz edilir.
    /// </summary>
    public const int DefaultDateRangeDays = 30;

    /// <summary>
    /// Difficult items endpointinde desteklenen source filtreleri.
    /// </summary>
    public static class DifficultItemSources
    {
        /// <summary>
        /// Hem manuel Difficult flag'i hem de progress sinyalleri dikkate alınır.
        /// </summary>
        public const string Both = "both";

        /// <summary>
        /// Sadece kullanıcının manuel Difficult flag'i dikkate alınır.
        /// </summary>
        public const string Manual = "manual";

        /// <summary>
        /// Sadece progress/quiz sinyalleri dikkate alınır.
        /// </summary>
        public const string Progress = "progress";
    }

    /// <summary>
    /// Difficult items endpointinde desteklenen sıralama değerleri.
    /// </summary>
    public static class DifficultItemSortBy
    {
        /// <summary>
        /// Confidence score düşükten yükseğe.
        /// En zayıf öğrenilen itemlar önce gelir.
        /// </summary>
        public const string ConfidenceAsc = "confidenceAsc";

        /// <summary>
        /// Yanlış sayısı çoktan aza.
        /// </summary>
        public const string WrongCountDesc = "wrongCountDesc";

        /// <summary>
        /// Üst üste yanlış sayısı çoktan aza.
        /// </summary>
        public const string ConsecutiveWrongDesc = "consecutiveWrongDesc";

        /// <summary>
        /// Tekrar tarihi en yakın/geçmiş olanlar önce.
        /// </summary>
        public const string NextReviewAsc = "nextReviewAsc";

        /// <summary>
        /// En son kaydedilen dictionary itemlar önce.
        /// </summary>
        public const string SavedAtDesc = "savedAtDesc";
    }

    /// <summary>
    /// Confidence score dağılımında kullanılacak bucket etiketleri.
    /// </summary>
    public static class ConfidenceBucketLabels
    {
        public const string VeryLow = "VeryLow";
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string VeryHigh = "VeryHigh";
    }
}