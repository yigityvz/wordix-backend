using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// İlk basit review schedule hesaplama servisidir.
/// 
/// Bu class ne yapar?
/// - Cevap doğru/yanlış mı bakar.
/// - Confidence score seviyesine bakar.
/// - Repetition level değerini dikkate alır.
/// - Bir sonraki tekrar tarihini hesaplar.
/// 
/// Bu class ne yapmaz?
/// - UserLearningProgress entity'sini değiştirmez.
/// - LearningProgressHistory oluşturmaz.
/// - DbContext veya repository kullanmaz.
/// 
/// İleride gerçek spaced repetition algoritması bu class içinde geliştirilebilir.
/// </summary>
public sealed class ReviewScheduleCalculator : IReviewScheduleCalculator
{
    /// <summary>
    /// Düşük confidence score üst sınırı.
    /// </summary>
    private const int LowConfidenceUpperBound = 30;

    /// <summary>
    /// Orta confidence score üst sınırı.
    /// </summary>
    private const int MediumConfidenceUpperBound = 60;

    /// <summary>
    /// Güçlü confidence score üst sınırı.
    /// </summary>
    private const int StrongConfidenceUpperBound = 85;

    /// <summary>
    /// 10 saniye ve üstü yavaş cevap kabul edilir.
    /// 
    /// Bu süre quiz geneli değil, tek bir QuizQuestion cevaplama süresidir.
    /// </summary>
    private const int SlowQuestionResponseThresholdInMilliseconds = 10_000;

    /// <summary>
    /// Bir sonraki tekrar tarihini hesaplar.
    /// </summary>
    public ReviewScheduleEvent Calculate(
        ReviewScheduleCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var reviewIntervalDays = CalculateReviewIntervalDays(request);

        var nextReviewDate = request.ReviewedAt.AddDays(reviewIntervalDays);

        return new ReviewScheduleEvent
        {
            ReviewedAt = request.ReviewedAt,
            NextReviewDate = nextReviewDate,
            ReviewIntervalDays = reviewIntervalDays,
            IsCorrect = request.IsCorrect,
            NewConfidenceScore = request.NewConfidenceScore,
            Reason = BuildReason(request, reviewIntervalDays)
        };
    }

    /// <summary>
    /// Request minimum olarak anlamlı mı kontrol eder.
    /// </summary>
    private static void ValidateRequest(
        ReviewScheduleCalculationRequest request)
    {
        if (request.ReviewedAt == default)
        {
            throw new BusinessRuleException(
                "Reviewed at value is required for review schedule calculation.",
                "REVIEWED_AT_REQUIRED_FOR_REVIEW_SCHEDULE");
        }

        if (request.NewConfidenceScore is < 0 or > 100)
        {
            throw new BusinessRuleException(
                "New confidence score must be between 0 and 100 for review schedule calculation.",
                "CONFIDENCE_SCORE_OUT_OF_RANGE_FOR_REVIEW_SCHEDULE");
        }

        if (request.CurrentRepetitionLevel < 0)
        {
            throw new BusinessRuleException(
                "Current repetition level cannot be negative.",
                "REPETITION_LEVEL_INVALID_FOR_REVIEW_SCHEDULE");
        }
    }

    /// <summary>
    /// İlk basit tekrar aralığı kuralı.
    /// 
    /// Yanlış cevap:
    /// - Ertesi gün tekrar.
    /// 
    /// Doğru cevap:
    /// - Düşük score: 2 gün
    /// - Orta score: 4 gün
    /// - Güçlü score: 7 gün
    /// - Çok yüksek score: 14 gün
    /// 
    /// Yavaş cevap:
    /// - Doğru olsa bile interval biraz kısaltılır.
    /// 
    /// Repetition level:
    /// - Doğru cevaplarda tekrar seviyesi arttıkça interval biraz uzatılır.
    /// </summary>
    private static int CalculateReviewIntervalDays(
        ReviewScheduleCalculationRequest request)
    {
        if (!request.IsCorrect)
        {
            return 1;
        }

        var baseIntervalDays = CalculateBaseIntervalDaysByConfidence(
            request.NewConfidenceScore);

        var repetitionBonusDays = CalculateRepetitionBonusDays(
            request.CurrentRepetitionLevel);

        var intervalDays = baseIntervalDays + repetitionBonusDays;

        if (IsSlowQuestionResponse(request.QuestionResponseTimeInMilliseconds))
        {
            intervalDays = Math.Max(1, intervalDays - 1);
        }

        return intervalDays;
    }

    /// <summary>
    /// Confidence score seviyesine göre temel tekrar aralığını hesaplar.
    /// </summary>
    private static int CalculateBaseIntervalDaysByConfidence(
        int confidenceScore)
    {
        if (confidenceScore <= LowConfidenceUpperBound)
        {
            return 2;
        }

        if (confidenceScore <= MediumConfidenceUpperBound)
        {
            return 4;
        }

        if (confidenceScore <= StrongConfidenceUpperBound)
        {
            return 7;
        }

        return 14;
    }

    /// <summary>
    /// Repetition level'a göre küçük bonus gün hesaplar.
    /// 
    /// İlk basit kural:
    /// - 0-1 seviye: bonus yok
    /// - 2-3 seviye: +1 gün
    /// - 4+ seviye: +2 gün
    /// </summary>
    private static int CalculateRepetitionBonusDays(
        int currentRepetitionLevel)
    {
        if (currentRepetitionLevel >= 4)
        {
            return 2;
        }

        if (currentRepetitionLevel >= 2)
        {
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Cevap süresi yavaş mı kontrol eder.
    /// </summary>
    private static bool IsSlowQuestionResponse(
        int? questionResponseTimeInMilliseconds)
    {
        return questionResponseTimeInMilliseconds is >= SlowQuestionResponseThresholdInMilliseconds;
    }

    /// <summary>
    /// Review schedule kararına açıklama üretir.
    /// </summary>
    private static string BuildReason(
        ReviewScheduleCalculationRequest request,
        int reviewIntervalDays)
    {
        if (!request.IsCorrect)
        {
            return "Wrong answer. Next review scheduled for tomorrow.";
        }

        var confidenceLabel = GetConfidenceLabel(request.NewConfidenceScore);

        var speedText = IsSlowQuestionResponse(request.QuestionResponseTimeInMilliseconds)
            ? " Slow question response shortened the review interval."
            : string.Empty;

        return $"Correct answer with {confidenceLabel} confidence. Next review scheduled in {reviewIntervalDays} days.{speedText}";
    }

    /// <summary>
    /// Confidence score seviyesini metin olarak döner.
    /// </summary>
    private static string GetConfidenceLabel(
        int confidenceScore)
    {
        if (confidenceScore <= LowConfidenceUpperBound)
        {
            return "low";
        }

        if (confidenceScore <= MediumConfidenceUpperBound)
        {
            return "medium";
        }

        if (confidenceScore <= StrongConfidenceUpperBound)
        {
            return "strong";
        }

        return "very strong";
    }
}