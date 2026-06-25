using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının bir öğrenilebilir içerikteki progress değişim geçmişini temsil eder.
/// 
/// UserLearningProgress güncel durumu tutar.
/// LearningProgressHistory ise bu durumun zaman içinde nasıl değiştiğini kayıt altına alır.
/// 
/// Örnek:
/// - New durumundan Learning durumuna geçti.
/// - Confidence score 20'den 45'e çıktı.
/// - Yanlış cevap sonrası score 60'tan 48'e düştü.
/// </summary>
public class LearningProgressHistory : BaseEntity
{
    /// <summary>
    /// Değişim geçmişinin ait olduğu progress kaydıdır.
    /// 
    /// Bu alan UserLearningProgress entity'sine bağlanır.
    /// </summary>
    public Guid UserLearningProgressId { get; private set; }

    /// <summary>
    /// Değişiklikten önceki öğrenme durumudur.
    /// Örnek: New, Learning, Reviewing, Learned
    /// </summary>
    public LearningStatus OldLearningStatus { get; private set; }

    /// <summary>
    /// Değişiklikten sonraki öğrenme durumudur.
    /// </summary>
    public LearningStatus NewLearningStatus { get; private set; }

    /// <summary>
    /// Değişiklikten önceki confidence score değeridir.
    /// </summary>
    public int OldConfidenceScore { get; private set; }

    /// <summary>
    /// Değişiklikten sonraki confidence score değeridir.
    /// </summary>
    public int NewConfidenceScore { get; private set; }

    /// <summary>
    /// Progress değişiminin neden gerçekleştiğini açıklar.
    /// 
    /// Örnek:
    /// - Correct quiz answer
    /// - Wrong quiz answer
    /// - Manual review update
    /// - System recalculation
    /// 
    /// İlk prototipte string tutuyoruz.
    /// İleride ihtiyaç olursa ChangeReason için ayrı enum oluşturabiliriz.
    /// </summary>
    public string ChangeReason { get; private set; } = string.Empty;

    /// <summary>
    /// Değişimin oluştuğu zamandır.
    /// 
    /// AuditableEntity kullanmadık çünkü bu kayıt güncellenmesi beklenen bir kayıt değildir.
    /// Oluştuğu anı CreatedAt ile tutmamız yeterlidir.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// EF Core için protected constructor.
    /// Dışarıdan boş history kaydı oluşturulmasını istemiyoruz.
    /// </summary>
    protected LearningProgressHistory()
    {
    }

    /// <summary>
    /// Yeni progress geçmiş kaydı oluşturur.
    /// 
    /// Bu constructor genelde quiz cevabı sonrası progress güncellenirken kullanılacaktır.
    /// </summary>
    public LearningProgressHistory(
        Guid userLearningProgressId,
        LearningStatus oldLearningStatus,
        LearningStatus newLearningStatus,
        int oldConfidenceScore,
        int newConfidenceScore,
        string changeReason)
    {
        if (userLearningProgressId == Guid.Empty)
        {
            throw new ArgumentException("UserLearningProgressId boş Guid olamaz.", nameof(userLearningProgressId));
        }

        ValidateScore(oldConfidenceScore, nameof(oldConfidenceScore));
        ValidateScore(newConfidenceScore, nameof(newConfidenceScore));

        if (string.IsNullOrWhiteSpace(changeReason))
        {
            throw new ArgumentException("ChangeReason boş olamaz.", nameof(changeReason));
        }

        UserLearningProgressId = userLearningProgressId;
        OldLearningStatus = oldLearningStatus;
        NewLearningStatus = newLearningStatus;
        OldConfidenceScore = oldConfidenceScore;
        NewConfidenceScore = newConfidenceScore;
        ChangeReason = changeReason.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confidence score değerinin 0-100 aralığında kalmasını sağlar.
    /// </summary>
    private static void ValidateScore(int score, string parameterName)
    {
        if (score < 0 || score > 100)
        {
            throw new ArgumentException("Confidence score 0 ile 100 arasında olmalıdır.", parameterName);
        }
    }
}