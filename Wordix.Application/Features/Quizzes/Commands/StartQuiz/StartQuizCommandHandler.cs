using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
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

    private readonly ICurrentUserService _currentUserService;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<UserLearningItem> _genericUserLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;
    private readonly IRepository<QuizSession> _quizSessionRepository;
    private readonly IRepository<QuizQuestion> _quizQuestionRepository;
    private readonly IRepository<QuizOption> _quizOptionRepository;
    private readonly IQuizQuestionGenerator _quizQuestionGenerator;
    private readonly IUnitOfWork _unitOfWork;

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
        ICurrentUserService currentUserService,
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<UserLearningItem> genericUserLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository,
        IRepository<QuizSession> quizSessionRepository,
        IRepository<QuizQuestion> quizQuestionRepository,
        IRepository<QuizOption> quizOptionRepository,
        IQuizQuestionGenerator quizQuestionGenerator,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userLearningItemRepository = userLearningItemRepository;
        _genericUserLearningItemRepository = genericUserLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _meaningRepository = meaningRepository;
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
        _quizSessionRepository = quizSessionRepository;
        _quizQuestionRepository = quizQuestionRepository;
        _quizOptionRepository = quizOptionRepository;
        _quizQuestionGenerator = quizQuestionGenerator;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// StartQuizCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<StartQuizResponse> Handle(
        StartQuizCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        // Kullanıcıya ait dictionary ve quiz kayıtları bu KeycloakUserId ile ilişkilendirilir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // Request string değerlerini domain enum değerlerine çeviriyoruz.
        // Controller veya handler içine dağınık string karşılaştırması koymamak için
        // parse işlemini küçük helper methodlarda topluyoruz.
        var quizType = ParseQuizType(request.QuizType);
        var quizSourceType = ParseQuizSourceType(request.QuizSourceType);
        var quizContentMode = ParseQuizContentMode(request.QuizContentMode);

        // 2. Quiz kaynağına göre kullanılacak UserLearningItem listesini çözüyoruz.
        //
        // UserDictionary:
        // - Kullanıcının tüm aktif dictionary itemları kullanılır.
        //
        // Deck:
        // - Sadece ilgili deck içindeki UserLearningItem kayıtları kullanılır.
        // - Deck ownership KeycloakUserId ile kontrol edilir.
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

        // 3. Dictionary itemlarından quiz üretmeye uygun candidate listesi hazırlıyoruz.
        var candidates = await BuildQuestionCandidatesAsync(
            userLearningItems,
            quizContentMode,
            cancellationToken);

        // 4 seçenekli test için minimum 4 farklı aday anlam gerekir.
        if (candidates.Count < OptionCountPerQuestion)
        {
            throw new BusinessRuleException(
                $"At least {OptionCountPerQuestion} saved learning items with meanings are required to start a multiple choice quiz.");
        }

        // Kullanıcı dictionary'sinde 5 uygun kelime varsa ve 10 soru isterse,
        // ilk prototipte üretilebilir maksimum kadar soru oluşturuyoruz.
        var actualQuestionCount = Math.Min(
            request.QuestionCount,
            candidates.Count);

        // 4. Generator request modelini hazırlıyoruz.
        var generationRequest = new QuizQuestionGenerationRequest
        {
            RequestedQuestionCount = actualQuestionCount,
            OptionCountPerQuestion = OptionCountPerQuestion,
            Candidates = candidates
        };

        // 5. Soru/seçenek planını generator'a ürettiriyoruz.
        var generationResult = await _quizQuestionGenerator.GenerateAsync(
            generationRequest,
            cancellationToken);

        if (!generationResult.HasQuestions)
        {
            throw new BusinessRuleException(
                "Quiz questions could not be generated from your dictionary items.",
                "QUIZ_QUESTIONS_COULD_NOT_BE_GENERATED");
        }

        // 6. QuizSession oluşturuyoruz.
        //
        // Yeni QuizSession constructor imzası artık UserProfileId değil KeycloakUserId alır.
        //
        // İlk prototipte:
        // - Test quiz
        // - Kullanıcının dictionary'si
        // - WordsOnly
        // - Beginner default difficulty
        // - Sistem önerisi yok
        // - Deck yok
        var quizSession = new QuizSession(
             keycloakUserId,
             quizType,
             quizSourceType,
             quizContentMode,
             DifficultyGroup.Beginner,
             generationResult.GeneratedQuestionCount,
             includeSystemRecommendations: false,
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

            // Mevcut QuizQuestion entity alanları:
            // QuizSessionId, LearningItemId, QuestionType, QuestionText,
            // CorrectAnswer, DisplayOrder, IsSystemRecommended.
            //
            // Bu entity'de UserLearningItemId, WordId, CorrectMeaningId yok.
            // O yüzden constructor'a sadece mevcut domain modelindeki bilgileri gönderiyoruz.
            var quizQuestion = new QuizQuestion(
                quizSession.Id,
                generatedQuestion.LearningItemId,
                questionType,
                generatedQuestion.QuestionText,
                correctAnswerText,
                generatedQuestion.QuestionOrder,
                isSystemRecommended: false);

            await _quizQuestionRepository.AddAsync(
                quizQuestion,
                cancellationToken);

            var createdOptionResponses = new List<QuizOptionResponse>();

            foreach (var generatedOption in generatedQuestion.Options
                .OrderBy(option => option.DisplayOrder))
            {
                // Mevcut QuizOption entity alanları:
                // QuizQuestionId, OptionText, IsCorrect, DisplayOrder.
                //
                // Bu entity'de MeaningId yok.
                // MeaningId generator ara modelinde kalabilir ama QuizOption entity'ye yazılmaz.
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
                    optionResponses: createdOptionResponses));
        }

        // 8. QuizSession + QuizQuestion + QuizOption kayıtlarını tek SaveChanges ile kaydediyoruz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizMapper.ToStartQuizResponse(
            request: request,
            quizSession: quizSession,
            questionResponses: createdQuestionResponses);
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
        QuizContentMode quizContentMode,
        CancellationToken cancellationToken)
    {
        var learningItemIds = userLearningItems
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();

        var allowedItemTypes = ResolveAllowedLearningItemTypes(quizContentMode);

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

        var words = await _wordRepository.ListAsync(
            word => filteredLearningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        var phrases = await _phraseRepository.ListAsync(
            phrase => filteredLearningItemIds.Contains(phrase.LearningItemId),
            cancellationToken);

        var phraseLookup = phrases.ToDictionary(phrase => phrase.LearningItemId);

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
                phraseLookup);

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
                ItemType = learningItem.ItemType,
                QuestionText = contentText,
                CorrectMeaningId = correctMeaning.Id,
                CorrectAnswerText = correctMeaning.MeaningText,
                PartOfSpeech = correctMeaning.PartOfSpeech
            });
        }

        return candidates;
    }


    /// <summary>
    /// QuizContentMode değerine göre hangi LearningItem tiplerinin quizde kullanılabileceğini çözer.
    /// </summary>
    private static IReadOnlyCollection<LearningItemType> ResolveAllowedLearningItemTypes(
        QuizContentMode quizContentMode)
    {
        return quizContentMode switch
        {
            QuizContentMode.WordsOnly => new[] { LearningItemType.Word },
            QuizContentMode.PhrasesOnly => new[] { LearningItemType.Phrase },
            QuizContentMode.Mixed => new[] { LearningItemType.Word, LearningItemType.Phrase },

            // Sentence quiz desteği Faz 19'a bırakıldı.
            QuizContentMode.SentencesOnly => throw new BusinessRuleException(
                "Sentence quiz mode is not supported in the current multiple choice quiz flow. It will be handled in the Writing Quiz phase.",
                "QUIZ_CONTENT_MODE_NOT_SUPPORTED"),

            _ => throw new BusinessRuleException(
                "Quiz content mode is not supported.",
                "QUIZ_CONTENT_MODE_NOT_SUPPORTED")
        };
    }

    /// <summary>
    /// LearningItem tipine göre quizde gösterilecek ana içerik metnini çözer.
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
