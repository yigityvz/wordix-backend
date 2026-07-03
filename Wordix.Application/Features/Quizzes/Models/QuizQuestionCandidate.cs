using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz sorusu üretmek için kullanılabilecek tek bir aday dictionary item'ı temsil eder.
/// 
/// Bu model nereden gelir?
/// - StartQuizCommandHandler kullanıcının dictionary kayıtlarını okur.
/// - UserLearningItem + LearningItem + Word + Meaning bilgilerini toplar.
/// - Bunları QuizQuestionCandidate modeline map eder.
/// - Sonra generator'a verir.
/// 
/// Bu model entity değildir.
/// Sadece soru üretme sürecinde kullanılan application modelidir.
/// </summary>
public sealed class QuizQuestionCandidate
{
    /// <summary>
    /// Kullanıcının kişisel dictionary item id değeridir.
    /// 
    /// Bu id UserLearningItem kaydını temsil eder.
    /// Quiz sorusunun hangi kullanıcı dictionary item'ından üretildiğini bilmek için tutulur.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer aday Word üzerinden geliyorsa Word id değeridir.
    /// İlk prototipte WordsOnly çalıştığımız için çoğunlukla dolu olur.
    /// </summary>
    public Guid? WordId { get; init; }


    /// <summary>
    /// Eğer aday Phrase üzerinden geliyorsa Phrase id değeridir.
    /// Word adaylarında null olur.
    /// </summary>
    public Guid? PhraseId { get; init; }


    /// <summary>
    /// Eğer aday Sentence üzerinden geliyorsa Sentence id değeridir.
    /// Word/Phrase adaylarında null olur.
    /// 
    /// Faz 21 Writing Quiz için önemlidir.
    /// Çünkü Sentence writing sorularında doğru cevap Meaning tablosundan değil,
    /// SentenceTranslation tablosundan gelir.
    /// </summary>
    public Guid? SentenceId { get; init; }

    /// <summary>
    /// Adayın içerik tipidir.
    /// 
    /// Word, Phrase veya ileride Sentence olabilir.
    /// Generator soru metnini üretirken bu bilgiden yararlanabilir.
    /// </summary>
    public LearningItemType ItemType { get; init; }


    /// <summary>
    /// Kullanıcıya soru içinde gösterilecek ana içerik metnidir.
    /// 
    /// Word için:
    /// achieve
    /// 
    /// Phrase için:
    /// give up
    /// 
    /// Sentence için:
    /// I want to improve my English
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;


    /// <summary>
    /// Kullanıcı bu dictionary item'ı Difficult olarak işaretledi mi?
    /// 
    /// Faz 22 UserLearningFlag desteğiyle gelir.
    /// Bu bilgi quiz generator tarafından soru seçimi sırasında öncelik sinyali olarak kullanılır.
    /// 
    /// Önemli:
    /// Bu alan database entity alanı değildir.
    /// Sadece quiz candidate üretimi sırasında kullanılan application model bilgisidir.
    /// </summary>
    public bool IsDifficult { get; init; }


    /// <summary>
    /// Bu candidate sistem önerisi olarak mı geldi?
    /// 
    /// Normal UserDictionary veya Deck itemlarında false olur.
    /// IQuizRecommendationService tarafından üretilen candidate'larda true olur.
    /// </summary>
    public bool IsSystemRecommended { get; init; }

    /// <summary>
    /// Sistem önerisi candidate ise öneri sebebidir.
    /// 
    /// Normal dictionary/deck candidate'larında null olur.
    /// </summary>
    public RecommendationReason? RecommendationReason { get; init; }

    /// <summary>
    /// Candidate'ın zorluk grubudur.
    /// 
    /// Faz 23'te QuizRecommendationItem oluştururken öneri anındaki DifficultyGroup bilgisini
    /// snapshot olarak kaydetmek için kullanılacaktır.
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; init; } = DifficultyGroup.Unknown;

    /// <summary>
    /// Doğru cevabın Meaning id değeridir.
    /// 
    /// Word/Phrase multiple choice sorularında dolu olur.
    /// Sentence writing sorularında doğru cevap SentenceTranslation üzerinden geldiği için boş Guid kalabilir.
    /// </summary>
    public Guid CorrectMeaningId { get; init; }

    /// <summary>
    /// Doğru cevap metnidir.
    /// 
    /// İlk prototipte Türkçe anlam olur.
    /// Örnek:
    /// başarmak
    /// çalışmak
    /// öğrenmek
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kelime türü bilgisidir.
    /// Örnek:
    /// verb, noun, adjective
    /// 
    /// İlk prototipte soru üretiminde zorunlu değil.
    /// İleride aynı türden distractor üretmek için kullanılabilir.
    /// </summary>
    public string? PartOfSpeech { get; init; }
}