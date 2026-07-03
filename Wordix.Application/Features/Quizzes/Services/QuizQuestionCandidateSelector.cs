using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz generator'ların soru adayı seçimini merkezi olarak yapan yardımcı class'tır.
/// 
/// Neden ayrı class?
/// - MultipleChoice ve Writing generator aynı Difficult öncelik kuralını kullanacak.
/// - Aynı algoritmayı iki generator içine kopyalamak istemiyoruz.
/// - İleride Favorite, WantMorePractice, confidence score veya due review gibi sinyaller eklenirse
///   soru seçimi tek yerden geliştirilebilir.
/// 
/// Bu class ne yapmaz?
/// - DbContext kullanmaz.
/// - Repository kullanmaz.
/// - Current user bilmez.
/// - Entity oluşturmaz.
/// 
/// Sadece kendisine verilen candidate listesinden hangi candidate'ların soru yapılacağını seçer.
/// </summary>
internal static class QuizQuestionCandidateSelector
{
    /// <summary>
    /// Difficult itemlara ve sistem önerilerine öncelik vererek soru adaylarını seçer.
    /// 
    /// Faz 22:
    /// - Difficult itemlar öncelikli seçilir.
    /// 
    /// Faz 23:
    /// - Sistem önerisi candidate varsa, soru sayısının yaklaşık %30'u kadar öneri seçilmeye çalışılır.
    /// - Sistem önerileri tüm quizi ele geçirmez.
    /// - Normal itemlar quiz içinde kalmaya devam eder.
    /// </summary>
    public static IReadOnlyList<QuizQuestionCandidate> SelectWithDifficultPriority(
        IReadOnlyList<QuizQuestionCandidate> candidates,
        int requestedQuestionCount)
    {
        if (requestedQuestionCount <= 0 || candidates.Count == 0)
        {
            return Array.Empty<QuizQuestionCandidate>();
        }

        var actualQuestionCount = Math.Min(
            requestedQuestionCount,
            candidates.Count);

        var systemRecommendedCandidates = Shuffle(
            candidates.Where(candidate => candidate.IsSystemRecommended));

        var nonSystemCandidates = candidates
            .Where(candidate => !candidate.IsSystemRecommended)
            .ToArray();

        var selectedCandidates = new List<QuizQuestionCandidate>();

        if (systemRecommendedCandidates.Count > 0)
        {
            var recommendationQuota = Math.Max(
                1,
                (int)Math.Floor(actualQuestionCount * 0.30));

            selectedCandidates.AddRange(
                systemRecommendedCandidates.Take(recommendationQuota));
        }

        var remainingQuestionCount = actualQuestionCount - selectedCandidates.Count;

        if (remainingQuestionCount > 0)
        {
            selectedCandidates.AddRange(
                SelectDifficultFirst(
                    nonSystemCandidates,
                    remainingQuestionCount));
        }

        // Eğer normal adaylar kalan sayıyı dolduramadıysa,
        // henüz seçilmemiş sistem önerilerinden tamamlarız.
        var stillNeededQuestionCount = actualQuestionCount - selectedCandidates.Count;

        if (stillNeededQuestionCount > 0)
        {
            var alreadySelectedLearningItemIds = selectedCandidates
                .Select(candidate => candidate.LearningItemId)
                .ToHashSet();

            selectedCandidates.AddRange(
                systemRecommendedCandidates
                    .Where(candidate => !alreadySelectedLearningItemIds.Contains(candidate.LearningItemId))
                    .Take(stillNeededQuestionCount));
        }

        return Shuffle(selectedCandidates)
            .Take(actualQuestionCount)
            .ToArray();
    }


    /// <summary>
    /// Verilen aday havuzunda Difficult itemlara öncelik vererek seçim yapar.
    /// 
    /// Bu helper sistem önerisi dışındaki normal candidate havuzu için kullanılır.
    /// </summary>
    private static IReadOnlyList<QuizQuestionCandidate> SelectDifficultFirst(
        IReadOnlyList<QuizQuestionCandidate> candidates,
        int requestedQuestionCount)
    {
        if (requestedQuestionCount <= 0 || candidates.Count == 0)
        {
            return Array.Empty<QuizQuestionCandidate>();
        }

        var actualQuestionCount = Math.Min(
            requestedQuestionCount,
            candidates.Count);

        var difficultCandidates = Shuffle(
            candidates.Where(candidate => candidate.IsDifficult));

        var regularCandidates = Shuffle(
            candidates.Where(candidate => !candidate.IsDifficult));

        if (difficultCandidates.Count == 0)
        {
            return Shuffle(candidates)
                .Take(actualQuestionCount)
                .ToArray();
        }

        var difficultQuota = Math.Max(
            1,
            (int)Math.Ceiling(actualQuestionCount * 0.5));

        var selectedDifficultCandidates = difficultCandidates
            .Take(difficultQuota)
            .ToList();

        var remainingQuestionCount = actualQuestionCount - selectedDifficultCandidates.Count;

        var selectedRegularCandidates = regularCandidates
            .Take(remainingQuestionCount)
            .ToList();

        var stillNeededQuestionCount =
            actualQuestionCount
            - selectedDifficultCandidates.Count
            - selectedRegularCandidates.Count;

        if (stillNeededQuestionCount > 0)
        {
            selectedDifficultCandidates.AddRange(
                difficultCandidates
                    .Skip(selectedDifficultCandidates.Count)
                    .Take(stillNeededQuestionCount));
        }

        return Shuffle(selectedDifficultCandidates.Concat(selectedRegularCandidates))
            .Take(actualQuestionCount)
            .ToArray();
    }

    /// <summary>
    /// Koleksiyonu basit şekilde karıştırır.
    /// 
    /// İlk prototip için Random.Shared yeterlidir.
    /// İleride unit testlerde deterministik seçim gerekirse random abstraction eklenebilir.
    /// </summary>
    private static IReadOnlyList<T> Shuffle<T>(
        IEnumerable<T> items)
    {
        return items
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();
    }
}