using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// Quiz içine eklenebilecek sistem önerisi itemları local database üzerinden üretir.
/// 
/// Faz 23 ilk implementation:
/// - Dış provider çağırmaz.
/// - AI/ML öneri yapmaz.
/// - Import sistemiyle veri çekmez.
/// - Sadece mevcut LearningItem havuzundan seçim yapar.
/// 
/// Bu servis neden var?
/// - StartQuizCommandHandler içine recommendation algoritması yazmak istemiyoruz.
/// - Quiz başlatma use-case'i zaten yeterince büyük.
/// - Recommendation davranışı ileride gelişeceği için ayrı servis olarak kalmalıdır.
/// 
/// Genişlemeye açık noktalar:
/// - ContentTag / LearningItemTag ile konu bazlı öneri.
/// - Difficult flag'e benzer içerik önerisi.
/// - Kullanıcının yanlış yaptığı itemlara göre öneri.
/// - Provider/import kaynaklı yeni içerikler.
/// - Analytics destekli recommendation scoring.
/// </summary>
public sealed class QuizRecommendationService : IQuizRecommendationService
{
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<SearchSuggestionLog> _searchSuggestionLogRepository;

    public QuizRecommendationService(
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<SearchSuggestionLog> searchSuggestionLogRepository)
    {
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _sentenceRepository = sentenceRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _meaningRepository = meaningRepository;
        _searchSuggestionLogRepository = searchSuggestionLogRepository;
    }

    /// <summary>
    /// Verilen quiz ayarlarına göre sistem önerisi candidate listesi üretir.
    /// 
    /// Önemli:
    /// Bu method QuizQuestion oluşturmaz.
    /// Bu method QuizRecommendationItem oluşturmaz.
    /// Bu method SearchSuggestionLog oluşturmaz.
    /// 
    /// Sadece generator tarafından kullanılabilecek QuizQuestionCandidate üretir.
    /// Kalıcı kayıtlar StartQuizCommandHandler tarafında, gerçek question oluşturulduktan sonra yapılacaktır.
    /// </summary>
    public async Task<QuizRecommendationResult> GetRecommendationsAsync(
        QuizRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestedRecommendationCount <= 0)
        {
            return QuizRecommendationResult.Empty(request.RequestedRecommendationCount);
        }

        var allowedItemTypes = ResolveAllowedLearningItemTypes(
            request.QuizType,
            request.QuizContentMode);

        if (allowedItemTypes.Count == 0)
        {
            return QuizRecommendationResult.Empty(request.RequestedRecommendationCount);
        }

        var excludedLearningItemIds = request.ExcludedLearningItemIds
            .Distinct()
            .ToArray();

        // Kullanıcının daha önce gördüğü önerileri okuyoruz.
        //
        // Bu fazda bu itemları tamamen yasaklamıyoruz.
        // Sadece seçim sırasında geriye itiyoruz.
        // Çünkü küçük seed database'de hiç öneri kalmaması istemeyiz.
        var previousSuggestionLogs = await _searchSuggestionLogRepository.ListAsync(
            log => log.KeycloakUserId == request.KeycloakUserId,
            cancellationToken);

        var previouslySuggestedLearningItemIds = previousSuggestionLogs
            .Select(log => log.LearningItemId)
            .Distinct()
            .ToHashSet();

        // Global content catalog içinden öneriye uygun aktif LearningItem kayıtlarını alıyoruz.
        //
        // ExcludedLearningItemIds:
        // - kullanıcının dictionary itemları olabilir
        // - deck içindeki itemlar olabilir
        // Böylece kullanıcıya zaten çalıştığı item sistem önerisi olarak tekrar gelmez.
        var learningItems = await _learningItemRepository.ListAsync(
            item =>
                item.IsActive &&
                allowedItemTypes.Contains(item.ItemType) &&
                !excludedLearningItemIds.Contains(item.Id),
            cancellationToken);

        if (learningItems.Count == 0)
        {
            return QuizRecommendationResult.Empty(request.RequestedRecommendationCount);
        }

        var selectedLearningItems = SelectLearningItemsForRecommendation(
            learningItems,
            request.PreferredDifficultyGroup,
            previouslySuggestedLearningItemIds,
            request.RequestedRecommendationCount);

        if (selectedLearningItems.Count == 0)
        {
            return QuizRecommendationResult.Empty(request.RequestedRecommendationCount);
        }

        var candidates = await BuildCandidatesAsync(
            selectedLearningItems,
            request.PreferredDifficultyGroup,
            cancellationToken);

        return new QuizRecommendationResult
        {
            RequestedRecommendationCount = request.RequestedRecommendationCount,
            Candidates = candidates
        };
    }

    /// <summary>
    /// Quiz type + content mode değerlerine göre önerilebilecek LearningItem tiplerini çözer.
    /// 
    /// Test quiz:
    /// - Word ve Phrase desteklenir.
    /// - Sentence desteklenmez çünkü çoktan seçmeli sentence translation akışı henüz yok.
    /// 
    /// Writing quiz:
    /// - Word, Phrase ve Sentence desteklenir.
    /// </summary>
    private static IReadOnlyCollection<LearningItemType> ResolveAllowedLearningItemTypes(
        QuizType quizType,
        QuizContentMode quizContentMode)
    {
        if (quizType == QuizType.Test)
        {
            return quizContentMode switch
            {
                QuizContentMode.WordsOnly => new[] { LearningItemType.Word },
                QuizContentMode.PhrasesOnly => new[] { LearningItemType.Phrase },
                QuizContentMode.Mixed => new[] { LearningItemType.Word, LearningItemType.Phrase },
                _ => Array.Empty<LearningItemType>()
            };
        }

        if (quizType == QuizType.Writing)
        {
            return quizContentMode switch
            {
                QuizContentMode.WordsOnly => new[] { LearningItemType.Word },
                QuizContentMode.PhrasesOnly => new[] { LearningItemType.Phrase },
                QuizContentMode.SentencesOnly => new[] { LearningItemType.Sentence },
                QuizContentMode.Mixed => new[]
                {
                    LearningItemType.Word,
                    LearningItemType.Phrase,
                    LearningItemType.Sentence
                },
                _ => Array.Empty<LearningItemType>()
            };
        }

        return Array.Empty<LearningItemType>();
    }

    /// <summary>
    /// LearningItem listesinden recommendation önceliğine göre seçim yapar.
    /// 
    /// Öncelik:
    /// 1. PreferredDifficultyGroup ile eşleşen ve daha önce önerilmemiş itemlar.
    /// 2. PreferredDifficultyGroup ile eşleşen ama daha önce önerilmiş itemlar.
    /// 3. Diğer zorluklardaki ve daha önce önerilmemiş itemlar.
    /// 4. Diğer zorluklardaki ve daha önce önerilmiş itemlar.
    /// 
    /// Her grubun kendi içinde random çalışması için Shuffle uygulanır.
    /// </summary>
    private static IReadOnlyCollection<LearningItem> SelectLearningItemsForRecommendation(
        IReadOnlyCollection<LearningItem> learningItems,
        DifficultyGroup preferredDifficultyGroup,
        IReadOnlySet<Guid> previouslySuggestedLearningItemIds,
        int requestedRecommendationCount)
    {
        var preferredFresh = Shuffle(
            learningItems.Where(item =>
                item.DifficultyGroup == preferredDifficultyGroup &&
                !previouslySuggestedLearningItemIds.Contains(item.Id)));

        var preferredPreviouslySuggested = Shuffle(
            learningItems.Where(item =>
                item.DifficultyGroup == preferredDifficultyGroup &&
                previouslySuggestedLearningItemIds.Contains(item.Id)));

        var otherFresh = Shuffle(
            learningItems.Where(item =>
                item.DifficultyGroup != preferredDifficultyGroup &&
                !previouslySuggestedLearningItemIds.Contains(item.Id)));

        var otherPreviouslySuggested = Shuffle(
            learningItems.Where(item =>
                item.DifficultyGroup != preferredDifficultyGroup &&
                previouslySuggestedLearningItemIds.Contains(item.Id)));

        return preferredFresh
            .Concat(preferredPreviouslySuggested)
            .Concat(otherFresh)
            .Concat(otherPreviouslySuggested)
            .Take(requestedRecommendationCount)
            .ToArray();
    }

    /// <summary>
    /// Seçilen LearningItem kayıtlarını QuizQuestionCandidate modeline dönüştürür.
    /// 
    /// Bu method Word/Phrase için Meaning tablosunu,
    /// Sentence için SentenceTranslation tablosunu kullanır.
    /// </summary>
    private async Task<IReadOnlyCollection<QuizQuestionCandidate>> BuildCandidatesAsync(
        IReadOnlyCollection<LearningItem> selectedLearningItems,
        DifficultyGroup preferredDifficultyGroup,
        CancellationToken cancellationToken)
    {
        var learningItemIds = selectedLearningItems
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var words = await _wordRepository.ListAsync(
            word => learningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        var phrases = await _phraseRepository.ListAsync(
            phrase => learningItemIds.Contains(phrase.LearningItemId),
            cancellationToken);

        var phraseLookup = phrases.ToDictionary(phrase => phrase.LearningItemId);

        var sentences = await _sentenceRepository.ListAsync(
            sentence =>
                sentence.LearningItemId.HasValue &&
                learningItemIds.Contains(sentence.LearningItemId.Value),
            cancellationToken);

        var sentenceLookup = sentences
            .Where(sentence => sentence.LearningItemId.HasValue)
            .GroupBy(sentence => sentence.LearningItemId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.First());

        var sentenceIds = sentences
            .Select(sentence => sentence.Id)
            .Distinct()
            .ToArray();

        var sentenceTranslations = sentenceIds.Length == 0
            ? Array.Empty<SentenceTranslation>()
            : await _sentenceTranslationRepository.ListAsync(
                translation => sentenceIds.Contains(translation.SourceSentenceId),
                cancellationToken);

        var translationsBySentenceId = sentenceTranslations
            .Where(translation => !string.IsNullOrWhiteSpace(translation.TranslatedText))
            .GroupBy(translation => translation.SourceSentenceId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(translation => translation.IsPrimary)
                    .ThenBy(translation => translation.DisplayOrder)
                    .First());

        var meanings = await _meaningRepository.ListAsync(
            meaning => learningItemIds.Contains(meaning.LearningItemId),
            cancellationToken);

        var meaningsByLearningItemId = meanings
            .GroupBy(meaning => meaning.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(meaning => meaning.IsPrimary)
                    .ThenBy(meaning => meaning.DisplayOrder)
                    .ToArray());

        var candidates = new List<QuizQuestionCandidate>();

        foreach (var learningItem in selectedLearningItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (learningItem.ItemType == LearningItemType.Sentence)
            {
                TryAddSentenceCandidate(
                    learningItem,
                    sentenceLookup,
                    translationsBySentenceId,
                    candidates);

                continue;
            }

            TryAddWordOrPhraseCandidate(
                learningItem,
                wordLookup,
                phraseLookup,
                meaningsByLearningItemId,
                preferredDifficultyGroup,
                candidates);
        }

        return candidates;
    }

    /// <summary>
    /// Word veya Phrase LearningItem kaydından recommendation candidate üretir.
    /// </summary>
    private static void TryAddWordOrPhraseCandidate(
        LearningItem learningItem,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Phrase> phraseLookup,
        IReadOnlyDictionary<Guid, Meaning[]> meaningsByLearningItemId,
        DifficultyGroup preferredDifficultyGroup,
        List<QuizQuestionCandidate> candidates)
    {
        if (!meaningsByLearningItemId.TryGetValue(
                learningItem.Id,
                out var itemMeanings))
        {
            return;
        }

        var correctMeaning = itemMeanings.FirstOrDefault();

        if (correctMeaning is null)
        {
            return;
        }

        var contentText = ResolveCandidateContentText(
            learningItem,
            wordLookup,
            phraseLookup);

        if (string.IsNullOrWhiteSpace(contentText))
        {
            return;
        }

        wordLookup.TryGetValue(learningItem.Id, out var word);
        phraseLookup.TryGetValue(learningItem.Id, out var phrase);

        candidates.Add(new QuizQuestionCandidate
        {
            // Sistem önerisi item kullanıcı dictionary'sinden gelmediği için
            // UserLearningItemId yoktur. Bu yüzden Guid.Empty bırakıyoruz.
            UserLearningItemId = Guid.Empty,

            LearningItemId = learningItem.Id,
            WordId = word?.Id,
            PhraseId = phrase?.Id,
            SentenceId = null,
            ItemType = learningItem.ItemType,
            QuestionText = contentText,
            CorrectMeaningId = correctMeaning.Id,
            CorrectAnswerText = correctMeaning.MeaningText,
            PartOfSpeech = correctMeaning.PartOfSpeech,

            IsDifficult = false,
            IsSystemRecommended = true,
            RecommendationReason = ResolveRecommendationReason(
                learningItem,
                preferredDifficultyGroup),
            DifficultyGroup = learningItem.DifficultyGroup
        });
    }

    /// <summary>
    /// Sentence LearningItem kaydından writing recommendation candidate üretir.
    /// </summary>
    private static void TryAddSentenceCandidate(
        LearningItem learningItem,
        IReadOnlyDictionary<Guid, Sentence> sentenceLookup,
        IReadOnlyDictionary<Guid, SentenceTranslation> translationsBySentenceId,
        List<QuizQuestionCandidate> candidates)
    {
        if (!sentenceLookup.TryGetValue(
                learningItem.Id,
                out var sentence))
        {
            return;
        }

        if (!translationsBySentenceId.TryGetValue(
                sentence.Id,
                out var sentenceTranslation))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sentence.Text) ||
            string.IsNullOrWhiteSpace(sentenceTranslation.TranslatedText))
        {
            return;
        }

        candidates.Add(new QuizQuestionCandidate
        {
            // Sistem önerisi item kullanıcı dictionary'sinden gelmediği için
            // UserLearningItemId yoktur.
            UserLearningItemId = Guid.Empty,

            LearningItemId = learningItem.Id,
            WordId = null,
            PhraseId = null,
            SentenceId = sentence.Id,
            ItemType = learningItem.ItemType,
            QuestionText = sentence.Text,
            CorrectMeaningId = Guid.Empty,
            CorrectAnswerText = sentenceTranslation.TranslatedText,
            PartOfSpeech = null,

            IsDifficult = false,
            IsSystemRecommended = true,
            RecommendationReason = ResolveRecommendationReason(
                learningItem,
                learningItem.DifficultyGroup),
            DifficultyGroup = learningItem.DifficultyGroup
        });
    }

    /// <summary>
    /// LearningItem tipine göre kullanıcıya gösterilecek ana metni çözer.
    /// </summary>
    private static string ResolveCandidateContentText(
        LearningItem learningItem,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Phrase> phraseLookup)
    {
        return learningItem.ItemType switch
        {
            LearningItemType.Word when wordLookup.TryGetValue(learningItem.Id, out var word)
                => word.Text,

            LearningItemType.Phrase when phraseLookup.TryGetValue(learningItem.Id, out var phrase)
                => phrase.Text,

            _ => string.Empty
        };
    }

    /// <summary>
    /// Recommendation reason bilgisini belirler.
    /// 
    /// İlk Faz 23 algoritmasında ana sebep zorluk uyumudur.
    /// Eğer item tercih edilen difficulty ile eşleşmiyorsa StarterRecommendation olarak işaretliyoruz.
    /// </summary>
    private static RecommendationReason ResolveRecommendationReason(
        LearningItem learningItem,
        DifficultyGroup preferredDifficultyGroup)
    {
        return learningItem.DifficultyGroup == preferredDifficultyGroup
            ? RecommendationReason.DifficultyLevelMatch
            : RecommendationReason.StarterRecommendation;
    }

    /// <summary>
    /// Koleksiyonu basit şekilde karıştırır.
    /// 
    /// İlk prototip için Random.Shared yeterlidir.
    /// İleride unit testlerde deterministik random ihtiyacı olursa random abstraction eklenebilir.
    /// </summary>
    private static IReadOnlyCollection<T> Shuffle<T>(
        IEnumerable<T> items)
    {
        return items
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();
    }
}