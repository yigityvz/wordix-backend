using MediatR;
using Wordix.Application.Features.Quizzes.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// Kullanıcının quiz sorusuna cevap gönderme işlemini temsil eden command modelidir.
/// 
/// Neden command?
/// - QuizAnswer kaydı oluşturulur.
/// - UserLearningProgress güncellenir.
/// - LearningProgressHistory kaydı oluşturulur.
/// - NextReviewDate hesaplanır.
/// 
/// Yani sistem durumunu değiştiren bir işlemdir.
/// Bu yüzden CQRS açısından query değil command'dir.
/// </summary>
public sealed record SubmitQuizAnswerCommand : IRequest<SubmitQuizAnswerResponse>
{
    /// <summary>
    /// Cevap verilen quiz session id değeridir.
    /// 
    /// Bu değer route üzerinden gelir:
    /// POST /api/quizzes/{quizSessionId}/answers
    /// </summary>
    public Guid QuizSessionId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği quiz option id değeridir.
    /// 
    /// Bu değer request body üzerinden gelir.
    /// </summary>
    public Guid SelectedQuizOptionId { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya cevaplama süresidir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// SelectedQuizOptionId üzerinden bulunacak QuizQuestion'a ait cevap süresidir.
    /// 
    /// Opsiyoneldir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Constructor.
    /// 
    /// Controller route parametresindeki quizSessionId ile body'deki selectedQuizOptionId
    /// ve questionResponseTimeInMilliseconds değerlerini command'e aktaracak.
    /// </summary>
    public SubmitQuizAnswerCommand(
        Guid quizSessionId,
        Guid selectedQuizOptionId,
        int? questionResponseTimeInMilliseconds)
    {
        QuizSessionId = quizSessionId;
        SelectedQuizOptionId = selectedQuizOptionId;
        QuestionResponseTimeInMilliseconds = questionResponseTimeInMilliseconds;
    }
}