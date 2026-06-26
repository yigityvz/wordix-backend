using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabını değerlendiren servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Cevap değerlendirme logic'i handler içine gömülmesin.
/// - İleride farklı quiz tipleri için farklı değerlendirme kuralları eklenebilsin.
/// - Test quiz, writing quiz, listening quiz gibi farklı cevap değerlendirme stratejileri geliştirilebilsin.
/// 
/// İlk implementation:
/// QuizAnswerEvaluator
/// olacaktır.
/// </summary>
public interface IQuizAnswerEvaluator
{
    /// <summary>
    /// Kullanıcının verdiği cevabı değerlendirir.
    /// 
    /// Bu method database'e gitmez.
    /// Entity oluşturmaz.
    /// Progress güncellemez.
    /// Sadece kendisine verilen option/question bilgisine göre doğru/yanlış sonucu üretir.
    /// </summary>
    /// <param name="request">Değerlendirme input modelidir.</param>
    /// <returns>Değerlendirme sonucudur.</returns>
    QuizAnswerEvaluationResult Evaluate(QuizAnswerEvaluationRequest request);
}