using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz soruları üreten servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - StartQuizCommandHandler soru üretme algoritmasını bilmesin.
/// - Farklı quiz tipleri için farklı generator implementation'ları yazılabilsin.
/// - Multiple choice, writing, flashcard gibi türler ayrı generator'lar ile geliştirilebilsin.
/// 
/// İlk implementation:
/// MultipleChoiceTranslationQuestionGenerator
/// olacaktır.
/// </summary>
public interface IQuizQuestionGenerator
{
    /// <summary>
    /// Verilen aday dictionary itemlarından quiz soruları üretir.
    /// 
    /// Bu method entity oluşturmaz.
    /// Sadece GeneratedQuizQuestion ve GeneratedQuizOption modelleri üretir.
    /// Entity oluşturma ve database'e kaydetme işi StartQuizCommandHandler sorumluluğundadır.
    /// </summary>
    /// <param name="request">Soru üretim request'i.</param>
    /// <param name="cancellationToken">Async operasyon iptal token'ı.</param>
    /// <returns>Üretilmiş soru listesi.</returns>
    Task<QuizQuestionGenerationResult> GenerateAsync(
        QuizQuestionGenerationRequest request,
        CancellationToken cancellationToken = default);
}