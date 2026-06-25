using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının bir quiz sorusuna verdiği cevabı temsil eder.
/// 
/// Doğru/yanlış bilgisinin yanında cevap süresi de tutulur.
/// Kullanıcıya ekranda süre sayacı gösterilmese bile,
/// sistem arka planda cevap süresini öğrenme analitiği için kullanabilir.
/// </summary>
public class QuizAnswer : BaseEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected QuizAnswer()
    {
    }

    /// <summary>
    /// Yeni quiz cevabı oluşturur.
    /// </summary>
    public QuizAnswer(
        Guid quizQuestionId,
        Guid userProfileId,
        string correctAnswer,
        AnswerResult answerResult,
        int responseTimeMilliseconds,
        Guid? selectedQuizOptionId = null,
        string? userAnswer = null,
        bool addedToDictionaryBecauseWrong = false)
    {
        if (quizQuestionId == Guid.Empty)
        {
            throw new ArgumentException("QuizQuestionId boş Guid olamaz.", nameof(quizQuestionId));
        }

        if (userProfileId == Guid.Empty)
        {
            throw new ArgumentException("UserProfileId boş Guid olamaz.", nameof(userProfileId));
        }

        if (string.IsNullOrWhiteSpace(correctAnswer))
        {
            throw new ArgumentException("CorrectAnswer boş olamaz.", nameof(correctAnswer));
        }

        if (responseTimeMilliseconds < 0)
        {
            throw new ArgumentException("ResponseTimeMilliseconds negatif olamaz.", nameof(responseTimeMilliseconds));
        }

        QuizQuestionId = quizQuestionId;
        UserProfileId = userProfileId;
        SelectedQuizOptionId = selectedQuizOptionId;
        UserAnswer = string.IsNullOrWhiteSpace(userAnswer) ? null : userAnswer.Trim();
        CorrectAnswer = correctAnswer.Trim();
        AnswerResult = answerResult;
        ResponseTimeMilliseconds = responseTimeMilliseconds;
        AnsweredAt = DateTime.UtcNow;
        AddedToDictionaryBecauseWrong = addedToDictionaryBecauseWrong;
    }

    /// <summary>
    /// Cevabın ait olduğu quiz sorusu Id'sidir.
    /// </summary>
    public Guid QuizQuestionId { get; private set; }

    /// <summary>
    /// Cevabı veren kullanıcı profil Id'sidir.
    /// </summary>
    public Guid UserProfileId { get; private set; }

    /// <summary>
    /// Test quizlerde kullanıcının seçtiği option Id'sidir.
    /// 
    /// Writing quizlerde null olabilir çünkü kullanıcı seçenek seçmez, metin yazar.
    /// </summary>
    public Guid? SelectedQuizOptionId { get; private set; }

    /// <summary>
    /// Kullanıcının verdiği cevap metnidir.
    /// 
    /// Test quizde seçilen option text buraya yazılabilir.
    /// Writing quizde kullanıcının yazdığı cevap tutulur.
    /// </summary>
    public string? UserAnswer { get; private set; }

    /// <summary>
    /// Sorunun doğru cevabının cevap anındaki snapshot değeridir.
    /// 
    /// Böylece ileride Meaning veya Question değişse bile,
    /// kullanıcının o anda neye göre değerlendirildiği kaybolmaz.
    /// </summary>
    public string CorrectAnswer { get; private set; } = string.Empty;

    /// <summary>
    /// Cevabın sonucudur.
    /// 
    /// Güncel karar:
    /// Timeout yok.
    /// Çünkü kullanıcıya süre sayacı gösterilmeyecek.
    /// Cevap süresi ayrı bir analiz verisi olarak tutulacak.
    /// </summary>
    public AnswerResult AnswerResult { get; private set; }

    /// <summary>
    /// Kullanıcının bu soruya kaç milisaniyede cevap verdiğini tutar.
    /// 
    /// Bu veri şunlar için kullanılabilir:
    /// - Hızlı + doğru
    /// - Yavaş + doğru
    /// - Hızlı + yanlış
    /// - Yavaş + yanlış
    /// 
    /// Kullanıcı bunu ekranda süre baskısı olarak görmez.
    /// Sistem arka planda öğrenme analitiği için kullanır.
    /// </summary>
    public int ResponseTimeMilliseconds { get; private set; }

    /// <summary>
    /// Cevabın verildiği zamandır.
    /// </summary>
    public DateTime AnsweredAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Eğer soru sistem önerisi bir içerikten geldiyse ve kullanıcı yanlış yaptıysa,
    /// sistem bu içeriği dictionary'ye otomatik ekleyebilir.
    /// 
    /// Bu alan o işlemin gerçekleşip gerçekleşmediğini takip eder.
    /// </summary>
    public bool AddedToDictionaryBecauseWrong { get; private set; }

    

    /// <summary>
    /// Sistem önerisi içerik yanlış bilindiği için dictionary'ye eklendiyse işaretler.
    /// 
    /// Bu işlem cevap oluşturulurken de set edilebilir,
    /// ancak bazı akışlarda önce cevap kaydedilip sonra dictionary ekleme yapılabilir.
    /// Bu yüzden ayrıca method bıraktık.
    /// </summary>
    public void MarkAsAddedToDictionaryBecauseWrong()
    {
        if (AddedToDictionaryBecauseWrong)
        {
            return;
        }

        AddedToDictionaryBecauseWrong = true;
    }
}