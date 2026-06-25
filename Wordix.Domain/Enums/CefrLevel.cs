namespace Wordix.Domain.Enums;

/// <summary>
/// CEFR, Avrupa Ortak Dil Referans Çerçevesi seviyelerini temsil eder.
/// 
/// A1-A2: Başlangıç
/// B1-B2: Orta/üst orta
/// C1-C2: İleri seviye
/// </summary>
public enum CefrLevel
{
    /// <summary>
    /// Seviye bilinmiyor veya henüz belirlenmemiş.
    /// </summary>
    Unknown = 0,

    A1 = 1,
    A2 = 2,
    B1 = 3,
    B2 = 4,
    C1 = 5,
    C2 = 6
}