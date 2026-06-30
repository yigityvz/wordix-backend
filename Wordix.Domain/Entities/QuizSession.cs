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
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Bu entity, quiz oturumunu token içindeki "sub" claiminden gelen KeycloakUserId ile ilişkilendirir.
/// </summary>
public class QuizSession : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// EF Core database'den kayıt okurken bu constructor'ı kullanabilir.
    /// Dışarıdan boş QuizSession oluşturulmasını istemediğimiz için protected bırakıyoruz.
    /// </summary>
    protected QuizSession()
    {
    }

    /// <summary>
    /// Yeni quiz oturumu oluşturur.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelir.
    /// - Quiz oturumunun hangi Keycloak kullanıcısına ait olduğunu belirtir.
    /// - Backend tarafından üretilen UserProfileId değildir.
    /// </summary>
    public QuizSession(
        string keycloakUserId,
        QuizType quizType,
        QuizSourceType quizSourceType,
        QuizContentMode quizContentMode,
        DifficultyGroup difficultyGroup,
        int questionCount,
        bool includeSystemRecommendations = false,
        Guid? deckId = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("KeycloakUserId boş olamaz.", nameof(keycloakUserId));
        }

        if (questionCount <= 0)
        {
            throw new ArgumentException("QuestionCount 0'dan büyük olmalıdır.", nameof(questionCount));
        }

        if (quizSourceType == QuizSourceType.Deck && deckId is null)
        {
            throw new ArgumentException("Deck kaynaklı quiz için DeckId zorunludur.", nameof(deckId));
        }

        KeycloakUserId = keycloakUserId.Trim();
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
    /// Quiz oturumunu başlatan Keycloak kullanıcısının id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden gelir.
    /// Wordix backend ayrıca UserProfileId/UserId üretmediği için
    /// kullanıcıya ait quiz oturumları bu alan üzerinden filtrelenir.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

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