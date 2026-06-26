namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz question generator'ın ürettiği sonucu temsil eder.
/// </summary>
public sealed class QuizQuestionGenerationResult
{
    /// <summary>
    /// Üretilen soru listesi.
    /// </summary>
    public IReadOnlyCollection<GeneratedQuizQuestion> Questions { get; init; }
        = Array.Empty<GeneratedQuizQuestion>();

    /// <summary>
    /// Gerçekten üretilen soru sayısıdır.
    /// </summary>
    public int GeneratedQuestionCount => Questions.Count;

    /// <summary>
    /// Soru üretildi mi?
    /// </summary>
    public bool HasQuestions => Questions.Count > 0;
}