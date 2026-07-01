namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcı bir sentence'i dictionary'sine başarıyla kaydettiğinde dönecek response modelidir.
/// 
/// Bu response hem oluşturulan öğrenme kaydını hem de sentence/translation kayıtlarını frontend'e döndürür.
/// </summary>
public sealed class SaveSentenceToDictionaryResponse
{
    /// <summary>
    /// Oluşturulan UserLearningItem kaydının id değeridir.
    /// 
    /// Dictionary item detayına gitmek için kullanılabilir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Sentence için oluşturulan veya bulunan LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Kaydedilen Sentence entity id değeridir.
    /// </summary>
    public Guid SentenceId { get; init; }

    /// <summary>
    /// Kaydedilen veya bulunan SentenceTranslation entity id değeridir.
    /// </summary>
    public Guid SentenceTranslationId { get; init; }

    /// <summary>
    /// Kaynak cümle metnidir.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş kaynak cümle metnidir.
    /// </summary>
    public string NormalizedSourceText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dildeki çeviri metnidir.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş çeviri metnidir.
    /// </summary>
    public string NormalizedTranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Oluşturulan UserLearningProgress kaydının id değeridir.
    /// </summary>
    public Guid UserLearningProgressId { get; init; }

    /// <summary>
    /// Kaydın hangi lookup history üzerinden geldiğini gösterir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }

    /// <summary>
    /// Kullanıcının dictionary'sine kayıt tarihi.
    /// </summary>
    public DateTimeOffset SavedAt { get; init; }

    /// <summary>
    /// İlk öğrenme durumudur.
    /// </summary>
    public string LearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// İlk confidence score değeridir.
    /// </summary>
    public int LearningConfidenceScore { get; init; }

    /// <summary>
    /// Kullanıcının dictionary kaydı aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}