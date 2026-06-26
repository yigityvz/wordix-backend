using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Learning confidence score hesaplayan servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Score hesaplama algoritmasını handler içine gömmemek için.
/// - İleride spaced repetition, zorluk grubu, cevap süresi ve geçmiş performansa göre
///   daha gelişmiş algoritmalar ekleyebilmek için.
/// - Test edilebilirliği artırmak için.
/// </summary>
public interface ILearningScoreCalculator
{
    /// <summary>
    /// Verilen cevap sonucuna göre yeni confidence score hesaplar.
    /// 
    /// Bu method:
    /// - database'e gitmez,
    /// - entity güncellemez,
    /// - progress history oluşturmaz.
    /// 
    /// Sadece hesaplama sonucu üretir.
    /// </summary>
    LearningScoreCalculationResult Calculate(
        LearningScoreCalculationRequest request);
}