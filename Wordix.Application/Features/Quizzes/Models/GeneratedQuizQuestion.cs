using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Generator tarafından üretilen tek bir quiz sorusunu temsil eder.
/// 
/// Bu model henüz QuizQuestion entity değildir.
/// Handler bu modeli alıp QuizQuestion ve QuizOption entity'lerine dönüştürecek.
/// </summary>
public sealed class GeneratedQuizQuestion
{
    /// <summary>
    /// Sorunun üretildiği kullanıcı dictionary item id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Sorunun üretildiği global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer soru Word üzerinden üretildiyse Word id değeridir.
    /// </summary>
    public Guid? WordId { get; init; }


    /// <summary>
    /// Eğer soru Phrase üzerinden üretildiyse Phrase id değeridir.
    /// Word sorularında null olur.
    /// </summary>
    public Guid? PhraseId { get; init; }


    /// <summary>
    /// Eğer soru Sentence üzerinden üretildiyse Sentence id değeridir.
    /// Word/Phrase sorularında null olur.
    /// </summary>
    public Guid? SentenceId { get; init; }


    /// <summary>
    /// Sorunun üretildiği içerik tipidir.
    /// 
    /// Response mapping sırasında hardcoded Word kullanmamak için tutulur.
    /// </summary>
    public LearningItemType ItemType { get; init; }


    /// <summary>
    /// Soru metnidir.
    /// 
    /// İlk prototipte İngilizce kelime olur.
    /// Örnek:
    /// achieve
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;


    /// <summary>
    /// Bu generated question sistem önerisi candidate üzerinden mi üretildi?
    /// 
    /// QuizQuestion.IsSystemRecommended alanına yazılacak bilgidir.
    /// </summary>
    public bool IsSystemRecommended { get; init; }

    /// <summary>
    /// Sistem önerisi ise öneri sebebidir.
    /// 
    /// Response DTO'ya taşınır ve QuizRecommendationItem oluştururken kullanılır.
    /// </summary>
    public RecommendationReason? RecommendationReason { get; init; }

    /// <summary>
    /// İlgili recommendation item id değeridir.
    /// 
    /// Question ilk generate edildiğinde null olabilir.
    /// QuizRecommendationItem oluşturulduktan sonra response tarafında doldurulabilir.
    /// </summary>
    public Guid? QuizRecommendationItemId { get; init; }

    /// <summary>
    /// Generated question'ın geldiği candidate'ın zorluk grubudur.
    /// 
    /// Sistem önerisi kayıtlarında snapshot olarak kullanılabilir.
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; init; } = DifficultyGroup.Unknown;

    /// <summary>
    /// Soru sırasıdır.
    /// </summary>
    public int QuestionOrder { get; init; }

    /// <summary>
    /// Soru tipi.
    /// 
    /// Test quiz için:
    /// MultipleChoiceTranslation
    /// 
    /// Writing quiz için:
    /// TranslateToTargetLanguage
    /// </summary>
    public string QuestionType { get; init; } = "MultipleChoiceTranslation";

    /// <summary>
    /// Doğru meaning id değeridir.
    /// 
    /// Bu bilgi backend tarafında QuizQuestion/QuizOption oluştururken kullanılabilir.
    /// API response'ta doğru cevap olarak dönülmez.
    /// </summary>
    public Guid CorrectMeaningId { get; init; }


    /// <summary>
    /// Doğru cevap metnidir.
    /// 
    /// Multiple choice quizde doğru option metniyle aynı değerdir.
    /// Writing quizde kullanıcının yazacağı beklenen cevaptır.
    /// 
    /// Örnek:
    /// - başarmak
    /// - vazgeçmek
    /// - İngilizcemi geliştirmek istiyorum.
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Üretilen seçeneklerdir.
    /// </summary>
    public IReadOnlyCollection<GeneratedQuizOption> Options { get; init; }
        = Array.Empty<GeneratedQuizOption>();
}