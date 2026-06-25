namespace Wordix.Domain.Enums;

/// <summary>
/// Quiz sorularının hangi kaynaktan üretileceğini temsil eder.
/// </summary>
public enum QuizSourceType
{
    /// <summary>
    /// Kullanıcının tüm kişisel dictionary'sinden quiz oluşturulur.
    /// </summary>
    UserDictionary = 1,

    /// <summary>
    /// Belirli bir deck üzerinden quiz oluşturulur.
    /// </summary>
    Deck = 2,

    /// <summary>
    /// Kullanıcının zor işaretlediği içeriklerden quiz oluşturulur.
    /// </summary>
    DifficultItems = 3,

    /// <summary>
    /// Sistem önerisi içeriklerden quiz oluşturulur.
    /// </summary>
    SystemRecommendations = 4,

    /// <summary>
    /// Birden fazla kaynak karışık kullanılabilir.
    /// </summary>
    Mixed = 5
}