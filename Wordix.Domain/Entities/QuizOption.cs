using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Çoktan seçmeli quizlerde bir soruya ait seçenekleri temsil eder.
/// 
/// Her QuizQuestion birden fazla QuizOption'a sahip olabilir.
/// İlk prototipte test quiz için aktif kullanılacaktır.
/// </summary>
public class QuizOption : BaseEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected QuizOption()
    {
    }

    /// <summary>
    /// Yeni çoktan seçmeli quiz seçeneği oluşturur.
    /// </summary>
    public QuizOption(
        Guid quizQuestionId,
        string optionText,
        bool isCorrect,
        int displayOrder)
    {
        if (quizQuestionId == Guid.Empty)
        {
            throw new ArgumentException("QuizQuestionId boş Guid olamaz.", nameof(quizQuestionId));
        }

        if (string.IsNullOrWhiteSpace(optionText))
        {
            throw new ArgumentException("OptionText boş olamaz.", nameof(optionText));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("DisplayOrder negatif olamaz.", nameof(displayOrder));
        }

        QuizQuestionId = quizQuestionId;
        OptionText = optionText.Trim();
        IsCorrect = isCorrect;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Seçeneğin ait olduğu quiz sorusu Id'sidir.
    /// </summary>
    public Guid QuizQuestionId { get; private set; }

    /// <summary>
    /// Kullanıcıya gösterilecek seçenek metnidir.
    /// Örnek: "başarmak"
    /// </summary>
    public string OptionText { get; private set; } = string.Empty;

    /// <summary>
    /// Bu seçenek doğru cevap mı?
    /// 
    /// Bir QuizQuestion için normalde sadece bir doğru seçenek olmalıdır.
    /// Bu kuralı ileride question generator veya validation tarafında kontrol edeceğiz.
    /// </summary>
    public bool IsCorrect { get; private set; }

    /// <summary>
    /// Seçeneğin görüntülenme sırasıdır.
    /// Örnek: 1, 2, 3, 4
    /// </summary>
    public int DisplayOrder { get; private set; }

    
}