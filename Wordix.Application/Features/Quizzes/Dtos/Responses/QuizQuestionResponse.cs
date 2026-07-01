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
    /// Soruya ait seçeneklerdir.
    /// 
    /// Doğru cevap bilgisi response'ta gönderilmez.
    /// </summary>
    public IReadOnlyCollection<QuizOptionResponse> Options { get; init; }
        = Array.Empty<QuizOptionResponse>();
}
