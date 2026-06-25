using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının kaydettiği bir öğrenilebilir içerikteki ilerlemesini temsil eder.
/// 
/// Bu entity quiz ve review algoritmasının temel veri kaynağıdır.
/// Kullanıcının doğru/yanlış sayıları, üst üste doğru/yanlış durumları,
/// confidence score ve sonraki tekrar tarihi burada tutulur.
/// </summary>
public class UserLearningProgress : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected UserLearningProgress()
    {
    }

    /// <summary>
    /// Yeni dictionary kaydı için başlangıç progress kaydı oluşturur.
    /// </summary>
    public UserLearningProgress(Guid userLearningItemId)
    {
        if (userLearningItemId == Guid.Empty)
        {
            throw new ArgumentException("UserLearningItemId boş Guid olamaz.", nameof(userLearningItemId));
        }

        UserLearningItemId = userLearningItemId;
        LearningStatus = LearningStatus.New;
        LearningConfidenceScore = 0;
        RepetitionLevel = 0;
    }

    /// <summary>
    /// Progress kaydının hangi kullanıcı dictionary item'ına ait olduğunu gösterir.
    /// 
    /// Her UserLearningItem için bir UserLearningProgress olması beklenir.
    /// </summary>
    public Guid UserLearningItemId { get; private set; }

    /// <summary>
    /// Kullanıcının bu içerikteki öğrenme durumudur.
    /// Örnek: New, Learning, Reviewing, Learned, Mastered
    /// </summary>
    public LearningStatus LearningStatus { get; private set; } = LearningStatus.New;

    /// <summary>
    /// Kullanıcının bu içerikte verdiği toplam doğru cevap sayısıdır.
    /// </summary>
    public int CorrectCount { get; private set; }

    /// <summary>
    /// Kullanıcının bu içerikte verdiği toplam yanlış cevap sayısıdır.
    /// </summary>
    public int WrongCount { get; private set; }

    /// <summary>
    /// Üst üste doğru cevap sayısıdır.
    /// </summary>
    public int ConsecutiveCorrectCount { get; private set; }

    /// <summary>
    /// Üst üste yanlış cevap sayısıdır.
    /// </summary>
    public int ConsecutiveWrongCount { get; private set; }

    /// <summary>
    /// Kullanıcının bu içeriği öğrenme güven skorudur.
    /// 
    /// Örnek aralık:
    /// 0-30: Öğrenilmedi
    /// 31-60: Gelişiyor
    /// 61-85: Büyük ölçüde öğrenildi
    /// 86-100: Öğrenildi
    /// </summary>
    public int LearningConfidenceScore { get; private set; }

    /// <summary>
    /// Spaced repetition benzeri tekrar seviyesidir.
    /// Doğru cevaplarla artabilir, yanlış cevaplarla düşebilir.
    /// </summary>
    public int RepetitionLevel { get; private set; }

    /// <summary>
    /// İçeriğin kullanıcıya bir sonraki ne zaman tekrar sorulabileceğini gösterir.
    /// </summary>
    public DateTime? NextReviewDate { get; private set; }

    /// <summary>
    /// Kullanıcının bu içeriği en son ne zaman review/quiz içinde gördüğünü tutar.
    /// </summary>
    public DateTime? LastReviewedAt { get; private set; }


    /// <summary>
    /// Kullanıcı doğru cevap verdiğinde progress'i günceller.
    /// 
    /// Not:
    /// Burada basit bir domain davranışı yazıyoruz.
    /// Asıl gelişmiş skor hesaplama ileride LearningScoreCalculator gibi ayrı servislerle yapılacak.
    /// </summary>
    public void RegisterCorrectAnswer(
        int newConfidenceScore,
        int newRepetitionLevel,
        LearningStatus newLearningStatus,
        DateTime? nextReviewDate)
    {
        ValidateScore(newConfidenceScore);
        ValidateRepetitionLevel(newRepetitionLevel);

        CorrectCount++;
        ConsecutiveCorrectCount++;
        ConsecutiveWrongCount = 0;

        LearningConfidenceScore = newConfidenceScore;
        RepetitionLevel = newRepetitionLevel;
        LearningStatus = newLearningStatus;
        LastReviewedAt = DateTime.UtcNow;
        NextReviewDate = nextReviewDate;

        MarkAsUpdated();
    }

    /// <summary>
    /// Kullanıcı yanlış cevap verdiğinde progress'i günceller.
    /// </summary>
    public void RegisterWrongAnswer(
        int newConfidenceScore,
        int newRepetitionLevel,
        LearningStatus newLearningStatus,
        DateTime? nextReviewDate)
    {
        ValidateScore(newConfidenceScore);
        ValidateRepetitionLevel(newRepetitionLevel);

        WrongCount++;
        ConsecutiveWrongCount++;
        ConsecutiveCorrectCount = 0;

        LearningConfidenceScore = newConfidenceScore;
        RepetitionLevel = newRepetitionLevel;
        LearningStatus = newLearningStatus;
        LastReviewedAt = DateTime.UtcNow;
        NextReviewDate = nextReviewDate;

        MarkAsUpdated();
    }

    /// <summary>
    /// Progress değerlerini toplu güncellemek için kullanılır.
    /// 
    /// İleride LearningProgressUpdater gibi bir application/domain service,
    /// hesapladığı sonuçları bu method üzerinden entity'ye uygulayabilir.
    /// </summary>
    public void UpdateProgress(
        LearningStatus learningStatus,
        int learningConfidenceScore,
        int repetitionLevel,
        DateTime? nextReviewDate,
        DateTime? lastReviewedAt)
    {
        ValidateScore(learningConfidenceScore);
        ValidateRepetitionLevel(repetitionLevel);

        LearningStatus = learningStatus;
        LearningConfidenceScore = learningConfidenceScore;
        RepetitionLevel = repetitionLevel;
        NextReviewDate = nextReviewDate;
        LastReviewedAt = lastReviewedAt;

        MarkAsUpdated();
    }

    /// <summary>
    /// Confidence score değerinin 0-100 aralığında kalmasını sağlar.
    /// </summary>
    private static void ValidateScore(int score)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentException("LearningConfidenceScore 0 ile 100 arasında olmalıdır.", nameof(score));
        }
    }

    /// <summary>
    /// Repetition level negatif olamaz.
    /// </summary>
    private static void ValidateRepetitionLevel(int repetitionLevel)
    {
        if (repetitionLevel < 0)
        {
            throw new ArgumentException("RepetitionLevel negatif olamaz.", nameof(repetitionLevel));
        }
    }
}