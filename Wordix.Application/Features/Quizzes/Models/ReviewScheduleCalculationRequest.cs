namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Bir sonraki tekrar tarihini hesaplamak için kullanılan request modelidir.
/// 
/// Bu model entity değildir.
/// ReviewScheduleCalculator servisine gönderilen hesaplama input'udur.
/// </summary>
public sealed class ReviewScheduleCalculationRequest
{
    /// <summary>
    /// Cevap doğru mu?
    /// </summary>
    public bool IsCorrect { get; init; }

    /// <summary>
    /// Güncelleme sonrası confidence score değeridir.
    /// </summary>
    public int NewConfidenceScore { get; init; }

    /// <summary>
    /// Güncelleme öncesi repetition level değeridir.
    /// </summary>
    public int CurrentRepetitionLevel { get; init; }

    /// <summary>
    /// Kullanıcının bu spesifik soruya kaç milisaniyede cevap verdiğini belirtir.
    /// 
    /// Bu süre quiz geneli değil, soru bazlıdır.
    /// </summary>
    public int? QuestionResponseTimeInMilliseconds { get; init; }

    /// <summary>
    /// Review hesaplamasının yapıldığı zamandır.
    /// 
    /// Handler bunu merkezi olarak DateTimeOffset.UtcNow ile verecek.
    /// </summary>
    public DateTimeOffset ReviewedAt { get; init; }
}