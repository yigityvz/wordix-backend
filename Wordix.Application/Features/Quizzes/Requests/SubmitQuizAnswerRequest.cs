namespace Wordix.Application.Features.Quizzes.Requests;

/// <summary>
/// Kullanıcının quiz sorusuna cevap verirken API'ye göndereceği request modelidir.
/// 
/// Endpoint:
/// POST /api/quizzes/{quizSessionId}/answers
/// 
/// İlk prototipte test quiz için kullanıcı seçtiği QuizOption id değerini gönderir.
/// 
/// Örnek JSON:
/// {
///   "selectedQuizOptionId": "00000000-0000-0000-0000-000000000000",
///   "questionResponseTimeInMilliseconds": 3500
/// }
/// </summary>
public sealed class SubmitQuizAnswerRequest
{
    /// <summary>
    /// Kullanıcının seçtiği QuizOption id değeridir.
    /// 
    /// Test quiz için cevap, seçilen seçenek üzerinden verilir.
    /// Backend bu option'ın ilgili quiz session'a ait olup olmadığını kontrol edecek.
    /// </summary>
    public Guid SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// Sadece seçilen option'ın bağlı olduğu QuizQuestion için ölçülen cevap süresidir.
    /// 
    /// Örnek:
    /// - 1200 ms: kullanıcı bu soruya 1.2 saniyede cevap verdi.
    /// - 4500 ms: kullanıcı bu soruya 4.5 saniyede cevap verdi.
    /// 
    /// İlk prototipte opsiyoneldir.
    /// İleride:
    /// - kelime bazlı zorluk analizi,
    /// - hızlı/yavaş cevap analizi,
    /// - confidence score hesabı,
    /// - spaced repetition planlama,
    /// - quiz summary
    /// için kullanılabilir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }
}