namespace Wordix.Domain.Enums;

/// <summary>
/// Dış provider cache kaydının durumunu temsil eder.
/// </summary>
public enum ExternalContentCacheStatus
{
    /// <summary>
    /// Durum bilinmiyor.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Cache kaydı aktif ve kullanılabilir.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Cache kaydı süresi dolmuş kabul edilir.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Cache kaydı admin/sistem tarafından pasif yapılmıştır.
    /// </summary>
    Disabled = 3
}