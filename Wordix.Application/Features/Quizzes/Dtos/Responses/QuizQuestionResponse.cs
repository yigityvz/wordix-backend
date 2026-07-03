namespace Wordix.Application.Features.Quizzes.Dtos.Responses;

/// <summary>
/// Quiz içindeki tek bir soruyu temsil eden response modelidir.
/// 
/// İlk prototipte soru tipi:
/// İngilizce kelime gösterilir, Türkçe anlam seçeneklerden seçilir.
/// </summary>
public sealed class QuizQuestionResponse
{
    /// <summary>
    /// QuizQuestion entity id değeridir.
    /// 
    /// Kullanıcı cevap gönderirken ileride bu id kullanılacak.
    /// </summary>
    public Guid QuizQuestionId { get; init; }

    /// <summary>
    /// Sorunun sıra numarasıdır.
    /// 
    /// Örnek:
    /// 1, 2, 3
    /// </summary>
    public int QuestionOrder { get; init; }

    /// <summary>
    /// Soru metnidir.
    /// 
    /// İlk prototipte İngilizce kelime olur.
    /// Örnek:
    /// achieve
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Sorunun hangi LearningItem'dan üretildiğini gösterir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer soru bir Word üzerinden üretildiyse Word id değeridir.
    /// Phrase/Sentence ileride geldiğinde null olabilir.
    /// </summary>
    public Guid? WordId { get; init; }


    /// <summary>
    /// Eğer soru bir Phrase üzerinden üretildiyse Phrase id değeridir.
    /// Word sorularında null olur.
    /// </summary>
    public Guid? PhraseId { get; init; }


    /// <summary>
    /// Eğer soru Sentence üzerinden üretildiyse Sentence id değeridir.
    /// Word/Phrase sorularında null olur.
    /// </summary>
    public Guid? SentenceId { get; init; }


    /// <summary>
    /// Sorunun içerik tipi.
    /// 
    /// İlk prototipte:
    /// Word
    /// </summary>
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Soru tipi.
    /// 
    /// İlk prototipte:
    /// MultipleChoiceTranslation
    /// </summary>
    public string QuestionType { get; init; } = string.Empty;


    /// <summary>
    /// Bu soru sistem önerisiyle mi eklendi?
    /// 
    /// true ise bu soru kullanıcının mevcut dictionary/deck itemından değil,
    /// sistem recommendation akışından gelmiştir.
    /// </summary>
    public bool IsSystemRecommended { get; init; }

    /// <summary>
    /// Sistem önerisi ise öneri sebebidir.
    /// 
    /// Örnek:
    /// DifficultyLevelMatch
    /// StarterRecommendation
    /// SimilarToDifficultItems
    /// 
    /// Normal dictionary/deck sorularında null olur.
    /// </summary>
    public string? RecommendationReason { get; init; }

    /// <summary>
    /// Bu soru bir QuizRecommendationItem kaydıyla ilişkiliyse onun id değeridir.
    /// 
    /// Faz 23'te özellikle submit answer sonrası veya save-to-dictionary akışında kullanılabilir.
    /// Normal sorularda null olur.
    /// </summary>
    public Guid? QuizRecommendationItemId { get; init; }

    /// <summary>
    /// Soruya ait seçeneklerdir.
    /// 
    /// Doğru cevap bilgisi response'ta gönderilmez.
    /// </summary>
    public IReadOnlyCollection<QuizOptionResponse> Options { get; init; }
        = Array.Empty<QuizOptionResponse>();
}
