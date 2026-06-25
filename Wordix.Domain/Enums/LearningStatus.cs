namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcının kaydettiği bir içeriği öğrenme durumunu temsil eder.
/// 
/// Bu enum UserLearningProgress tarafında kullanılacaktır.
/// </summary>
public enum LearningStatus
{
    /// <summary>
    /// Kullanıcı içeriği yeni kaydetti, henüz çalışmadı.
    /// </summary>
    New = 1,

    /// <summary>
    /// Kullanıcı içeriği çalışmaya başladı ama henüz güvenilir şekilde öğrenmedi.
    /// </summary>
    Learning = 2,

    /// <summary>
    /// İçerik tekrar/review aşamasında.
    /// </summary>
    Reviewing = 3,

    /// <summary>
    /// Kullanıcı içeriği büyük ölçüde öğrendi.
    /// </summary>
    Learned = 4,

    /// <summary>
    /// Kullanıcı içeriği çok güçlü şekilde öğrendi.
    /// </summary>
    Mastered = 5
}