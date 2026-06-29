using Wordix.Application.Features.Quizzes.Commands.StartQuiz;
using Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;
using Wordix.Application.Features.Quizzes.Requests;

namespace Wordix.Application.Features.Quizzes.Mappers;

/// <summary>
/// Quizzes feature'ına ait DTO → Command dönüşümlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu sınıf neden var?
/// - Controller içinde property property manual mapping yapmak istemiyoruz.
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi de kullanmak istemiyoruz.
/// - Mapping kurallarını feature seviyesinde, açık ve kontrollü biçimde topluyoruz.
/// 
/// Bu yaklaşım:
/// - Magic mapping değildir.
/// - Controller'ı sadeleştirir.
/// - Mapping kodunu tek yerde toplar.
/// - İleride request DTO değişirse controller değil, mapper güncellenir.
/// </summary>
public static class QuizMapper
{
    /// <summary>
    /// API request DTO'sunu StartQuizCommand modeline dönüştürür.
    /// 
    /// Dikkat:
    /// Controller içinde null body kontrolü yapmıyoruz.
    /// Eğer request null gelirse string alanlar boş kalır, QuestionCount 0 olur.
    /// StartQuizCommandValidator bu değerleri ValidationBehavior üzerinden yakalar.
    /// </summary>
    public static StartQuizCommand ToStartQuizCommand(
        StartQuizRequest? request)
    {
        return new StartQuizCommand
        {
            QuizType = request?.QuizType ?? string.Empty,
            QuizSourceType = request?.QuizSourceType ?? string.Empty,
            QuizContentMode = request?.QuizContentMode ?? string.Empty,
            QuestionCount = request?.QuestionCount ?? 0
        };
    }

    /// <summary>
    /// Route'tan gelen quizSessionId ve API request DTO'sunu
    /// SubmitQuizAnswerCommand modeline dönüştürür.
    /// 
    /// Bu endpoint iki farklı kaynaktan veri alır:
    /// - quizSessionId route üzerinden gelir.
    /// - selectedQuizOptionId ve questionResponseTimeInMilliseconds body üzerinden gelir.
    /// 
    /// Controller içinde null body veya Guid.Empty kontrolü yapmıyoruz.
    /// Request null gelirse SelectedQuizOptionId Guid.Empty olur.
    /// SubmitQuizAnswerCommandValidator bunu ValidationBehavior üzerinden yakalar.
    /// </summary>
    public static SubmitQuizAnswerCommand ToSubmitQuizAnswerCommand(
        Guid quizSessionId,
        SubmitQuizAnswerRequest? request)
    {
        return new SubmitQuizAnswerCommand(
            quizSessionId,
            request?.SelectedQuizOptionId ?? Guid.Empty,
            request?.QuestionResponseTimeInMilliseconds);
    }
}