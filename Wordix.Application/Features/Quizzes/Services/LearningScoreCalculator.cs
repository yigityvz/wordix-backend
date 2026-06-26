using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// İlk basit confidence score hesaplama servisidir.
/// 
/// Bu class ne yapar?
/// - Cevap doğruysa confidence score'u artırır.
/// - Cevap yanlışsa confidence score'u azaltır.
/// - Soru bazlı cevap süresi varsa küçük bonus/ceza uygular.
/// - Score değerini 0-100 aralığında tutar.
/// 
/// Bu class ne yapmaz?
/// - UserLearningProgress entity'sini değiştirmez.
/// - LearningProgressHistory oluşturmaz.
/// - QuizAnswer oluşturmaz.
/// - DbContext veya repository kullanmaz.
/// 
/// Entity güncelleme sorumluluğu LearningProgressUpdater'a bırakılacaktır.
/// </summary>
public sealed class LearningScoreCalculator : ILearningScoreCalculator
{
    /// <summary>
    /// Confidence score minimum değeri.
    /// </summary>
    private const int MinimumConfidenceScore = 0;

    /// <summary>
    /// Confidence score maksimum değeri.
    /// </summary>
    private const int MaximumConfidenceScore = 100;

    /// <summary>
    /// Normal doğru cevap artışı.
    /// </summary>
    private const int CorrectAnswerBaseIncrease = 12;

    /// <summary>
    /// Normal yanlış cevap düşüşü.
    /// </summary>
    private const int WrongAnswerBaseDecrease = -10;

    /// <summary>
    /// Hızlı doğru cevap ekstra bonusu.
    /// </summary>
    private const int FastCorrectAnswerBonus = 3;

    /// <summary>
    /// Hızlı yanlış cevap ekstra cezası.
    /// </summary>
    private const int FastWrongAnswerPenalty = -4;

    /// <summary>
    /// Çok yavaş doğru cevap için toplam artış daha düşük tutulur.
    /// </summary>
    private const int SlowCorrectAnswerIncrease = 6;

    /// <summary>
    /// Çok yavaş yanlış cevap için düşüş daha yumuşak tutulur.
    /// </summary>
    private const int SlowWrongAnswerDecrease = -8;

    /// <summary>
    /// 3 saniye ve altı hızlı cevap kabul edilir.
    /// </summary>
    private const int FastResponseThresholdInMilliseconds = 3_000;

    /// <summary>
    /// 10 saniye ve üstü yavaş cevap kabul edilir.
    /// </summary>
    private const int SlowResponseThresholdInMilliseconds = 10_000;

    /// <summary>
    /// Cevap sonucuna göre yeni confidence score hesaplar.
    /// </summary>
    public LearningScoreCalculationResult Calculate(
        LearningScoreCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var previousScore = ClampScore(request.CurrentConfidenceScore);

        var scoreDelta = CalculateScoreDelta(
            isCorrect: request.IsCorrect,
            questionResponseTimeInMilliseconds: request.QuestionResponseTimeInMilliseconds);

        var newScore = ClampScore(previousScore + scoreDelta);

        return new LearningScoreCalculationResult
        {
            PreviousConfidenceScore = previousScore,
            NewConfidenceScore = newScore,
            IsCorrect = request.IsCorrect,
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds,
            ChangeReason = BuildChangeReason(
                request.IsCorrect,
                request.QuestionResponseTimeInMilliseconds,
                scoreDelta)
        };
    }

    /// <summary>
    /// Doğru/yanlış ve soru bazlı cevap süresine göre score değişimini hesaplar.
    /// </summary>
    private static int CalculateScoreDelta(
        bool isCorrect,
        int? questionResponseTimeInMilliseconds)
    {
        if (questionResponseTimeInMilliseconds is null)
        {
            return isCorrect
                ? CorrectAnswerBaseIncrease
                : WrongAnswerBaseDecrease;
        }

        var responseTime = questionResponseTimeInMilliseconds.Value;

        if (isCorrect)
        {
            if (responseTime <= FastResponseThresholdInMilliseconds)
            {
                return CorrectAnswerBaseIncrease + FastCorrectAnswerBonus;
            }

            if (responseTime >= SlowResponseThresholdInMilliseconds)
            {
                return SlowCorrectAnswerIncrease;
            }

            return CorrectAnswerBaseIncrease;
        }

        if (responseTime <= FastResponseThresholdInMilliseconds)
        {
            return WrongAnswerBaseDecrease + FastWrongAnswerPenalty;
        }

        if (responseTime >= SlowResponseThresholdInMilliseconds)
        {
            return SlowWrongAnswerDecrease;
        }

        return WrongAnswerBaseDecrease;
    }

    /// <summary>
    /// Score değerini 0-100 aralığında tutar.
    /// </summary>
    private static int ClampScore(int score)
    {
        return Math.Clamp(
            score,
            MinimumConfidenceScore,
            MaximumConfidenceScore);
    }

    /// <summary>
    /// Score değişim nedenini açıklayan metin üretir.
    /// 
    /// Bu metin ileride LearningProgressHistory.ChangeReason alanına yazılabilir.
    /// </summary>
    private static string BuildChangeReason(
        bool isCorrect,
        int? questionResponseTimeInMilliseconds,
        int scoreDelta)
    {
        if (questionResponseTimeInMilliseconds is null)
        {
            return isCorrect
                ? $"Correct answer. Confidence score changed by {scoreDelta}."
                : $"Wrong answer. Confidence score changed by {scoreDelta}.";
        }

        var speedLabel = GetResponseSpeedLabel(questionResponseTimeInMilliseconds.Value);

        return isCorrect
            ? $"Correct answer with {speedLabel} response. Confidence score changed by {scoreDelta}."
            : $"Wrong answer with {speedLabel} response. Confidence score changed by {scoreDelta}.";
    }

    /// <summary>
    /// Soru bazlı cevap süresini basit şekilde sınıflandırır.
    /// </summary>
    private static string GetResponseSpeedLabel(int questionResponseTimeInMilliseconds)
    {
        if (questionResponseTimeInMilliseconds <= FastResponseThresholdInMilliseconds)
        {
            return "fast";
        }

        if (questionResponseTimeInMilliseconds >= SlowResponseThresholdInMilliseconds)
        {
            return "slow";
        }

        return "normal";
    }
}