using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Bir quiz oturumunda sorulan tek bir soruyu temsil eder.
/// 
/// QuizQuestion doğrudan Word'e değil LearningItem'a bağlıdır.
/// Böylece ileride Phrase veya Sentence soruları da aynı quiz altyapısıyla sorulabilir.
/// </summary>
public class QuizQuestion : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected QuizQuestion()
    {
    }

    /// <summary>
    /// Yeni quiz sorusu oluşturur.
    /// </summary>
    public QuizQuestion(
        Guid quizSessionId,
        Guid learningItemId,
        QuestionType questionType,
        string questionText,
        string correctAnswer,
        int displayOrder,
        bool isSystemRecommended = false)
    {
        if (quizSessionId == Guid.Empty)
        {
            throw new ArgumentException("QuizSessionId boş Guid olamaz.", nameof(quizSessionId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (string.IsNullOrWhiteSpace(questionText))
        {
            throw new ArgumentException("QuestionText boş olamaz.", nameof(questionText));
        }

        if (string.IsNullOrWhiteSpace(correctAnswer))
        {
            throw new ArgumentException("CorrectAnswer boş olamaz.", nameof(correctAnswer));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        QuizSessionId = quizSessionId;
        LearningItemId = learningItemId;
        QuestionType = questionType;
        QuestionText = questionText.Trim();
        CorrectAnswer = correctAnswer.Trim();
        DisplayOrder = displayOrder;
        IsSystemRecommended = isSystemRecommended;
    }

    /// <summary>
    /// Sorunun ait olduğu quiz oturumu Id'sidir.
    /// </summary>
    public Guid QuizSessionId { get; private set; }

    /// <summary>
    /// Soruda kullanılan öğrenilebilir içerik Id'sidir.
    /// Örneğin "achieve" kelimesinin LearningItem Id'si.
    /// </summary>
    public Guid LearningItemId { get; private set; }

    /// <summary>
    /// Sorunun tipidir.
    /// Örnek: MultipleChoice, Writing, TranslateToTargetLanguage
    /// </summary>
    public QuestionType QuestionType { get; private set; }

    /// <summary>
    /// Kullanıcıya gösterilecek soru metnidir.
    /// Örnek: "What does achieve mean?"
    /// </summary>
    public string QuestionText { get; private set; } = string.Empty;

    /// <summary>
    /// Sorunun doğru cevabıdır.
    /// 
    /// Çoktan seçmeli quizde doğru option da ayrıca tutulur.
    /// Ancak doğru cevabı burada snapshot olarak tutmak,
    /// ileride anlam verisi değişse bile o quizde doğru cevabın ne olduğunu korur.
    /// </summary>
    public string CorrectAnswer { get; private set; } = string.Empty;

    /// <summary>
    /// Sorunun quiz içindeki görüntülenme sırasıdır.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Bu soru sistem önerisiyle mi eklendi?
    /// 
    /// İleride öneri item yanlış bilinirse otomatik dictionary'ye ekleme akışı için önemlidir.
    /// </summary>
    public bool IsSystemRecommended { get; private set; }

    
}