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
    /// </summary>
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Doğru cevabın Meaning id değeridir.
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