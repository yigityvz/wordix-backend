using Wordix.Application.Common.Exceptions;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz cevabı sonrası UserLearningProgress için yeni state hesaplayan servis implementation'ıdır.
/// 
/// Bu class ne yapar?
/// - CorrectCount / WrongCount değerlerini hesaplar.
/// - ConsecutiveCorrectCount / ConsecutiveWrongCount değerlerini hesaplar.
/// - LearningConfidenceScore sonucunu score calculator sonucundan alır.
/// - LearningStatus değerini confidence score'a göre belirler.
/// - RepetitionLevel için ilk basit kuralı uygular.
/// - LastReviewedAt ve geçici NextReviewDate üretir.
/// 
/// Bu class ne yapmaz?
/// - UserLearningProgress entity'sini doğrudan değiştirmez.
/// - LearningProgressHistory entity'si oluşturmaz.
/// - ReviewScheduleEvent entity'si oluşturmaz.
/// - DbContext veya repository kullanmaz.
/// </summary>
public sealed class LearningProgressUpdater : ILearningProgressUpdater
{
    /// <summary>
    /// Confidence score düşükse item hâlâ yeni/zayıf kabul edilir.
    /// </summary>
    private const int NewStatusUpperBound = 30;

    /// <summary>
    /// 31-60 arası gelişiyor kabul edilir.
    /// </summary>
    private const int LearningStatusUpperBound = 60;

    /// <summary>
    /// 61-85 arası güçlü öğrenme/review aşaması kabul edilir.
    /// </summary>
    private const int ReviewStatusUpperBound = 85;

    /// <summary>
    /// 86-100 arası mastered/öğrenildi kabul edilir.
    /// </summary>
    private const int MasteredStatusLowerBound = 86;

    /// <summary>
    /// Yeni progress state'ini hesaplar.
    /// </summary>
    public LearningProgressUpdateResult CalculateUpdate(
        LearningProgressUpdateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var newCorrectCount = request.IsCorrect
            ? request.CurrentCorrectCount + 1
            : request.CurrentCorrectCount;

        var newWrongCount = request.IsCorrect
            ? request.CurrentWrongCount
            : request.CurrentWrongCount + 1;

        var newConsecutiveCorrectCount = request.IsCorrect
            ? request.CurrentConsecutiveCorrectCount + 1
            : 0;

        var newConsecutiveWrongCount = request.IsCorrect
            ? 0
            : request.CurrentConsecutiveWrongCount + 1;

        var newRepetitionLevel = CalculateRepetitionLevel(
            request.CurrentRepetitionLevel,
            request.IsCorrect);

        var newConfidenceScore = request.ScoreCalculationResult.NewConfidenceScore;

        var newLearningStatus = ResolveLearningStatus(
            newConfidenceScore,
            request.CurrentLearningStatus);

        var nextReviewDate = request.ReviewScheduleEvent.NextReviewDate;

        return new LearningProgressUpdateResult
        {
            UserLearningProgressId = request.UserLearningProgressId,
            UserLearningItemId = request.UserLearningItemId,

            PreviousLearningStatus = request.CurrentLearningStatus,
            NewLearningStatus = newLearningStatus,

            PreviousConfidenceScore = request.ScoreCalculationResult.PreviousConfidenceScore,
            NewConfidenceScore = newConfidenceScore,

            CorrectCount = newCorrectCount,
            WrongCount = newWrongCount,
            ConsecutiveCorrectCount = newConsecutiveCorrectCount,
            ConsecutiveWrongCount = newConsecutiveWrongCount,
            RepetitionLevel = newRepetitionLevel,

            LastReviewedAt = request.ReviewedAt,
            NextReviewDate = nextReviewDate,

            ReviewIntervalDays = request.ReviewScheduleEvent.ReviewIntervalDays,
            ReviewScheduleReason = request.ReviewScheduleEvent.Reason,

            ChangeReason = BuildChangeReason(
                isCorrect: request.IsCorrect,
                scoreReason: request.ScoreCalculationResult.ChangeReason,
                previousStatus: request.CurrentLearningStatus,
                newStatus: newLearningStatus)
        };
    }

    /// <summary>
    /// Request minimum olarak anlamlı mı kontrol eder.
    /// </summary>
    private static void ValidateRequest(LearningProgressUpdateRequest request)
    {
        if (request.UserLearningProgressId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "User learning progress id is required.",
                "USER_LEARNING_PROGRESS_ID_REQUIRED");
        }

        if (request.UserLearningItemId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "User learning item id is required.",
                "USER_LEARNING_ITEM_ID_REQUIRED");
        }

        if (request.ScoreCalculationResult is null)
        {
            throw new BusinessRuleException(
                "Score calculation result is required.",
                "SCORE_CALCULATION_RESULT_REQUIRED");
        }

        if (request.ReviewedAt == default)
        {
            throw new BusinessRuleException(
                "Reviewed at value is required.",
                "REVIEWED_AT_REQUIRED");
        }

        if (request.ReviewScheduleEvent is null)
        {
            throw new BusinessRuleException(
                "Review schedule event is required.",
                "REVIEW_SCHEDULE_EVENT_REQUIRED");
        }
    }

    /// <summary>
    /// İlk basit repetition level kuralı.
    /// 
    /// Doğru cevapta tekrar seviyesi 1 artar.
    /// Yanlış cevapta 0'a düşer.
    /// 
    /// Faz 16F ve ileride spaced repetition algoritmasıyla bu kural gelişebilir.
    /// </summary>
    private static int CalculateRepetitionLevel(
        int currentRepetitionLevel,
        bool isCorrect)
    {
        if (!isCorrect)
        {
            return 0;
        }

        return Math.Max(0, currentRepetitionLevel) + 1;
    }

    /// <summary>
    /// Confidence score'a göre learning status belirler.
    /// 
    /// Burada enum isimlerine doğrudan sıkı bağımlı olmamak için güvenli resolver kullanıyoruz.
    /// Çünkü mevcut LearningStatus enum değerleri değişirse compile kırılmasın.
    /// </summary>
    private static LearningStatus ResolveLearningStatus(
        int confidenceScore,
        LearningStatus fallbackStatus)
    {
        if (confidenceScore >= MasteredStatusLowerBound)
        {
            return ResolveStatusByAliases(
                fallbackStatus,
                "Mastered",
                "Learned",
                "Completed",
                "Known");
        }

        if (confidenceScore > LearningStatusUpperBound
            && confidenceScore <= ReviewStatusUpperBound)
        {
            return ResolveStatusByAliases(
                fallbackStatus,
                "Review",
                "Reviewing",
                "Practicing",
                "Learning");
        }

        if (confidenceScore > NewStatusUpperBound
            && confidenceScore <= LearningStatusUpperBound)
        {
            return ResolveStatusByAliases(
                fallbackStatus,
                "Learning",
                "InProgress",
                "Practicing");
        }

        return ResolveStatusByAliases(
            fallbackStatus,
            "New",
            "NotStarted",
            "Unknown");
    }

    /// <summary>
    /// Verilen alias isimlerinden domain enum içinde var olan ilk değeri döner.
    /// Hiçbiri yoksa mevcut status korunur.
    /// 
    /// Bu yöntem sayesinde bu servis mevcut enum isimlerine aşırı sıkı bağlanmaz.
    /// </summary>
    private static LearningStatus ResolveStatusByAliases(
        LearningStatus fallbackStatus,
        params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (Enum.TryParse<LearningStatus>(
                    alias,
                    ignoreCase: true,
                    out var parsedStatus))
            {
                return parsedStatus;
            }
        }

        return fallbackStatus;
    }

    /// <summary>
    /// Progress değişimi için açıklama metni üretir.
    /// Bu metin LearningProgressHistory.ChangeReason alanında kullanılabilir.
    /// </summary>
    private static string BuildChangeReason(
        bool isCorrect,
        string scoreReason,
        LearningStatus previousStatus,
        LearningStatus newStatus)
    {
        var answerResultText = isCorrect
            ? "Correct answer"
            : "Wrong answer";

        var statusChangeText = previousStatus.Equals(newStatus)
            ? $"Learning status stayed as {newStatus}."
            : $"Learning status changed from {previousStatus} to {newStatus}.";

        return $"{answerResultText}. {scoreReason} {statusChangeText}";
    }
}