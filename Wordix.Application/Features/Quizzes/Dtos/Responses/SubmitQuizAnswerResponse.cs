namespace Wordix.Application.Features.Quizzes.Dtos.Responses;

/// <summary>
/// Kullanıcı quiz cevabı gönderdiğinde API'nin döneceği response modelidir.
/// 
/// Bu response:
/// - Oluşturulan QuizAnswer bilgisini,
/// - Cevabın doğru/yanlış sonucunu,
/// - Seçilen option bilgisini,
/// - Progress güncelleme sonucunu,
/// - Bir sonraki tekrar zamanını
/// frontend'e bildirir.
/// </summary>
public sealed class SubmitQuizAnswerResponse
{
    /// <summary>
    /// Oluşturulan QuizAnswer id değeridir.
    /// </summary>
    public Guid QuizAnswerId { get; init; }

    /// <summary>
    /// Cevap verilen quiz session id değeridir.
    /// </summary>
    public Guid QuizSessionId { get; init; }

    /// <summary>
    /// Cevap verilen quiz question id değeridir.
    /// </summary>
    public Guid QuizQuestionId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği quiz option id değeridir.
    /// </summary>
    public Guid? SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının verdiği cevap doğru mu?
    /// 
    /// Burada doğru/yanlış sonucunu dönebiliriz.
    /// Çünkü kullanıcı cevabı verdikten sonra feedback almalıdır.
    /// 
    /// Dikkat:
    /// Quiz başlatma response'unda doğru cevap dönmüyorduk.
    /// Ama cevap gönderildikten sonra sonucu dönmek normaldir.
    /// </summary>
    public bool IsCorrect { get; init; }


    /// <summary>
    /// Cevabın domain sonucudur.
    /// 
    /// Örnek:
    /// Correct, Incorrect, PartiallyCorrect, Skipped.
    /// </summary>
    public string AnswerResult { get; init; } = string.Empty;

    /// <summary>
    /// Cevap kısmen doğru mu?
    /// Writing quiz için kullanışlıdır.
    /// </summary>
    public bool IsPartiallyCorrect { get; init; }

    /// <summary>
    /// Kullanıcının yazdığı cevap metnidir.
    /// Writing quiz için doludur.
    /// </summary>
    public string? UserAnswerText { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği option text değeridir.
    /// Test quiz için doludur.
    /// Writing quizde boş olabilir.
    /// </summary>
    public string SelectedOptionText { get; init; } = string.Empty;

    /// <summary>
    /// Doğru cevap metnidir.
    /// 
    /// Cevap gönderildikten sonra kullanıcıya doğru cevabı göstermek normaldir.
    /// </summary>
    public string CorrectAnswerText { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// QuizQuestion bazlıdır.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Cevabın backend tarafından kaydedildiği zamandır.
    /// </summary>
    public DateTimeOffset AnsweredAt { get; init; }

    /// <summary>
    /// Güncelleme sonrası doğru cevap sayısıdır.
    /// </summary>
    public int CorrectCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası yanlış cevap sayısıdır.
    /// </summary>
    public int WrongCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası art arda doğru cevap sayısıdır.
    /// </summary>
    public int ConsecutiveCorrectCount { get; init; }

    /// <summary>
    /// Güncelleme sonrası art arda yanlış cevap sayısıdır.
    /// </summary>
    public int ConsecutiveWrongCount { get; init; }

    /// <summary>
    /// Güncelleme öncesi learning status değeridir.
    /// </summary>
    public string PreviousLearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// Güncelleme sonrası learning status değeridir.
    /// </summary>
    public string CurrentLearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// Güncelleme öncesi confidence score değeridir.
    /// </summary>
    public int PreviousConfidenceScore { get; init; }

    /// <summary>
    /// Güncelleme sonrası confidence score değeridir.
    /// </summary>
    public int CurrentConfidenceScore { get; init; }

    /// <summary>
    /// Bir sonraki tekrar tarihi.
    /// 
    /// İlk prototipte basit kuralla hesaplanacak.
    /// İleride spaced repetition algoritması gelişebilir.
    /// </summary>
    public DateTimeOffset? NextReviewDate { get; init; }
}
