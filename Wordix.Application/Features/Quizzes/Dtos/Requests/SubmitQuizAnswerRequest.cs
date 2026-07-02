namespace Wordix.Application.Features.Quizzes.Dtos.Requests;

/// <summary>
/// Kullanıcının quiz sorusuna cevap verirken API'ye göndereceği request modelidir.
/// 
/// Endpoint:
/// POST /api/quizzes/{quizSessionId}/answers
/// 
/// Test quiz örneği:
/// {
///   "selectedQuizOptionId": "00000000-0000-0000-0000-000000000000",
///   "questionResponseTimeInMilliseconds": 3500
/// }
/// 
/// Writing quiz örneği:
/// {
///   "userAnswer": "başarmak",
///   "questionResponseTimeInMilliseconds": 4200
/// }
/// </summary>
public sealed class SubmitQuizAnswerRequest
{

    /// <summary>
    /// Cevap verilen QuizQuestion id değeridir.
    /// 
    /// Writing quiz için zorunludur.
    /// Çünkü Writing quizde QuizOption oluşmaz ve backend soruyu option üzerinden bulamaz.
    /// 
    /// Test quizde zorunlu değildir; Test quizde soru SelectedQuizOptionId üzerinden çözülebilir.
    /// </summary>
    public Guid? QuizQuestionId { get; init; }


    /// <summary>
    /// Kullanıcının seçtiği QuizOption id değeridir.
    /// 
    /// Test quiz için kullanılır.
    /// Writing quizde null olabilir çünkü kullanıcı seçenek seçmez, metin yazar.
    /// 
    /// Önemli:
    /// Bu alan artık nullable'dır.
    /// Çünkü Writing quizde option oluşmaz.
    /// </summary>
    public Guid? SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının yazdığı cevaptır.
    /// 
    /// Writing quiz için kullanılır.
    /// Test quizde null olabilir çünkü kullanıcı option seçer.
    /// 
    /// Örnek:
    /// - başarmak
    /// - vazgeçmek
    /// - İngilizcemi geliştirmek istiyorum.
    /// </summary>
    public string? UserAnswer { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// Sadece cevaplanan QuizQuestion için ölçülen cevap süresidir.
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