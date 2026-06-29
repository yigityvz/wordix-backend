using MediatR;
using Wordix.Application.Features.Quizzes.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// Kullanıcının quiz başlatma isteğini temsil eden command modelidir.
/// 
/// Neden command?
/// - Quiz başlatıldığında QuizSession oluşturulur.
/// - QuizQuestion kayıtları oluşturulur.
/// - QuizOption kayıtları oluşturulur.
/// 
/// Yani bu işlem sistem durumunu değiştirir.
/// Bu yüzden CQRS açısından Query değil Command olarak modellenir.
/// </summary>
public sealed record StartQuizCommand : IRequest<StartQuizResponse>
{
    /// <summary>
    /// Quiz türüdür.
    /// 
    /// İlk prototipte sadece Test desteklenir.
    /// </summary>
    public string QuizType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz sorularının kaynağıdır.
    /// 
    /// İlk prototipte sadece Dictionary desteklenir.
    /// </summary>
    public string QuizSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz içerik modudur.
    /// 
    /// İlk prototipte sadece WordsOnly desteklenir.
    /// </summary>
    public string QuizContentMode { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının istediği soru sayısıdır.
    /// 
    /// Handler gerçek üretilebilen soru sayısını dictionary durumuna göre belirleyecek.
    /// </summary>
    public int QuestionCount { get; init; }
}