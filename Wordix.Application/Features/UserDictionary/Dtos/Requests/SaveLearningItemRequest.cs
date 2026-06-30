namespace Wordix.Application.Features.UserDictionary.Dtos.Requests;

/// <summary>
/// Kullanıcının bir LearningItem'ı kendi dictionary'sine kaydetmek için
/// API'ye göndereceği request modelidir.
/// 
/// Bu model HTTP request body contract'ıdır.
/// Controller bu modeli alır, SaveLearningItemCommand'e manual map eder.
/// 
/// Örnek JSON:
/// {
///   "learningItemId": "....",
///   "selectedMeaningId": "....",
///   "sourceLookupHistoryId": "...."
/// }
/// </summary>
public sealed class SaveLearningItemRequest
{
    /// <summary>
    /// Kullanıcının dictionary'sine kaydetmek istediği LearningItem id değeridir.
    /// 
    /// Wordix'te Word/Phrase/Sentence gibi öğrenilebilir tüm içerikler
    /// LearningItem ortak çatısı altında tutulur.
    /// Bu yüzden dictionary kaydı doğrudan WordId ile değil LearningItemId ile yapılır.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının bu içerik için seçtiği anlam id değeridir.
    /// 
    /// Bir LearningItem'ın birden fazla Meaning kaydı olabilir.
    /// Kullanıcı özellikle hangi anlamı öğrenmek istiyorsa onu seçebilir.
    /// 
    /// İlk prototipte genellikle primary meaning seçilecek.
    /// </summary>
    public Guid? SelectedMeaningId { get; init; }

    /// <summary>
    /// Bu dictionary kaydının hangi lookup işleminden geldiğini gösteren opsiyonel alandır.
    /// 
    /// Faz 13'te lookup response içinde LookupHistoryId dönmüştük.
    /// Kullanıcı lookup sonucundan sonra "Kaydet" derse frontend bu id'yi gönderebilir.
    /// 
    /// Nullable çünkü kullanıcı ileride başka ekrandan da içerik kaydedebilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }
}
