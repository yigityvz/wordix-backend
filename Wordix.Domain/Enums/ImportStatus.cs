namespace Wordix.Domain.Enums;

/// <summary>
/// Import işlemlerinin durumunu temsil eder.
/// 
/// Bu enum ileride ImportJob ve ImportItem entity'lerinde kullanılabilir.
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// İşlem oluşturuldu ama henüz başlamadı.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// İşlem devam ediyor.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// İşlem başarılı tamamlandı.
    /// </summary>
    Succeeded = 3,

    /// <summary>
    /// İşlem hata aldı.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// İşlem kısmen başarılı oldu.
    /// Örneğin 1000 kaydın 950'si başarılı, 50'si hatalı olabilir.
    /// </summary>
    PartiallySucceeded = 5,

    /// <summary>
    /// İşlem iptal edildi.
    /// </summary>
    Cancelled = 6
}