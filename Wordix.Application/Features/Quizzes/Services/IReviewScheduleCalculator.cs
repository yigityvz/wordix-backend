using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabı sonrası bir sonraki tekrar tarihini hesaplayan servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Review schedule algoritması LearningProgressUpdater içine gömülmesin.
/// - İleride spaced repetition algoritması geliştiğinde sadece bu servis değişsin.
/// - Handler, progress update ve schedule hesaplamasını ayrı servislerle yönetebilsin.
/// </summary>
public interface IReviewScheduleCalculator
{
    /// <summary>
    /// Cevap sonucu, confidence score ve repetition bilgisine göre bir sonraki tekrar tarihini hesaplar.
    /// 
    /// Bu method:
    /// - database'e gitmez,
    /// - entity güncellemez,
    /// - history oluşturmaz.
    /// 
    /// Sadece tekrar planlama sonucunu üretir.
    /// </summary>
    ReviewScheduleEvent Calculate(
        ReviewScheduleCalculationRequest request);
}