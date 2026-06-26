using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabı sonrası learning progress state'ini hesaplayan servis sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Progress güncelleme kuralları handler içine gömülmesin.
/// - Doğru/yanlış sayacı, streak, status ve repetition level hesapları ayrı dursun.
/// - İleride daha gelişmiş adaptive learning kuralları eklenebilsin.
/// </summary>
public interface ILearningProgressUpdater
{
    /// <summary>
    /// Mevcut progress değerleri ve cevap sonucu üzerinden yeni progress state'i hesaplar.
    /// 
    /// Bu method database'e gitmez.
    /// Entity mutate etmez.
    /// SaveChanges çağırmaz.
    /// Sadece yeni progress state sonucunu üretir.
    /// </summary>
    LearningProgressUpdateResult CalculateUpdate(
        LearningProgressUpdateRequest request);
}