namespace Wordix.Domain.Enums;

/// <summary>
/// Sistem önerisinin kullanıcıya neden gösterildiğini temsil eder.
/// 
/// Bu enum neden var?
/// - RecommendationReason bilgisini string olarak dağınık tutmak istemiyoruz.
/// - QuizRecommendationItem ve SearchSuggestionLog kayıtları aynı kontrollü değerleri kullanır.
/// - İleride analytics tarafında "hangi öneri sebebi daha çok işe yarıyor?" sorusu cevaplanabilir.
/// </summary>
public enum RecommendationReason
{
    /// <summary>
    /// Sebep bilinmiyor veya henüz belirlenmemiş.
    /// Defensive/default değer olarak tutulur.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// İçerik, kullanıcının tercih ettiği zorluk grubuna uygun olduğu için önerildi.
    /// 
    /// Faz 23'teki ilk local recommendation algoritmasının ana sebebi budur.
    /// </summary>
    DifficultyLevelMatch = 1,

    /// <summary>
    /// Kullanıcının mevcut öğrenme seviyesine uygun olduğu için önerildi.
    /// 
    /// İleride UserLearningProgress ve başarı oranlarına göre daha anlamlı hale gelir.
    /// </summary>
    UserLevelMatch = 2,

    /// <summary>
    /// Kullanıcının dictionary'sindeki içeriklere benzer veya tamamlayıcı olduğu için önerildi.
    /// 
    /// Faz 24/27 sonrası tag/category veya analytics ile güçlendirilebilir.
    /// </summary>
    RelatedToKnownItems = 3,

    /// <summary>
    /// Kullanıcının daha önce zorlandığı alana benzer olduğu için önerildi.
    /// 
    /// Faz 22 Difficult flag ve ileride analytics verileriyle beslenebilir.
    /// </summary>
    SimilarToDifficultItems = 4,

    /// <summary>
    /// Sistemin genel başlangıç önerisi olarak seçildi.
    /// 
    /// Kullanıcıda yeterli preference/progress verisi yoksa kullanılabilir.
    /// </summary>
    StarterRecommendation = 5
}