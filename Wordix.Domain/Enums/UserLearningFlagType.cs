namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcının kendi dictionary item'ına verebileceği kişisel işaret tiplerini temsil eder.
/// 
/// Bu enum neden var?
/// - Favorite, Difficult gibi değerleri string olarak dağınık tutmak istemiyoruz.
/// - Database'de flag tipi kontrollü bir enum olarak saklanır.
/// - Quiz, dashboard ve recommendation gibi modüller bu enum üzerinden karar verebilir.
/// 
/// Önemli:
/// Bu flagler global LearningItem'a değil, kullanıcının kişisel UserLearningItem kaydına uygulanır.
/// Yani bir kelime Yiğit için Difficult olabilir, başka kullanıcı için olmayabilir.
/// </summary>
public enum UserLearningFlagType
{
    /// <summary>
    /// Kullanıcının favori olarak işaretlediği içerik.
    /// 
    /// Frontend'de yıldız gibi gösterilebilir.
    /// </summary>
    Favorite = 1,

    /// <summary>
    /// Kullanıcının zorlandığını belirttiği içerik.
    /// 
    /// Faz 22'de quiz candidate seçiminde öncelik sinyali olarak kullanılacaktır.
    /// </summary>
    Difficult = 2,

    /// <summary>
    /// Kullanıcının daha fazla pratik yapmak istediği içerik.
    /// 
    /// Faz 22'de endpoint olarak desteklenebilir.
    /// Quiz önceliğine Difficult kadar güçlü etki ettirmeyebiliriz.
    /// Future-ready olarak ekliyoruz.
    /// </summary>
    WantMorePractice = 3,

    /// <summary>
    /// Kullanıcının şimdilik çalışmak istemediği veya göz ardı etmek istediği içerik.
    /// 
    /// Bu fazda aktif quiz dışlama kuralı yapmayacağız.
    /// Future-ready olarak tutulur.
    /// </summary>
    Ignored = 4
}