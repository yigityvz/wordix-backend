using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Application.Features.Quizzes.Dtos.Responses;
using Wordix.Application.Features.Quizzes.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;
using Wordix.Application.Features.Quizzes.Mappers;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// StartQuizCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Kullanıcının dictionary itemlarını okur.
/// - İlk prototipte sadece Word itemlardan soru üretir.
/// - Dictionary itemlarını QuizQuestionCandidate modeline dönüştürür.
/// - IQuizQuestionGenerator ile soru/seçenek planı üretir.
/// - QuizSession oluşturur.
/// - QuizQuestion kayıtları oluşturur.
/// - QuizOption kayıtları oluşturur.
/// - Oluşturulan quiz bilgisini response olarak döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Quiz oturumu token içindeki KeycloakUserId ile kullanıcıya bağlanır.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir use-case akışıdır.
/// - HTTP detayı bilmez.
/// - DbContext bilmez.
/// - Keycloak claim detayı bilmez.
/// - Repository ve servis abstraction'ları üzerinden çalışır.
/// </summary>
public sealed class StartQuizCommandHandler
    : IRequestHandler<StartQuizCommand, StartQuizResponse>
{
    /// <summary>
    /// İlk prototipte her soru 4 seçenekli olacak.
    /// </summary>
    private const int OptionCountPerQuestion = 4;

    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<UserLearningItem> _genericUserLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;
    private readonly IRepository<UserLearningFlag> _userLearningFlagRepository;
    private readonly IRepository<UserPreference> _userPreferenceRepository;
    private readonly IRepository<QuizRecommendationItem> _quizRecommendationItemRepository;
    private readonly IRepository<SearchSuggestionLog> _searchSuggestionLogRepository;
    private readonly IQuizRecommendationService _quizRecommendationService;
    private readonly IRepository<QuizSession> _quizSessionRepository;
    private readonly IRepository<QuizQuestion> _quizQuestionRepository;
    private readonly IRepository<QuizOption> _quizOptionRepository;
    private readonly IQuizQuestionGeneratorResolver _quizQuestionGeneratorResolver;
    
    /// <summary>
    /// Handler ihtiyacı olan tüm servis ve repository abstraction'larını DI üzerinden alır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden alınır.
    /// Soru üretme algoritması da direkt burada yazılmaz; IQuizQuestionGenerator kullanılır.
    /// </summary>
    public StartQuizCommandHandler(
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<UserLearningItem> genericUserLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository,
        IRepository<UserLearningFlag> userLearningFlagRepository,
        IRepository<UserPreference> userPreferenceRepository,
        IRepository<QuizRecommendationItem> quizRecommendationItemRepository,
        IRepository<SearchSuggestionLog> searchSuggestionLogRepository,
        IQuizRecommendationService quizRecommendationService,
        IRepository<QuizSession> quizSessionRepository,
        IRepository<QuizQuestion> quizQuestionRepository,
        IRepository<QuizOption> quizOptionRepository,
        IQuizQuestionGeneratorResolver quizQuestionGeneratorResolver
        )
    {
        _userLearningItemRepository = userLearningItemRepository;
        _genericUserLearningItemRepository = genericUserLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _sentenceRepository = sentenceRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _meaningRepository = meaningRepository;
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
        _userLearningFlagRepository = userLearningFlagRepository;
        _userPreferenceRepository = userPreferenceRepository;
        _quizRecommendationItemRepository = quizRecommendationItemRepository;
        _searchSuggestionLogRepository = searchSuggestionLogRepository;
        _quizRecommendationService = quizRecommendationService;
        _quizSessionRepository = quizSessionRepository;
        _quizQuestionRepository = quizQuestionRepository;
        _quizOptionRepository = quizOptionRepository;
        _quizQuestionGeneratorResolver = quizQuestionGeneratorResolver;
        
    }

    /// <summary>
    /// StartQuizCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<StartQuizResponse> Handle(
        StartQuizCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer CurrentUserBehavior tarafından pipeline içinde doldurulur.
        // Handler artık ICurrentUserService'i doğrudan çağırmaz.
        var keycloakUserId = request.KeycloakUserId;

        // Request string değerlerini domain enum değerlerine çeviriyoruz.
        var quizType = ParseQuizType(request.QuizType);
        var quizSourceType = ParseQuizSourceType(request.QuizSourceType);
        var quizContentMode = ParseQuizContentMode(request.QuizContentMode);

        // Faz 23:
        // UserPreference varsa quiz default difficulty ve include system recommendation değerlerini dikkate alıyoruz.
        // Preference yoksa /api/profile/me gibi endpointlerde preference oluşturmadığımız için default değerlerle devam ediyoruz.
        var userPreference = await _userPreferenceRepository.FirstOrDefaultAsync(
            preference => preference.KeycloakUserId == keycloakUserId,
            cancellationToken);

        var preferredDifficultyGroup =
            userPreference?.DefaultDifficultyGroup ?? DifficultyGroup.Beginner;

        // Request değeri preference değerini override eder.
        //
        // true  => Bu quiz için önerileri açıkça aç.
        // false => Bu quiz için önerileri açıkça kapat.
        // null  => UserPreference varsa onu kullan, yoksa false kabul et.
        var shouldIncludeSystemRecommendations =
            request.IncludeSystemRecommendations
            ?? userPreference?.IncludeSystemRecommendations
            ?? false;

        // 2. Quiz kaynağına göre kullanılacak UserLearningItem listesini çözüyoruz.
        var userLearningItems = await ResolveUserLearningItemsForQuizSourceAsync(
            keycloakUserId: keycloakUserId,
            quizSourceType: quizSourceType,
            deckId: request.DeckId,
            cancellationToken: cancellationToken);

        if (userLearningItems.Count == 0)
        {
            throw new BusinessRuleException(
                "There are no learning items available for the selected quiz source.",
                "QUIZ_SOURCE_IS_EMPTY");
        }

        // 3. Dictionary/deck itemlarından normal quiz candidate listesi hazırlıyoruz.
        var baseCandidates = await BuildQuestionCandidatesAsync(
            userLearningItems,
            quizType,
            quizContentMode,
            cancellationToken);

        var minimumCandidateCount = ResolveMinimumCandidateCount(quizType);

        // Faz 23:
        // Sistem önerileri normal candidate listesine ek kaynak olarak karışır.
        // Recommendation service QuizQuestion/QuizSession oluşturmaz; sadece candidate üretir.
        var recommendationResult = QuizRecommendationResult.Empty(0);

        if (shouldIncludeSystemRecommendations)
        {
            var recommendationQuota = ResolveRecommendationQuota(
                requestedQuestionCount: request.QuestionCount,
                baseCandidateCount: baseCandidates.Count,
                minimumCandidateCount: minimumCandidateCount);

            if (recommendationQuota > 0)
            {
                var excludedLearningItemIds = await ResolveExcludedLearningItemIdsForRecommendationsAsync(
                    keycloakUserId,
                    userLearningItems,
                    cancellationToken);

                recommendationResult = await _quizRecommendationService.GetRecommendationsAsync(
                    new QuizRecommendationRequest
                    {
                        KeycloakUserId = keycloakUserId,
                        QuizType = quizType,
                        QuizContentMode = quizContentMode,
                        PreferredDifficultyGroup = preferredDifficultyGroup,
                        RequestedRecommendationCount = recommendationQuota,
                        ExcludedLearningItemIds = excludedLearningItemIds
                    },
                    cancellationToken);
            }
        }

        var candidates = baseCandidates
            .Concat(recommendationResult.Candidates)
            .ToArray();

        if (candidates.Length < minimumCandidateCount)
        {
            throw new BusinessRuleException(
                BuildNotEnoughQuizItemsMessage(quizType),
                "NOT_ENOUGH_QUIZ_ITEMS");
        }

        // Kullanıcı 10 soru isteyebilir ama elimizde daha az uygun item varsa
        // üretilebilir maksimum kadar soru oluştururuz.
        var actualQuestionCount = Math.Min(
            request.QuestionCount,
            candidates.Length);

        var quizQuestionGenerator = _quizQuestionGeneratorResolver.Resolve(quizType);

        // 4. Generator request modelini hazırlıyoruz.
        var generationRequest = new QuizQuestionGenerationRequest
        {
            RequestedQuestionCount = actualQuestionCount,

            OptionCountPerQuestion = quizType == QuizType.Test
                ? OptionCountPerQuestion
                : 0,

            Candidates = candidates
        };

        // 5. Soru/seçenek planını seçilen generator'a ürettiriyoruz.
        var generationResult = await quizQuestionGenerator.GenerateAsync(
            generationRequest,
            cancellationToken);

        if (!generationResult.HasQuestions)
        {
            throw new BusinessRuleException(
                BuildNotEnoughQuizItemsMessage(quizType),
                "QUIZ_QUESTIONS_COULD_NOT_BE_GENERATED");
        }

        var hasGeneratedSystemRecommendation = generationResult.Questions
            .Any(question => question.IsSystemRecommended);

        // 6. QuizSession oluşturuyoruz.
        //
        // IncludeSystemRecommendations alanına request niyetini değil,
        // gerçekten quiz içine sistem önerisi girip girmediğini yazıyoruz.
        var quizSession = new QuizSession(
             keycloakUserId,
             quizType,
             quizSourceType,
             quizContentMode,
             preferredDifficultyGroup,
             generationResult.GeneratedQuestionCount,
             includeSystemRecommendations: hasGeneratedSystemRecommendation,
             deckId: quizSourceType == QuizSourceType.Deck
                 ? request.DeckId
                 : null);

        await _quizSessionRepository.AddAsync(
            quizSession,
            cancellationToken);

        // 7. Generated question/option modellerinden domain entity'leri oluşturuyoruz.
        var createdQuestionResponses = new List<QuizQuestionResponse>();

        foreach (var generatedQuestion in generationResult.Questions
            .OrderBy(question => question.QuestionOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var correctAnswerText = QuizMapper.ResolveCorrectAnswerText(generatedQuestion);

            var questionType = QuizMapper.ToDomainQuestionType(generatedQuestion.QuestionType);

            var quizQuestion = new QuizQuestion(
                quizSession.Id,
                generatedQuestion.LearningItemId,
                questionType,
                generatedQuestion.QuestionText,
                correctAnswerText,
                generatedQuestion.QuestionOrder,
                isSystemRecommended: generatedQuestion.IsSystemRecommended);

            await _quizQuestionRepository.AddAsync(
                quizQuestion,
                cancellationToken);

            QuizRecommendationItem? quizRecommendationItem = null;

            // Faz 23:
            // Eğer soru sistem önerisi candidate'ından üretildiyse,
            // hem QuizRecommendationItem hem SearchSuggestionLog kaydı oluşturuyoruz.
            if (generatedQuestion.IsSystemRecommended)
            {
                var recommendationReason =
                    generatedQuestion.RecommendationReason ?? RecommendationReason.Unknown;

                quizRecommendationItem = new QuizRecommendationItem(
                    quizSession.Id,
                    quizQuestion.Id,
                    quizQuestion.LearningItemId,
                    recommendationReason,
                    generatedQuestion.DifficultyGroup);

                await _quizRecommendationItemRepository.AddAsync(
                    quizRecommendationItem,
                    cancellationToken);

                var suggestionLog = new SearchSuggestionLog(
                    keycloakUserId,
                    quizQuestion.LearningItemId,
                    recommendationReason,
                    quizSession.Id,
                    quizRecommendationItem.Id);

                await _searchSuggestionLogRepository.AddAsync(
                    suggestionLog,
                    cancellationToken);
            }

            var createdOptionResponses = new List<QuizOptionResponse>();

            foreach (var generatedOption in generatedQuestion.Options
                .OrderBy(option => option.DisplayOrder))
            {
                var quizOption = new QuizOption(
                    quizQuestion.Id,
                    generatedOption.OptionText,
                    generatedOption.IsCorrect,
                    generatedOption.DisplayOrder);

                await _quizOptionRepository.AddAsync(
                    quizOption,
                    cancellationToken);

                createdOptionResponses.Add(
                     QuizMapper.ToQuizOptionResponse(quizOption));
            }

            createdQuestionResponses.Add(
                QuizMapper.ToQuizQuestionResponse(
                    quizQuestion: quizQuestion,
                    generatedQuestion: generatedQuestion,
                    optionResponses: createdOptionResponses,
                    quizRecommendationItem: quizRecommendationItem));
        }

        

        return QuizMapper.ToStartQuizResponse(
            request: request,
            quizSession: quizSession,
            questionResponses: createdQuestionResponses);
    }


    /// <summary>
    /// Quiz tipine göre minimum gerekli candidate sayısını döner.
    /// 
    /// Test quiz:
    /// - 4 seçenekli olduğu için en az 4 uygun item gerekir.
    /// 
    /// Writing quiz:
    /// - Seçenek olmadığı için en az 1 uygun item yeterlidir.
    /// </summary>
    private static int ResolveMinimumCandidateCount(
        QuizType quizType)
    {
        return quizType switch
        {
            QuizType.Test => OptionCountPerQuestion,
            QuizType.Writing => 1,

            QuizType.Mixed => throw new BusinessRuleException(
                "Mixed quiz type is not supported yet.",
                "QUIZ_TYPE_NOT_SUPPORTED"),

            _ => throw new BusinessRuleException(
                "Quiz type is not supported.",
                "QUIZ_TYPE_NOT_SUPPORTED")
        };
    }

    /// <summary>
    /// Soru üretilemediğinde kullanıcıya quiz tipine uygun hata mesajı döner.
    /// </summary>
    private static string BuildNotEnoughQuizItemsMessage(
        QuizType quizType)
    {
        return quizType switch
        {
            QuizType.Test =>
                "At least 4 saved learning items with meanings are required to start a multiple choice quiz.",

            QuizType.Writing =>
                "At least 1 saved learning item with a valid answer is required to start a writing quiz.",

            _ =>
                "There are not enough learning items to start this quiz."
        };
    }


    /// <summary>
    /// Quiz kaynağına göre hangi UserLearningItem kayıtlarının kullanılacağını çözer.
    /// 
    /// UserDictionary:
    /// - Kullanıcının tüm aktif dictionary itemları kullanılır.
    /// 
    /// Deck:
    /// - Önce deck var mı ve current user'a ait mi kontrol edilir.
    /// - Sonra sadece o deck içindeki UserLearningItem kayıtları kullanılır.
    /// 
    /// Bu method neden var?
    /// - Handle methodunun okunabilir kalmasını sağlar.
    /// - UserDictionary ve Deck kaynak seçimini tek yerde toplar.
    /// - İleride DifficultItems/SystemRecommendations geldiğinde aynı method genişletilebilir.
    /// </summary>
    private async Task<IReadOnlyCollection<UserLearningItem>> ResolveUserLearningItemsForQuizSourceAsync(
        string keycloakUserId,
        QuizSourceType quizSourceType,
        Guid? deckId,
        CancellationToken cancellationToken)
    {
        return quizSourceType switch
        {
            QuizSourceType.UserDictionary => await GetUserDictionaryItemsForQuizAsync(
                keycloakUserId,
                cancellationToken),

            QuizSourceType.Deck => await GetDeckItemsForQuizAsync(
                keycloakUserId,
                deckId,
                cancellationToken),

            _ => throw new BusinessRuleException(
                "Quiz source type is not supported in the current quiz flow.",
                "QUIZ_SOURCE_TYPE_NOT_SUPPORTED")
        };
    }

    /// <summary>
    /// UserDictionary kaynaklı quiz için current user'ın tüm aktif dictionary itemlarını getirir.
    /// </summary>
    private async Task<IReadOnlyCollection<UserLearningItem>> GetUserDictionaryItemsForQuizAsync(
        string keycloakUserId,
        CancellationToken cancellationToken)
    {
        var userLearningItems = await _userLearningItemRepository
            .GetActiveItemsByUserAsync(keycloakUserId, cancellationToken);

        if (userLearningItems.Count == 0)
        {
            throw new BusinessRuleException(
                "You need to save learning items to your dictionary before starting a quiz.",
                "DICTIONARY_IS_EMPTY");
        }

        return userLearningItems;
    }

    /// <summary>
    /// Deck kaynaklı quiz için sadece ilgili deck içindeki UserLearningItem kayıtlarını getirir.
    /// 
    /// Önemli:
    /// DeckItem doğrudan LearningItemId tutmaz.
    /// DeckItem -> UserLearningItem -> LearningItem zinciri kullanılır.
    /// Böylece deck quiz kullanıcı dictionary sistemiyle uyumlu çalışır.
    /// </summary>
    private async Task<IReadOnlyCollection<UserLearningItem>> GetDeckItemsForQuizAsync(
        string keycloakUserId,
        Guid? deckId,
        CancellationToken cancellationToken)
    {
        if (deckId is null || deckId.Value == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Deck id is required when starting a deck quiz.",
                "DECK_ID_REQUIRED_FOR_DECK_QUIZ");
        }

        var deck = await _deckRepository.FirstOrDefaultAsync(
            deck => deck.Id == deckId.Value && deck.IsActive,
            cancellationToken);

        if (deck is null)
        {
            throw new NotFoundException("Deck", deckId.Value);
        }

        if (!string.Equals(
                deck.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot start a quiz from another user's deck.");
        }

        var deckItems = await _deckItemRepository.ListAsync(
            deckItem => deckItem.DeckId == deck.Id,
            cancellationToken);

        if (deckItems.Count == 0)
        {
            throw new BusinessRuleException(
                "You need to add learning items to this deck before starting a quiz.",
                "DECK_IS_EMPTY");
        }

        var userLearningItemIds = deckItems
            .Select(deckItem => deckItem.UserLearningItemId)
            .Distinct()
            .ToArray();

        var userLearningItems = await _genericUserLearningItemRepository.ListAsync(
            item =>
                userLearningItemIds.Contains(item.Id) &&
                item.KeycloakUserId == keycloakUserId &&
                item.IsActive,
            cancellationToken);

        if (userLearningItems.Count == 0)
        {
            throw new BusinessRuleException(
                "This deck does not contain active dictionary items that can be used for a quiz.",
                "DECK_HAS_NO_ACTIVE_ITEMS");
        }

        return userLearningItems;
    }

    /// <summary>
    /// Kullanıcının dictionary kayıtlarından quiz question candidate listesi oluşturur.
    /// 
    /// Faz 18 itibarıyla:
    /// - WordsOnly sadece Word itemları kullanır.
    /// - PhrasesOnly sadece Phrase itemları kullanır.
    /// - Mixed Word + Phrase itemlarını birlikte kullanır.
    /// 
    /// SentencesOnly şimdilik Faz 19'a bırakılmıştır.
    /// </summary>
    private async Task<IReadOnlyCollection<QuizQuestionCandidate>> BuildQuestionCandidatesAsync(
            IReadOnlyCollection<UserLearningItem> userLearningItems,
            QuizType quizType,
            QuizContentMode quizContentMode,
            CancellationToken cancellationToken)
    {
        var learningItemIds = userLearningItems
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();

        var allowedItemTypes = ResolveAllowedLearningItemTypes(
            quizType,
            quizContentMode);

        var learningItems = await _learningItemRepository.ListAsync(
            item => learningItemIds.Contains(item.Id)
                    && item.IsActive
                    && allowedItemTypes.Contains(item.ItemType),
            cancellationToken);

        if (learningItems.Count == 0)
        {
            return Array.Empty<QuizQuestionCandidate>();
        }

        var filteredLearningItemIds = learningItems
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var userLearningItemsByLearningItemId = userLearningItems
            .Where(item => filteredLearningItemIds.Contains(item.LearningItemId))
            .GroupBy(item => item.LearningItemId)
            .ToDictionary(group => group.Key, group => group.First());


        // Faz 22:
        // Difficult flag'i olan UserLearningItem kayıtlarını quiz seçiminde öncelikli kullanacağız.
        //
        // Burada sadece quiz adayı olabilecek filtrelenmiş UserLearningItem id'lerini kullanıyoruz.
        // Böylece current user'a ait olmayan veya bu quiz content mode'a girmeyen flagler dikkate alınmaz.
        var filteredUserLearningItemIds = userLearningItemsByLearningItemId
            .Values
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var difficultFlags = await _userLearningFlagRepository.ListAsync(
            flag =>
                filteredUserLearningItemIds.Contains(flag.UserLearningItemId) &&
                flag.FlagType == UserLearningFlagType.Difficult,
            cancellationToken);

        var difficultUserLearningItemIds = difficultFlags
            .Select(flag => flag.UserLearningItemId)
            .ToHashSet();

        var words = await _wordRepository.ListAsync(
            word => filteredLearningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        var phrases = await _phraseRepository.ListAsync(
            phrase => filteredLearningItemIds.Contains(phrase.LearningItemId),
            cancellationToken);

        var phraseLookup = phrases.ToDictionary(phrase => phrase.LearningItemId);

        // Sentence writing quiz için sentence detaylarını çekiyoruz.
        //
        // Önemli:
        // Sentence entity'de LearningItemId nullable olabilir.
        // Çünkü her sentence quiz/review item olmak zorunda değildir.
        // Ancak quiz adayı olabilmesi için LearningItemId dolu olmalıdır.
        var sentences = await _sentenceRepository.ListAsync(
            sentence =>
                sentence.LearningItemId.HasValue &&
                filteredLearningItemIds.Contains(sentence.LearningItemId.Value),
            cancellationToken);

        var sentenceLookup = sentences
            .Where(sentence => sentence.LearningItemId.HasValue)
            .GroupBy(sentence => sentence.LearningItemId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.First());


        // Sentence sorularının doğru cevabı Meaning tablosundan değil,
        // SentenceTranslation tablosundan gelir.
        var sentenceIds = sentences
            .Select(sentence => sentence.Id)
            .Distinct()
            .ToArray();

        var sentenceTranslations = await _sentenceTranslationRepository.ListAsync(
            translation => sentenceIds.Contains(translation.SourceSentenceId),
            cancellationToken);

        var translationsBySentenceId = sentenceTranslations
            .Where(translation => !string.IsNullOrWhiteSpace(translation.TranslatedText))
            .GroupBy(translation => translation.SourceSentenceId)
            .ToDictionary(
                group => group.Key,
                group => group.First());


        var meanings = await _meaningRepository.ListAsync(
            meaning => filteredLearningItemIds.Contains(meaning.LearningItemId),
            cancellationToken);

        var meaningsByLearningItemId = meanings
            .GroupBy(meaning => meaning.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(meaning => meaning.DisplayOrder)
                    .ToArray());

        var candidates = new List<QuizQuestionCandidate>();

        foreach (var learningItem in learningItems)
        {
            if (!userLearningItemsByLearningItemId.TryGetValue(
                    learningItem.Id,
                    out var userLearningItem))
            {
                continue;
            }

            // Sentence writing soruları Meaning üzerinden değil,
            // SentenceTranslation üzerinden doğru cevap üretir.
            if (learningItem.ItemType == LearningItemType.Sentence)
            {
                if (!sentenceLookup.TryGetValue(
                        learningItem.Id,
                        out var sentence))
                {
                    continue;
                }

                if (!translationsBySentenceId.TryGetValue(
                        sentence.Id,
                        out var sentenceTranslation))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(sentence.Text)
                    || string.IsNullOrWhiteSpace(sentenceTranslation.TranslatedText))
                {
                    continue;
                }

                if (!userLearningItemsByLearningItemId.TryGetValue(
                        learningItem.Id,
                        out var sentenceUserLearningItem))
                {
                    continue;
                }

                candidates.Add(new QuizQuestionCandidate
                {
                    UserLearningItemId = sentenceUserLearningItem.Id,
                    LearningItemId = learningItem.Id,
                    WordId = null,
                    PhraseId = null,
                    SentenceId = sentence.Id,
                    ItemType = learningItem.ItemType,
                    QuestionText = sentence.Text,
                    CorrectMeaningId = Guid.Empty,
                    CorrectAnswerText = sentenceTranslation.TranslatedText,
                    PartOfSpeech = null,
                    IsDifficult = difficultUserLearningItemIds.Contains(sentenceUserLearningItem.Id)
                });

                continue;
            }

            if (!meaningsByLearningItemId.TryGetValue(
                    learningItem.Id,
                    out var itemMeanings))
            {
                continue;
            }

            var correctMeaning = ResolveCorrectMeaning(
                selectedMeaningId: userLearningItem.SelectedMeaningId,
                meanings: itemMeanings);

            if (correctMeaning is null)
            {
                continue;
            }

            var contentText = ResolveCandidateContentText(
                learningItem,
                wordLookup,
                phraseLookup,
                sentenceLookup);

            if (string.IsNullOrWhiteSpace(contentText))
            {
                continue;
            }

            wordLookup.TryGetValue(learningItem.Id, out var word);
            phraseLookup.TryGetValue(learningItem.Id, out var phrase);

            candidates.Add(new QuizQuestionCandidate
            {
                UserLearningItemId = userLearningItem.Id,
                LearningItemId = learningItem.Id,
                WordId = word?.Id,
                PhraseId = phrase?.Id,
                SentenceId = null,
                ItemType = learningItem.ItemType,
                QuestionText = contentText,
                CorrectMeaningId = correctMeaning.Id,
                CorrectAnswerText = correctMeaning.MeaningText,
                PartOfSpeech = correctMeaning.PartOfSpeech,
                IsDifficult = difficultUserLearningItemIds.Contains(userLearningItem.Id)
            });
        }

        return candidates;
    }


    /// <summary>
    /// Quiz içine en fazla kaç sistem önerisi candidate ekleneceğini hesaplar.
    /// 
    /// Ana kural:
    /// - Normal durumda soru sayısının yaklaşık %30'u kadar öneri eklenir.
    /// - En az 1 öneri eklenmeye çalışılır.
    /// - Test quiz gibi minimum candidate ihtiyacı varsa, eksik aday sayısını tamamlamak için quota artırılabilir.
    /// 
    /// Bu neden gerekli?
    /// - Sistem önerileri tüm quizi ele geçirmesin.
    /// - Ama kullanıcının item sayısı test quiz için yetmiyorsa öneriler quiz başlatmayı mümkün kılsın.
    /// </summary>
    private static int ResolveRecommendationQuota(
        int requestedQuestionCount,
        int baseCandidateCount,
        int minimumCandidateCount)
    {
        if (requestedQuestionCount <= 0)
        {
            return 0;
        }

        var ratioBasedQuota = Math.Max(
            1,
            (int)Math.Floor(requestedQuestionCount * 0.30));

        var neededToReachMinimumCandidateCount = Math.Max(
            0,
            minimumCandidateCount - baseCandidateCount);

        var quota = Math.Max(
            ratioBasedQuota,
            neededToReachMinimumCandidateCount);

        return Math.Min(
            quota,
            requestedQuestionCount);
    }


    /// <summary>
    /// Sistem önerilerinde önerilmemesi gereken LearningItem id listesini üretir.
    /// 
    /// Faz 23 kuralı:
    /// Kullanıcının dictionary'sinde zaten olan itemlar sistem önerisi olarak gelmemelidir.
    /// 
    /// Deck quizde bile tüm aktif dictionary itemlarını dışlıyoruz.
    /// Çünkü item deckte olmasa bile kullanıcının dictionary'sindeyse "sistem önerisi" sayılmaz.
    /// </summary>
    private async Task<IReadOnlyCollection<Guid>> ResolveExcludedLearningItemIdsForRecommendationsAsync(
        string keycloakUserId,
        IReadOnlyCollection<UserLearningItem> currentSourceItems,
        CancellationToken cancellationToken)
    {
        var allUserDictionaryItems = await _userLearningItemRepository
            .GetActiveItemsByUserAsync(keycloakUserId, cancellationToken);

        return allUserDictionaryItems
            .Concat(currentSourceItems)
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();
    }


    /// <summary>
    /// QuizType + QuizContentMode değerlerine göre hangi LearningItem tiplerinin quizde kullanılabileceğini çözer.
    /// 
    /// Test quiz:
    /// - Word/Phrase desteklenir.
    /// - Sentence desteklenmez.
    /// 
    /// Writing quiz:
    /// - Word/Phrase/Sentence desteklenir.
    /// - Mixed seçilirse kullanıcının kaydettiği tüm uygun Word, Phrase ve Sentence itemları kullanılabilir.
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

                QuizContentMode.SentencesOnly => throw new BusinessRuleException(
                    "SentencesOnly content mode is supported only for Writing quiz type.",
                    "SENTENCES_ONLY_REQUIRES_WRITING_QUIZ"),

                _ => throw new BusinessRuleException(
                    "Quiz content mode is not supported.",
                    "QUIZ_CONTENT_MODE_NOT_SUPPORTED")
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

                _ => throw new BusinessRuleException(
                    "Quiz content mode is not supported.",
                    "QUIZ_CONTENT_MODE_NOT_SUPPORTED")
            };
        }

        throw new BusinessRuleException(
            "Quiz type is not supported for resolving quiz content mode.",
            "QUIZ_TYPE_NOT_SUPPORTED");
    }

    /// <summary>
    /// LearningItem tipine göre quizde gösterilecek ana içerik metnini çözer.
    /// </summary>
    private static string ResolveCandidateContentText(
        LearningItem learningItem,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Phrase> phraseLookup,
        IReadOnlyDictionary<Guid, Sentence> sentenceLookup)
    {
        return learningItem.ItemType switch
        {
            LearningItemType.Word when wordLookup.TryGetValue(learningItem.Id, out var word)
                => word.Text,

            LearningItemType.Phrase when phraseLookup.TryGetValue(learningItem.Id, out var phrase)
                => phrase.Text,

            LearningItemType.Sentence when sentenceLookup.TryGetValue(learningItem.Id, out var sentence)
                => sentence.Text,

            _ => string.Empty
        };
    }

    /// <summary>
    /// API'den gelen quiz type string değerini domain enum değerine çevirir.
    /// </summary>
    private static QuizType ParseQuizType(string quizType)
    {
        if (Enum.TryParse<QuizType>(
                quizType?.Trim(),
                ignoreCase: true,
                out var parsedQuizType))
        {
            return parsedQuizType;
        }

        throw new BusinessRuleException(
            $"Quiz type '{quizType}' is not supported.",
            "QUIZ_TYPE_NOT_SUPPORTED");
    }

    /// <summary>
    /// API'den gelen quiz source type string değerini domain enum değerine çevirir.
    /// 
    /// Domain enum değeri UserDictionary'dir.
    /// Ancak eski Swagger/request örneklerinde Dictionary kullanıldığı için
    /// Dictionary alias değerini de UserDictionary olarak kabul ediyoruz.
    /// </summary>
    private static QuizSourceType ParseQuizSourceType(string quizSourceType)
    {
        var normalizedQuizSourceType = quizSourceType?.Trim();

        if (string.Equals(
                normalizedQuizSourceType,
                "Dictionary",
                StringComparison.OrdinalIgnoreCase))
        {
            return QuizSourceType.UserDictionary;
        }

        if (Enum.TryParse<QuizSourceType>(
                normalizedQuizSourceType,
                ignoreCase: true,
                out var parsedQuizSourceType))
        {
            return parsedQuizSourceType;
        }

        throw new BusinessRuleException(
            $"Quiz source type '{quizSourceType}' is not supported.",
            "QUIZ_SOURCE_TYPE_NOT_SUPPORTED");
    }

    /// <summary>
    /// API'den gelen quiz content mode string değerini domain enum değerine çevirir.
    /// </summary>
    private static QuizContentMode ParseQuizContentMode(string quizContentMode)
    {
        if (Enum.TryParse<QuizContentMode>(
                quizContentMode?.Trim(),
                ignoreCase: true,
                out var parsedQuizContentMode))
        {
            return parsedQuizContentMode;
        }

        throw new BusinessRuleException(
            $"Quiz content mode '{quizContentMode}' is not supported.",
            "QUIZ_CONTENT_MODE_NOT_SUPPORTED");
    }


    /// <summary>
    /// Quiz için doğru anlamı belirler.
    /// 
    /// Öncelik sırası:
    /// 1. UserLearningItem.SelectedMeaningId
    /// 2. IsPrimary olan meaning
    /// 3. DisplayOrder'a göre ilk meaning
    /// 4. null
    /// </summary>
    private static Meaning? ResolveCorrectMeaning(
        Guid? selectedMeaningId,
        IReadOnlyCollection<Meaning> meanings)
    {
        if (meanings.Count == 0)
        {
            return null;
        }

        if (selectedMeaningId is not null)
        {
            var selectedMeaning = meanings.FirstOrDefault(
                meaning => meaning.Id == selectedMeaningId.Value);

            if (selectedMeaning is not null)
            {
                return selectedMeaning;
            }
        }

        var primaryMeaning = meanings.FirstOrDefault(meaning => meaning.IsPrimary);

        if (primaryMeaning is not null)
        {
            return primaryMeaning;
        }

        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .FirstOrDefault();
    }
}
