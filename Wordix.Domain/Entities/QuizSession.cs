using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının başlattığı quiz oturumunu temsil eder.
/// 
/// Bir QuizSession içinde birden fazla QuizQuestion bulunabilir.
/// Her QuizQuestion için seçenekler ve kullanıcı cevabı ayrı entity'lerde tutulur.
/// 
/// İlk prototipte:
/// - QuizType genelde Test olacak.
/// - QuizSourceType genelde UserDictionary olacak.
/// - QuizContentMode genelde WordsOnly olacak.
/// </summary>
public class QuizSession : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// Dışarıdan boş QuizSession oluşturulmasını istemiyoruz.
    /// </summary>
    protected QuizSession()
    {
    }

    /// <summary>
    /// Yeni quiz oturumu oluşturur.
    /// </summary>
    public QuizSession(
        Guid userProfileId,
        QuizType quizType,
        QuizSourceType quizSourceType,
        QuizContentMode quizContentMode,
        DifficultyGroup difficultyGroup,
        int questionCount,
        bool includeSystemRecommendations = false,
        Guid? deckId = null)
    {
        if (userProfileId == Guid.Empty)
        {
            throw new ArgumentException("UserProfileId boş Guid olamaz.", nameof(userProfileId));
        }

        if (questionCount <= 0)
        {
            throw new ArgumentException("QuestionCount 0'dan büyük olmalıdır.", nameof(questionCount));
        }

        if (quizSourceType == QuizSourceType.Deck && deckId is null)
        {
            throw new ArgumentException("Deck kaynaklı quiz için DeckId zorunludur.", nameof(deckId));
        }

        UserProfileId = userProfileId;
        QuizType = quizType;
        QuizSourceType = quizSourceType;
        QuizContentMode = quizContentMode;
        DifficultyGroup = difficultyGroup;
        QuestionCount = questionCount;
        IncludeSystemRecommendations = includeSystemRecommendations;
        DeckId = deckId;
        StartedAt = DateTime.UtcNow;
        Status = QuizSessionStatus.InProgress;
    }

    /// <summary>
    /// Quiz'i başlatan kullanıcı profil Id'sidir.
    /// </summary>
    public Guid UserProfileId { get; private set; }

    /// <summary>
    /// Quiz'in genel tipidir.
    /// Örnek: Test, Writing, Mixed
    /// </summary>
    public QuizType QuizType { get; private set; }

    /// <summary>
    /// Quiz sorularının hangi kaynaktan üretildiğini gösterir.
    /// Örnek: UserDictionary, Deck, DifficultItems, SystemRecommendations
    /// </summary>
    public QuizSourceType QuizSourceType { get; private set; }

    /// <summary>
    /// Quiz içinde hangi içerik tiplerinin kullanılacağını gösterir.
    /// Örnek: WordsOnly, PhrasesOnly, SentencesOnly, Mixed
    /// </summary>
    public QuizContentMode QuizContentMode { get; private set; }

    /// <summary>
    /// Quiz için seçilen zorluk grubudur.
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; private set; }

    /// <summary>
    /// Eğer quiz bir deck üzerinden başlatıldıysa ilgili deck Id burada tutulur.
    /// 
    /// İlk prototipte Deck modülü aktif olmadığı için genelde null olacaktır.
    /// </summary>
    public Guid? DeckId { get; private set; }

    /// <summary>
    /// Quiz içine sistem önerisi içerikler dahil edildi mi?
    /// 
    /// İlk prototipte false olabilir.
    /// İleride System Recommendation modülü geldiğinde aktif kullanılacaktır.
    /// </summary>
    public bool IncludeSystemRecommendations { get; private set; }

    /// <summary>
    /// Kullanıcının istediği soru sayısıdır.
    /// </summary>
    public int QuestionCount { get; private set; }

    /// <summary>
    /// Quiz'in başladığı zamandır.
    /// 
    /// UTC tutulur. Frontend gerekirse kullanıcının saat dilimine göre gösterir.
    /// </summary>
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Quiz'in tamamlandığı zamandır.
    /// Quiz henüz devam ediyorsa null kalır.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Quiz oturumunun durumudur.
    /// Örnek: InProgress, Completed, Cancelled
    /// </summary>
    public QuizSessionStatus Status { get; private set; } = QuizSessionStatus.InProgress;
    

    /// <summary>
    /// Quiz oturumunu tamamlanmış hale getirir.
    /// 
    /// Doğru/yanlış sayıları burada tutulmaz.
    /// Çünkü bunlar QuizAnswer kayıtlarından hesaplanabilir.
    /// Böylece aynı veri iki yerde tekrar edilmez.
    /// </summary>
    public void Complete(DateTime? completedAtUtc = null)
    {
        if (Status == QuizSessionStatus.Completed)
        {
            return;
        }

        CompletedAt = completedAtUtc ?? DateTime.UtcNow;
        Status = QuizSessionStatus.Completed;

        MarkAsUpdated();
    }

    /// <summary>
    /// Quiz oturumunu iptal edilmiş hale getirir.
    /// 
    /// MVP'de aktif kullanılmayabilir.
    /// İleride kullanıcı quizden çıkarsa veya sistem quiz'i sonlandırırsa kullanılabilir.
    /// </summary>
    public void Cancel()
    {
        if (Status != QuizSessionStatus.InProgress)
        {
            return;
        }

        Status = QuizSessionStatus.Cancelled;
        MarkAsUpdated();
    }
}