using Wordix.Application.Features.Quizzes.Models;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Kullanıcının cevabı kendisinin yazdığı translation soruları üreten generator implementation'ıdır.
/// 
/// Writing quiz formatı:
/// - Kullanıcıya kaynak dilde Word/Phrase/Sentence gösterilir.
/// - Kullanıcı hedef dilde cevabı kendisi yazar.
/// - Seçenek üretilmez.
/// 
/// Örnek Word:
/// QuestionText: Write the Turkish meaning of "achieve".
/// CorrectAnswerText: başarmak
/// 
/// Örnek Phrase:
/// QuestionText: Write the Turkish meaning of the phrase "give up".
/// CorrectAnswerText: vazgeçmek
/// 
/// Örnek Sentence:
/// QuestionText: Translate the sentence: "I want to improve my English".
/// CorrectAnswerText: İngilizcemi geliştirmek istiyorum.
/// 
/// Bu class ne yapmaz?
/// - DbContext kullanmaz.
/// - Repository kullanmaz.
/// - Entity oluşturmaz.
/// - SaveChanges çağırmaz.
/// - Current user bilmez.
/// - Cevap değerlendirme yapmaz.
/// 
/// Sadece kendisine verilen adaylardan writing soru planı üretir.
/// </summary>
public sealed class WrittenTranslationQuestionGenerator : IQuizQuestionGenerator
{
    /// <summary>
    /// Writing quizde kullanılacak domain QuestionType değeridir.
    /// 
    /// Burada string tutuyoruz çünkü mevcut GeneratedQuizQuestion modeli QuestionType'ı string olarak taşıyor.
    /// Handler tarafında QuizMapper.ToDomainQuestionType ile domain enum değerine çevrilecek.
    /// </summary>
    private static readonly string QuestionType =
        Domain.Enums.QuestionType.TranslateToTargetLanguage.ToString();

    /// <summary>
    /// Verilen adaylardan writing translation soruları üretir.
    /// 
    /// Multiple choice generator'dan farkı:
    /// - Option üretmez.
    /// - OptionCountPerQuestion kullanmaz.
    /// - CorrectAnswerText doğrudan QuizQuestion.CorrectAnswer snapshot'ına yazılacak değerdir.
    /// </summary>
    public Task<QuizQuestionGenerationResult> GenerateAsync(
        QuizQuestionGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var candidates = NormalizeCandidates(request.Candidates);

        if (request.RequestedQuestionCount <= 0 || candidates.Count == 0)
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

        var generatedQuestions = selectedQuestionCandidates
            .Select((candidate, index) => new GeneratedQuizQuestion
            {
                UserLearningItemId = candidate.UserLearningItemId,
                LearningItemId = candidate.LearningItemId,
                WordId = candidate.WordId,
                PhraseId = candidate.PhraseId,
                SentenceId = candidate.SentenceId,
                ItemType = candidate.ItemType,
                QuestionText = BuildQuestionText(candidate),
                QuestionOrder = index + 1,
                QuestionType = QuestionType,
                CorrectMeaningId = candidate.CorrectMeaningId,
                CorrectAnswerText = candidate.CorrectAnswerText.Trim(),

                IsSystemRecommended = candidate.IsSystemRecommended,
                RecommendationReason = candidate.RecommendationReason,
                DifficultyGroup = candidate.DifficultyGroup,

                Options = Array.Empty<GeneratedQuizOption>()
            })
            .ToArray();

        return Task.FromResult(new QuizQuestionGenerationResult
        {
            Questions = generatedQuestions
        });
    }

    /// <summary>
    /// Writing soru üretimine uygun adayları filtreler.
    /// 
    /// Multiple choice'dan farklı olarak CorrectMeaningId zorunlu değildir.
    /// Çünkü Sentence writing sorularında doğru cevap SentenceTranslation üzerinden gelebilir.
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
                IsValidCandidateOwner(candidate)
                && candidate.LearningItemId != Guid.Empty
                && !string.IsNullOrWhiteSpace(candidate.QuestionText)
                && !string.IsNullOrWhiteSpace(candidate.CorrectAnswerText))
            .GroupBy(candidate => candidate.LearningItemId)
            .Select(group => group.First())
            .ToArray();
    }

    /// <summary>
    /// Candidate'ın quiz sorusu üretmek için geçerli bir sahiplik bilgisi taşıyıp taşımadığını kontrol eder.
    /// 
    /// Normal UserDictionary/Deck candidate:
    /// - UserLearningItemId dolu olmalıdır.
    /// 
    /// System recommendation candidate:
    /// - Kullanıcının dictionary'sinden gelmediği için UserLearningItemId boş olabilir.
    /// - Bu durumda IsSystemRecommended = true olması yeterlidir.
    /// </summary>
    private static bool IsValidCandidateOwner(
        QuizQuestionCandidate candidate)
    {
        return candidate.UserLearningItemId != Guid.Empty
               || candidate.IsSystemRecommended;
    }

    /// <summary>
    /// Adayın içerik tipine göre kullanıcıya gösterilecek writing soru metnini üretir.
    /// </summary>
    private static string BuildQuestionText(
        QuizQuestionCandidate candidate)
    {
        var contentText = candidate.QuestionText.Trim();

        return candidate.ItemType switch
        {
            LearningItemType.Word =>
                $"Write the Turkish meaning of \"{contentText}\".",

            LearningItemType.Phrase =>
                $"Write the Turkish meaning of the phrase \"{contentText}\".",

            LearningItemType.Sentence =>
                $"Translate the sentence: \"{contentText}\".",

            _ =>
                $"Write the Turkish meaning of \"{contentText}\"."
        };
    }

}