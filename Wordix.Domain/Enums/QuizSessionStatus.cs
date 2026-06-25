namespace Wordix.Domain.Enums;

/// <summary>
/// Bir quiz oturumunun yaşam döngüsündeki durumunu temsil eder.
/// 
/// QuizSession başladığında InProgress olur.
/// Kullanıcı tüm cevapları verdiğinde Completed olur.
/// İleride kullanıcı quizden çıkarsa veya sistem iptal ederse Cancelled kullanılabilir.
/// </summary>
public enum QuizSessionStatus
{
    /// <summary>
    /// Quiz başladı ama henüz tamamlanmadı.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Quiz kullanıcı tarafından tamamlandı.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Quiz tamamlanmadan iptal edildi.
    /// MVP'de aktif kullanılmayabilir ama ilerisi için hazır durur.
    /// </summary>
    Cancelled = 3
}