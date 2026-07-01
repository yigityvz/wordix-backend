namespace Wordix.Application.Features.UserDictionary.Dtos.Requests;

/// <summary>
/// Kullanıcının sentence lookup sonucunu kendi dictionary'sine kaydetmek için gönderdiği request modelidir.
/// 
/// Neden mevcut SaveLearningItemRequest'i kullanmıyoruz?
/// - SaveLearningItemRequest mevcut bir LearningItemId bekler.
/// - Sentence lookup anında LearningItem oluşturulmaz.
/// - Sentence ancak kullanıcı kaydetmek isterse LearningItem + Sentence + SentenceTranslation olarak kalıcı hale gelir.
/// 
/// Bu yüzden sentence save ayrı bir use-case olarak modellenmiştir.
/// </summary>
public sealed class SaveSentenceToDictionaryRequest
{
    /// <summary>
    /// Kaynak cümle metnidir.
    /// 
    /// Örnek:
    /// I want to improve my English
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dildeki cümle çevirisidir.
    /// 
    /// Örnek:
    /// İngilizcemi geliştirmek istiyorum.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Bu sentence kaydının hangi lookup history sonucundan geldiğini gösterir.
    /// 
    /// Nullable tutulur.
    /// Çünkü ileride kullanıcı sentence'i farklı ekranlardan da dictionary'ye kaydedebilir.
    /// Ancak lookup'tan geliyorsa gönderilmesi önerilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }
}