namespace Wordix.Domain.Enums;

/// <summary>
/// Import job çalışma durumunu temsil eder.
/// </summary>
public enum ImportJobStatus
{
    /// <summary>
    /// Durum bilinmiyor.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Job oluşturuldu ama henüz başlamadı.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Job çalışıyor.
    /// </summary>
    Running = 2,

    /// <summary>
    /// Job başarıyla tamamlandı.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Job hata ile sonlandı.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Job iptal edildi.
    /// </summary>
    Cancelled = 5
}