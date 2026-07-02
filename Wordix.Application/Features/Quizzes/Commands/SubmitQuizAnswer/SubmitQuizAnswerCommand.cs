using MediatR;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// Kullanıcının quiz sorusuna cevap verme use-case command modelidir.
/// 
/// Test quiz:
/// - SelectedQuizOptionId dolu gelir.
/// - QuizQuestionId opsiyoneldir.
/// - UserAnswer genellikle null gelir.
/// 
/// Writing quiz:
/// - QuizQuestionId dolu gelir.
/// - UserAnswer dolu gelir.
/// - SelectedQuizOptionId null gelir.
/// 
/// Hangi alanın zorunlu olduğu quiz session tipine göre handler içinde kontrol edilir.
/// Çünkü validator tek başına quiz tipini database'den bilemez.
/// </summary>
public sealed record SubmitQuizAnswerCommand(
    Guid QuizSessionId,
    Guid? QuizQuestionId,
    Guid? SelectedQuizOptionId,
    string? UserAnswer,
    int? QuestionResponseTimeInMilliseconds)
    : IRequest<SubmitQuizAnswerResponse>;