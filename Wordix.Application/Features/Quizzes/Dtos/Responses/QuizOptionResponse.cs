namespace Wordix.Application.Features.Quizzes.Dtos.Responses;

/// <summary>
/// Quiz sorusundaki tek bir seçeneği temsil eden response modelidir.
/// 
/// İlk prototipte seçenekler Türkçe anlamlardan oluşur.
/// Örnek:
/// A) başarmak
/// B) çalışmak
/// C) hatırlamak
/// D) geliştirmek
/// </summary>
public sealed class QuizOptionResponse
{
    /// <summary>
    /// QuizOption entity id değeridir.
    /// 
    /// Kullanıcı cevap verdiğinde ileride bu option id gönderilecek.
    /// </summary>
    public Guid QuizOptionId { get; init; }

    /// <summary>
    /// Seçenek sırasıdır.
    /// 
    /// Örnek:
    /// 1, 2, 3, 4
    /// 
    /// Frontend bunu isterse A/B/C/D olarak gösterebilir.
    /// </summary>
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Kullanıcıya gösterilecek seçenek metnidir.
    /// 
    /// Örnek:
    /// başarmak
    /// </summary>
    public string OptionText { get; init; } = string.Empty;
}
