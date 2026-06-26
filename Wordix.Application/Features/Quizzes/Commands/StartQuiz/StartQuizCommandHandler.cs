using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Application.Features.Quizzes.Responses;
using Wordix.Application.Features.Quizzes.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// StartQuizCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın UserProfile kaydını alır.
/// - Kullanıcının dictionary itemlarını okur.
/// - İlk prototipte sadece Word itemlardan soru üretir.
/// - Dictionary itemlarını QuizQuestionCandidate modeline dönüştürür.
/// - IQuizQuestionGenerator ile soru/seçenek planı üretir.
/// - QuizSession oluşturur.
/// - QuizQuestion kayıtları oluşturur.
/// - QuizOption kayıtları oluşturur.
/// - Oluşturulan quiz bilgisini response olarak döner.
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

    private readonly IUserProfileSyncService _userProfileSyncService;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Meaning> _meaningRepository;
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
    /// Soru üretme algoritması da direkt burada yazılmaz; IQuizQuestionGenerator kullanılır.
    /// </summary>
    public StartQuizCommandHandler(
        IUserProfileSyncService userProfileSyncService,
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<QuizSession> quizSessionRepository,
        IRepository<QuizQuestion> quizQuestionRepository,
        IRepository<QuizOption> quizOptionRepository,
        IQuizQuestionGenerator quizQuestionGenerator,
        IUnitOfWork unitOfWork)
    {
        _userProfileSyncService = userProfileSyncService;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _meaningRepository = meaningRepository;
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
        // 1. Current user'ın Wordix UserProfile kaydını alıyoruz.
        // Kullanıcı bilgisi request body'den değil, JWT token üzerinden gelir.
        var userProfile = await _userProfileSyncService
            .GetOrCreateCurrentUserProfileAsync(cancellationToken);

        // 2. Kullanıcının aktif dictionary itemlarını alıyoruz.
        // QuizSourceType = Dictionary olduğu için global kelimelerden değil,
        // kullanıcının kendi kaydettiği itemlardan soru üreteceğiz.
        var userLearningItems = await _userLearningItemRepository
            .GetActiveItemsByUserAsync(userProfile.Id, cancellationToken);

        if (userLearningItems.Count == 0)
        {
            throw new BusinessRuleException(
                "You need to save learning items to your dictionary before starting a quiz.",
                "DICTIONARY_IS_EMPTY");
        }

        // 3. Dictionary itemlarından quiz üretmeye uygun candidate listesi hazırlıyoruz.
        var candidates = await BuildQuestionCandidatesAsync(
            userLearningItems,
            cancellationToken);

        // 4 seçenekli test için minimum 4 farklı aday anlam gerekir.
        if (candidates.Count < OptionCountPerQuestion)
        {
            throw new BusinessRuleException(
                $"At least {OptionCountPerQuestion} saved word items with meanings are required to start a multiple choice quiz.",
                "NOT_ENOUGH_DICTIONARY_ITEMS_FOR_QUIZ");
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
        // Önemli:
        // Mevcut QuizSession constructor imzası şu şekilde:
        // QuizSession(
        //     Guid userProfileId,
        //     QuizType quizType,
        //     QuizSourceType quizSourceType,
        //     QuizContentMode quizContentMode,
        //     DifficultyGroup difficultyGroup,
        //     int questionCount,
        //     bool includeSystemRecommendations,
        //     Guid? deckId)
        //
        // İlk prototipte:
        // - Test quiz
        // - Kullanıcının dictionary'si
        // - WordsOnly
        // - Beginner default difficulty
        // - Sistem önerisi yok
        // - Deck yok
        var quizSession = new QuizSession(
            userProfile.Id,
            QuizType.Test,
            QuizSourceType.UserDictionary,
            QuizContentMode.WordsOnly,
            DifficultyGroup.Beginner,
            generationResult.GeneratedQuestionCount,
            includeSystemRecommendations: false,
            deckId: null);

        await _quizSessionRepository.AddAsync(
            quizSession,
            cancellationToken);

        // 7. Generated question/option modellerinden domain entity'leri oluşturuyoruz.
        var createdQuestionResponses = new List<QuizQuestionResponse>();

        foreach (var generatedQuestion in generationResult.Questions
            .OrderBy(question => question.QuestionOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Generator doğru cevabı seçenekler içinde IsCorrect = true olarak işaretledi.
            // QuizQuestion entity ise CorrectAnswer string alanı istiyor.
            var correctAnswerText = ResolveCorrectAnswerText(generatedQuestion);

            // GeneratedQuestion.QuestionType string olarak geliyor.
            // Domain entity ise QuestionType enum bekliyor.
            var questionType = ResolveQuestionType(generatedQuestion.QuestionType);

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

                // API response'a IsCorrect koymuyoruz.
                // Doğru cevap bilgisi sadece backend/database tarafında kalır.
                createdOptionResponses.Add(new QuizOptionResponse
                {
                    QuizOptionId = quizOption.Id,
                    DisplayOrder = quizOption.DisplayOrder,
                    OptionText = quizOption.OptionText
                });
            }

            createdQuestionResponses.Add(new QuizQuestionResponse
            {
                QuizQuestionId = quizQuestion.Id,
                QuestionOrder = quizQuestion.DisplayOrder,
                QuestionText = quizQuestion.QuestionText,
                LearningItemId = quizQuestion.LearningItemId,
                WordId = generatedQuestion.WordId,
                ItemType = LearningItemType.Word.ToString(),

                // API response'ta generator'ın daha açıklayıcı question type değerini döndürüyoruz.
                // Database tarafında ise QuizQuestion.QuestionType domain enum olarak saklanıyor.
                QuestionType = generatedQuestion.QuestionType,

                Options = createdOptionResponses
                .OrderBy(option => option.DisplayOrder)
                .ToArray()
            });
        }

        // 8. QuizSession + QuizQuestion + QuizOption kayıtlarını tek SaveChanges ile kaydediyoruz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Response dönüyoruz.
        return new StartQuizResponse
        {
            QuizSessionId = quizSession.Id,
            QuizType = request.QuizType.Trim(),
            QuizSourceType = request.QuizSourceType.Trim(),
            QuizContentMode = request.QuizContentMode.Trim(),
            QuestionCount = createdQuestionResponses.Count,
            StartedAt = quizSession.StartedAt,
            Status = quizSession.Status.ToString(),
            Questions = createdQuestionResponses
        };
    }

    /// <summary>
    /// Kullanıcının dictionary kayıtlarından quiz question candidate listesi oluşturur.
    /// 
    /// İlk prototipte sadece Word itemlar desteklenir.
    /// Phrase/Sentence itemlar dictionary'de olsa bile bu quiz modunda kullanılmaz.
    /// </summary>
    private async Task<IReadOnlyCollection<QuizQuestionCandidate>> BuildQuestionCandidatesAsync(
        IReadOnlyCollection<UserLearningItem> userLearningItems,
        CancellationToken cancellationToken)
    {
        var learningItemIds = userLearningItems
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();

        var learningItems = await _learningItemRepository.ListAsync(
            item => learningItemIds.Contains(item.Id)
                    && item.IsActive
                    && item.ItemType == LearningItemType.Word,
            cancellationToken);

        if (learningItems.Count == 0)
        {
            return Array.Empty<QuizQuestionCandidate>();
        }

        var wordLearningItemIds = learningItems
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        var userLearningItemsByLearningItemId = userLearningItems
            .Where(item => wordLearningItemIds.Contains(item.LearningItemId))
            .GroupBy(item => item.LearningItemId)
            .ToDictionary(group => group.Key, group => group.First());

        var words = await _wordRepository.ListAsync(
            word => wordLearningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        var meanings = await _meaningRepository.ListAsync(
            meaning => wordLearningItemIds.Contains(meaning.LearningItemId),
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

            if (!wordLookup.TryGetValue(learningItem.Id, out var word))
            {
                continue;
            }

            if (!meaningsByLearningItemId.TryGetValue(learningItem.Id, out var itemMeanings))
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

            candidates.Add(new QuizQuestionCandidate
            {
                UserLearningItemId = userLearningItem.Id,
                LearningItemId = learningItem.Id,
                WordId = word.Id,
                QuestionText = word.Text,
                CorrectMeaningId = correctMeaning.Id,
                CorrectAnswerText = correctMeaning.MeaningText,
                PartOfSpeech = correctMeaning.PartOfSpeech
            });
        }

        return candidates;
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

    /// <summary>
    /// GeneratedQuizQuestion içindeki doğru seçeneğin metnini bulur.
    /// 
    /// QuizQuestion entity, doğru cevabı string olarak saklıyor.
    /// Bu yüzden doğru option'ın OptionText değerini CorrectAnswer alanına yazıyoruz.
    /// </summary>
    private static string ResolveCorrectAnswerText(GeneratedQuizQuestion generatedQuestion)
    {
        var correctOption = generatedQuestion.Options
            .FirstOrDefault(option => option.IsCorrect);

        if (correctOption is null)
        {
            throw new BusinessRuleException(
                "Generated quiz question does not contain a correct option.",
                "GENERATED_QUESTION_HAS_NO_CORRECT_OPTION");
        }

        return correctOption.OptionText;
    }

    /// <summary>
    /// Generator'dan gelen question type değerini domain QuestionType enum'una çevirir.
    /// 
    /// Neden bu mapping gerekli?
    /// - Generator daha açıklayıcı bir application-level değer üretebilir:
    ///   "MultipleChoiceTranslation"
    /// 
    /// - Domain enum ise daha genel bir değer tutuyor olabilir:
    ///   "MultipleChoice", "Test", "Translation" vb.
    /// 
    /// Bu yüzden generator string'i ile domain enum adının birebir aynı olmasını zorunlu kılmıyoruz.
    /// Handler boundary'sinde güvenli mapping yapıyoruz.
    /// </summary>
    private static QuestionType ResolveQuestionType(string questionType)
    {
        if (string.IsNullOrWhiteSpace(questionType))
        {
            throw new BusinessRuleException(
                "Generated question type is required.",
                "QUESTION_TYPE_REQUIRED");
        }

        // 1. Önce birebir enum parse deniyoruz.
        // Eğer domain enum içinde "MultipleChoiceTranslation" varsa direkt çalışır.
        if (Enum.TryParse<QuestionType>(
                questionType.Trim(),
                ignoreCase: true,
                out var parsedQuestionType))
        {
            return parsedQuestionType;
        }

        // 2. İlk prototip generator'ımız "MultipleChoiceTranslation" üretiyor.
        // Fakat mevcut Domain enum bu ismi taşımıyor.
        // Bu yüzden onu domain tarafındaki en yakın çoktan seçmeli soru tipine map ediyoruz.
        if (string.Equals(
                questionType.Trim(),
                "MultipleChoiceTranslation",
                StringComparison.OrdinalIgnoreCase))
        {
            var multipleChoiceAliases = new[]
            {
            "MultipleChoice",
            "Test",
            "Translation",
            "WordTranslation",
            "WordToMeaning",
            "MeaningSelection",
            "MultipleChoiceMeaning",
            "MultipleChoiceWordTranslation"
        };

            foreach (var alias in multipleChoiceAliases)
            {
                if (Enum.TryParse<QuestionType>(
                        alias,
                        ignoreCase: true,
                        out var aliasQuestionType))
                {
                    return aliasQuestionType;
                }
            }
        }

        // 3. Hâlâ bulunamadıysa bu sefer hatada mevcut enum değerlerini de gösteriyoruz.
        // Böylece tekrar runtime hatası alırsak enum dosyasını açmadan bile hangi değerlerin olduğunu görürüz.
        var supportedQuestionTypes = string.Join(
            ", ",
            Enum.GetNames<QuestionType>());

        throw new BusinessRuleException(
            $"Question type '{questionType}' is not supported. Supported domain question types: {supportedQuestionTypes}.",
            "QUESTION_TYPE_NOT_SUPPORTED");
    }
}