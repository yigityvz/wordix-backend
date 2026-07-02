using Wordix.Domain.Enums;

using Wordix.Application.Features.Quizzes.Models;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// İngilizce kelimeden Türkçe anlam seçmeli soru üreten generator implementation'ıdır.
/// 
/// İlk quiz prototipi şu formattadır:
/// - Soru metni: İngilizce kelime
/// - Seçenekler: Türkçe anlamlar
/// - Doğru cevap: ilgili kelimenin selected/primary meaning değeri
/// 
/// Örnek:
/// Soru: achieve
/// Seçenekler:
/// 1. başarmak  -> doğru
/// 2. çalışmak  -> yanlış
/// 3. öğrenmek  -> yanlış
/// 4. hatırlamak -> yanlış
/// 
/// Bu class ne yapmaz?
/// - DbContext kullanmaz.
/// - Repository kullanmaz.
/// - Entity oluşturmaz.
/// - SaveChanges çağırmaz.
/// - Current user bilmez.
/// 
/// Sadece kendisine verilen adaylardan soru/seçenek planı üretir.
/// </summary>
public sealed class MultipleChoiceTranslationQuestionGenerator : IQuizQuestionGenerator
{
    /// <summary>
    /// Bu generator'ın ürettiği soru tipi.
    /// 
    /// Bu değer ileride QuizQuestion entity'sine veya response'a yazılacak.
    /// </summary>
    private const string QuestionType = "MultipleChoiceTranslation";

    /// <summary>
    /// Verilen aday dictionary item'larından multiple choice translation soruları üretir.
    /// </summary>
    public Task<QuizQuestionGenerationResult> GenerateAsync(
        QuizQuestionGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Generator herhangi bir async I/O yapmıyor.
        // Interface geleceğe hazır olsun diye async sözleşme kullandık.
        cancellationToken.ThrowIfCancellationRequested();

        // Soru üretmeye uygun adayları temizliyoruz.
        // Boş question text, boş answer text veya boş meaning id olan adaylardan soru üretilmez.
        var candidates = NormalizeCandidates(request.Candidates);

        if (request.RequestedQuestionCount <= 0
            || request.OptionCountPerQuestion < 2
            || candidates.Count < request.OptionCountPerQuestion)
        {
            return Task.FromResult(new QuizQuestionGenerationResult());
        }

        // Soru olacak adayları seçiyoruz.
        //
        // Faz 22 itibarıyla Difficult olarak işaretlenen itemlar
        // seçim sırasında öncelikli değerlendirilir.
        var selectedQuestionCandidates = QuizQuestionCandidateSelector
            .SelectWithDifficultPriority(
                candidates,
                request.RequestedQuestionCount);

        var generatedQuestions = new List<GeneratedQuizQuestion>();

        foreach (var questionCandidate in selectedQuestionCandidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var options = CreateOptionsForQuestion(
                questionCandidate,
                candidates,
                request.OptionCountPerQuestion);

            // Yeterli yanlış seçenek üretilemiyorsa bu adaydan soru üretmiyoruz.
            // Örneğin 4 seçenekli test için en az 3 farklı yanlış anlam gerekir.
            if (options.Count < request.OptionCountPerQuestion)
            {
                continue;
            }

            generatedQuestions.Add(new GeneratedQuizQuestion
            {
                UserLearningItemId = questionCandidate.UserLearningItemId,
                LearningItemId = questionCandidate.LearningItemId,
                WordId = questionCandidate.WordId,
                PhraseId = questionCandidate.PhraseId,
                SentenceId = questionCandidate.SentenceId,
                ItemType = questionCandidate.ItemType,
                QuestionText = BuildQuestionText(questionCandidate),
                QuestionOrder = generatedQuestions.Count + 1,
                QuestionType = QuestionType,
                CorrectMeaningId = questionCandidate.CorrectMeaningId,
                CorrectAnswerText = questionCandidate.CorrectAnswerText.Trim(),
                Options = options
            });
        }

        return Task.FromResult(new QuizQuestionGenerationResult
        {
            Questions = generatedQuestions
        });
    }

    /// <summary>
    /// Generator'a gelen adayları soru üretmeye uygun hale getirir.
    /// 
    /// Burada yaptığımız şey validation değil, defensive filtering'dir.
    /// Asıl request validation StartQuizCommandValidator tarafından yapılır.
    /// </summary>
    private static IReadOnlyList<QuizQuestionCandidate> NormalizeCandidates(
        IReadOnlyCollection<QuizQuestionCandidate> candidates)
    {
        if (candidates.Count == 0)
        {
            return Array.Empty<QuizQuestionCandidate>();
        }

        return candidates
            .Where(candidate =>
                candidate.UserLearningItemId != Guid.Empty
                && candidate.LearningItemId != Guid.Empty
                && candidate.CorrectMeaningId != Guid.Empty
                && !string.IsNullOrWhiteSpace(candidate.QuestionText)
                && !string.IsNullOrWhiteSpace(candidate.CorrectAnswerText))
            .GroupBy(candidate => candidate.LearningItemId)
            .Select(group => group.First())
            .ToArray();
    }

    /// <summary>
    /// Tek bir soru adayı için doğru ve yanlış seçenekleri üretir.
    /// </summary>
    private static IReadOnlyCollection<GeneratedQuizOption> CreateOptionsForQuestion(
        QuizQuestionCandidate questionCandidate,
        IReadOnlyList<QuizQuestionCandidate> allCandidates,
        int optionCountPerQuestion)
    {
        var correctOption = new GeneratedQuizOption
        {
            MeaningId = questionCandidate.CorrectMeaningId,
            OptionText = questionCandidate.CorrectAnswerText.Trim(),
            IsCorrect = true
        };

        var distractorCount = optionCountPerQuestion - 1;

        var distractors = allCandidates
            .Where(candidate => candidate.LearningItemId != questionCandidate.LearningItemId)
            .Where(candidate => !AreSameOptionText(
                candidate.CorrectAnswerText,
                questionCandidate.CorrectAnswerText))
            .GroupBy(candidate => NormalizeOptionText(candidate.CorrectAnswerText))
            .Select(group => group.First())
            .ToArray();

        var selectedDistractors = Shuffle(distractors)
            .Take(distractorCount)
            .Select(candidate => new GeneratedQuizOption
            {
                MeaningId = candidate.CorrectMeaningId,
                OptionText = candidate.CorrectAnswerText.Trim(),
                IsCorrect = false
            })
            .ToArray();

        if (selectedDistractors.Length < distractorCount)
        {
            return Array.Empty<GeneratedQuizOption>();
        }

        var shuffledOptions = Shuffle(new[] { correctOption }.Concat(selectedDistractors))
            .Select((option, index) => new GeneratedQuizOption
            {
                MeaningId = option.MeaningId,
                OptionText = option.OptionText,
                IsCorrect = option.IsCorrect,
                DisplayOrder = index + 1
            })
            .ToArray();

        return shuffledOptions;
    }

    /// <summary>
    /// Koleksiyonu basit şekilde karıştırır.
    /// 
    /// İlk prototip için Random.Shared yeterlidir.
    /// İleride deterministik test ihtiyacı doğarsa random abstraction eklenebilir.
    /// </summary>
    private static IReadOnlyList<T> Shuffle<T>(IEnumerable<T> items)
    {
        return items
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();
    }

    /// <summary>
    /// İki option text aynı mı kontrol eder.
    /// 
    /// Örnek:
    /// "Başarmak" ve " başarmak " aynı kabul edilir.
    /// </summary>
    private static bool AreSameOptionText(string first, string second)
    {
        return string.Equals(
            NormalizeOptionText(first),
            NormalizeOptionText(second),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Option text karşılaştırması için normalize edilmiş değer üretir.
    /// </summary>
    private static string NormalizeOptionText(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Adayın içerik tipine göre kullanıcıya gösterilecek soru metnini üretir.
    /// 
    /// Word için:
    /// What does "achieve" mean?
    /// 
    /// Phrase için:
    /// What does the phrase "give up" mean?
    /// 
    /// Bu kural generator içindedir çünkü soru üretim formatı generator sorumluluğudur.
    /// </summary>
    private static string BuildQuestionText(
        QuizQuestionCandidate candidate)
    {
        var contentText = candidate.QuestionText.Trim();

        return candidate.ItemType switch
        {
            LearningItemType.Phrase => $"What does the phrase \"{contentText}\" mean?",
            LearningItemType.Word => $"What does \"{contentText}\" mean?",
            _ => $"What does \"{contentText}\" mean?"
        };
    }

}