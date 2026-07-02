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
    /// Difficult itemlara öncelik vererek soru adaylarını seçer.
    /// 
    /// Kural:
    /// - Difficult candidate yoksa normal rastgele seçim yapılır.
    /// - Difficult candidate varsa soru sayısının yaklaşık yarısı Difficult itemlardan seçilmeye çalışılır.
    /// - Yeterli Difficult item yoksa kalan normal itemlardan tamamlanır.
    /// - Yeterli normal item yoksa kalan Difficult itemlarla tamamlanır.
    /// - Son seçilen liste tekrar karıştırılır ki response'ta tüm Difficult sorular üst üste gelmesin.
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

        var difficultCandidates = Shuffle(
            candidates.Where(candidate => candidate.IsDifficult));

        var regularCandidates = Shuffle(
            candidates.Where(candidate => !candidate.IsDifficult));

        // Difficult item yoksa eski davranışı koruruz:
        // tüm adaylar arasından rastgele seçim.
        if (difficultCandidates.Count == 0)
        {
            return Shuffle(candidates)
                .Take(actualQuestionCount)
                .ToArray();
        }

        // Soru sayısının yaklaşık yarısını difficult itemlardan seçmeye çalışıyoruz.
        //
        // Örnek:
        // 5 soru istenirse difficult quota 3 olur.
        // 4 soru istenirse difficult quota 2 olur.
        //
        // Math.Max(1, ...) sayesinde difficult varsa en az 1 difficult soru gelmeye çalışır.
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

        // Eğer normal adaylar kalan soru sayısını dolduramazsa,
        // kalan hakkı henüz seçilmemiş difficult adaylardan tamamlarız.
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

        // Son listeyi tekrar karıştırıyoruz.
        // Böylece response içinde Difficult itemlar blok halinde üstte görünmez.
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