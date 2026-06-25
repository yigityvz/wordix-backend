namespace Wordix.Domain.Enums;

/// <summary>
/// Bir LearningItem kaydının sisteme hangi yolla eklendiğini temsil eder.
/// 
/// Bu enum, içerik kaynağını anlamamızı sağlar:
/// - Sistem başlangıç verisi mi?
/// - Kullanıcı lookup yaptı diye mi eklendi?
/// - Provider'dan mı geldi?
/// - Import job ile mi oluşturuldu?
/// </summary>
public enum LearningItemSourceType
{
    /// <summary>
    /// Kaynağı bilinmeyen veya henüz belirlenmemiş içerik.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Sistem başlangıcında seed edilen içerik.
    /// Örneğin başlangıç test kelimeleri.
    /// </summary>
    SystemSeed = 1,

    /// <summary>
    /// Kullanıcının lookup yapması sonucunda sisteme eklenen içerik.
    /// </summary>
    UserLookup = 2,

    /// <summary>
    /// Dış provider'dan gelen içerik.
    /// Örneğin ileride Wiktionary veya Kaikki provider'ı.
    /// </summary>
    Provider = 3,

    /// <summary>
    /// Toplu import süreciyle sisteme eklenen içerik.
    /// </summary>
    Import = 4,

    /// <summary>
    /// Admin tarafından manuel oluşturulan içerik.
    /// </summary>
    AdminCreated = 5
}