namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Learning confidence score hesaplamak için kullanılan request modelidir.
/// 
/// Bu model entity değildir.
/// Sadece LearningScoreCalculator servisine gönderilen hesaplama input'udur.
/// </summary>
public sealed class LearningScoreCalculationRequest
{
    /// <summary>
    /// Güncelleme öncesindeki confidence score değeridir.
    /// 
    /// Beklenen aralık:
    /// 0 - 100
    /// </summary>
    public int CurrentConfidenceScore { get; init; }

    /// <summary>
    /// Kullanıcının cevabı doğru mu?
    /// 
    /// Bu bilgi QuizAnswerEvaluator sonucundan gelir.
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğidir.
    /// 
    /// Bu süre quiz session geneli değildir.
    /// QuizQuestion bazlıdır.
    /// 
    /// Nullable olmasının nedeni:
    /// Her client response time göndermek zorunda olmayabilir.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }
}