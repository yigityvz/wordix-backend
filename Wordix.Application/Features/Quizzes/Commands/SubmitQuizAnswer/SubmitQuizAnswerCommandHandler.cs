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

namespace Wordix.Application.Features.Quizzes.Commands.SubmitQuizAnswer;

/// <summary>
/// SubmitQuizAnswerCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Current user'ın quiz session'a cevap verme yetkisini kontrol eder.
/// - Seçilen quiz option'ın ilgili session içindeki bir question'a ait olup olmadığını doğrular.
/// - Cevabı değerlendirir.
/// - QuizAnswer kaydı oluşturur.
/// - UserLearningProgress değerlerini günceller.
/// - LearningProgressHistory kaydı oluşturur.
/// - Response döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Quiz session, quiz answer ve dictionary ownership kontrolleri KeycloakUserId ile yapılır.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir use-case akışıdır.
/// - Controller, DbContext veya HTTP detaylarını bilmez.
/// - Repository abstraction'ları ve application servisleri üzerinden çalışır.
/// </summary>
public sealed class SubmitQuizAnswerCommandHandler
    : IRequestHandler<SubmitQuizAnswerCommand, SubmitQuizAnswerResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IQuizRepository _quizRepository;
    private readonly IRepository<QuizQuestion> _quizQuestionRepository;
    private readonly IRepository<QuizOption> _quizOptionRepository;
    private readonly IRepository<QuizAnswer> _quizAnswerRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<QuizRecommendationItem> _quizRecommendationItemRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;
    private readonly IRepository<LearningProgressHistory> _learningProgressHistoryRepository;
    private readonly IQuizAnswerEvaluator _quizAnswerEvaluator;
    private readonly ILearningScoreCalculator _learningScoreCalculator;
    private readonly IReviewScheduleCalculator _reviewScheduleCalculator;
    private readonly ILearningProgressUpdater _learningProgressUpdater;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler'ın ihtiyaç duyduğu dependency'ler DI üzerinden alınır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller logic'i yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden gelir.
    /// Quiz ownership ve duplicate answer sorguları IQuizRepository üzerinden yapılır.
    /// </summary>
    public SubmitQuizAnswerCommandHandler(
        ICurrentUserService currentUserService,
        IQuizRepository quizRepository,
        IRepository<QuizQuestion> quizQuestionRepository,
        IRepository<QuizOption> quizOptionRepository,
        IRepository<QuizAnswer> quizAnswerRepository,
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<QuizRecommendationItem> quizRecommendationItemRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository,
        IRepository<LearningProgressHistory> learningProgressHistoryRepository,
        IQuizAnswerEvaluator quizAnswerEvaluator,
        ILearningScoreCalculator learningScoreCalculator,
        IReviewScheduleCalculator reviewScheduleCalculator,
        ILearningProgressUpdater learningProgressUpdater,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _quizRepository = quizRepository;
        _quizQuestionRepository = quizQuestionRepository;
        _quizOptionRepository = quizOptionRepository;
        _quizAnswerRepository = quizAnswerRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _quizRecommendationItemRepository = quizRecommendationItemRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
        _learningProgressHistoryRepository = learningProgressHistoryRepository;
        _quizAnswerEvaluator = quizAnswerEvaluator;
        _learningScoreCalculator = learningScoreCalculator;
        _reviewScheduleCalculator = reviewScheduleCalculator;
        _learningProgressUpdater = learningProgressUpdater;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Kullanıcının quiz cevabını işler.
    /// </summary>
    public async Task<SubmitQuizAnswerResponse> Handle(
        SubmitQuizAnswerCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // UserProfileId client'tan alınmaz.
        // Backend burada UserProfile oluşturmaz.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. QuizSession var mı ve current user'a ait mi kontrol ediyoruz.
        //
        // Eski yapı:
        // QuizSession.UserProfileId == userProfile.Id
        //
        // Yeni yapı:
        // QuizSession.KeycloakUserId == keycloakUserId
        var quizSession = await _quizRepository.GetSessionByIdForUserAsync(
            request.QuizSessionId,
            keycloakUserId,
            cancellationToken);

        if (quizSession is null)
        {
            throw new NotFoundException(
                "Quiz session",
                request.QuizSessionId);
        }

        // 3. Quiz session cevap kabul edebilir durumda mı?
        EnsureQuizSessionCanAcceptAnswer(quizSession);

        // 4. Quiz tipine göre cevap hedefini çözüyoruz.
        //
        // Test quiz:
        // - selectedQuizOptionId üzerinden QuizOption bulunur.
        // - QuizQuestion option üzerinden çözülür.
        //
        // Writing quiz:
        // - quizQuestionId üzerinden QuizQuestion bulunur.
        // - Option olmadığı için selectedOption null kalır.
        var selectedOption = await ResolveSelectedOptionForAnswerAsync(
            quizSession,
            request,
            cancellationToken);

        var quizQuestion = await ResolveQuizQuestionForAnswerAsync(
            quizSession,
            request,
            selectedOption,
            cancellationToken);

        // 5. Question route'taki quiz session'a mı ait?
        // Bu kontrol, başka session'daki question/option id'nin bu session'a gönderilmesini engeller.
        if (quizQuestion.QuizSessionId != quizSession.Id)
        {
            throw new BusinessRuleException(
                "Answered question does not belong to the specified quiz session.",
                "ANSWERED_QUESTION_DOES_NOT_BELONG_TO_QUIZ_SESSION");
        }

        // 6. Aynı soru daha önce cevaplanmış mı?
        // İlk prototipte her QuizQuestion sadece bir kez cevaplanabilir.
        var alreadyAnswered = await _quizRepository.HasAnswerForQuestionAsync(
            quizQuestion.Id,
            keycloakUserId,
            cancellationToken);

        if (alreadyAnswered)
        {
            throw new BusinessRuleException(
                "This quiz question has already been answered.",
                "QUIZ_QUESTION_ALREADY_ANSWERED");
        }

        // 7. Cevap değerlendirme request'i hazırlanır.
        var evaluationRequest = new QuizAnswerEvaluationRequest
        {
            QuizSessionId = quizSession.Id,
            QuizQuestionId = quizQuestion.Id,
            QuizType = quizSession.QuizType,
            QuestionType = quizQuestion.QuestionType,

            SelectedQuizOptionId = selectedOption?.Id,
            SelectedOptionText = selectedOption?.OptionText,
            SelectedOptionIsCorrect = selectedOption?.IsCorrect,

            UserAnswerText = request.UserAnswer,
            CorrectAnswerText = quizQuestion.CorrectAnswer,
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds
        };

        // 8. Cevap değerlendirmesi ayrı servisle yapılır.
        var evaluationResult = _quizAnswerEvaluator.Evaluate(evaluationRequest);

        // 9. QuizAnswer entity oluşturulur.
        //
        // Test quiz:
        // - SelectedQuizOptionId doludur.
        // - UserAnswer olarak seçilen option text saklanır.
        //
        // Writing quiz:
        // - SelectedQuizOptionId null olur.
        // - UserAnswer olarak kullanıcının yazdığı cevap saklanır.
        var quizAnswer = new QuizAnswer(
            quizQuestion.Id,
            keycloakUserId,
            evaluationResult.CorrectAnswerText,
            evaluationResult.AnswerResult,
            evaluationResult.QuestionResponseTimeInMilliseconds ?? 0,
            evaluationResult.SelectedQuizOptionId,
            evaluationResult.SubmittedAnswerText);

        await _quizAnswerRepository.AddAsync(
            quizAnswer,
            cancellationToken);

        // 11. Eğer cevaplanan soru sistem önerisiyse,
        // ilgili QuizRecommendationItem kaydını bulup cevap sonucunu güncelliyoruz.
        //
        // Normal dictionary/deck sorularında quizRecommendationItem null kalır.
        QuizRecommendationItem? quizRecommendationItem = null;

        if (quizQuestion.IsSystemRecommended)
        {
            quizRecommendationItem = await _quizRecommendationItemRepository.FirstOrDefaultAsync(
                recommendationItem => recommendationItem.QuizQuestionId == quizQuestion.Id,
                cancellationToken);

            if (quizRecommendationItem is null)
            {
                throw new BusinessRuleException(
                    "System recommended quiz question does not have a recommendation tracking record.",
                    "QUIZ_RECOMMENDATION_ITEM_NOT_FOUND");
            }

            quizRecommendationItem.RegisterAnswerResult(evaluationResult.IsCorrect);
        }

        // 12. Bu question hangi LearningItem'dan üretildiyse,
        // current user'ın UserLearningItem kaydını buluyoruz.
        //
        // Normal soru:
        // - UserLearningItem kesinlikle olmalı.
        // - Yoksa hata.
        //
        // System recommendation soru:
        // - UserLearningItem olmayabilir.
        // - Çünkü önerilen item henüz kullanıcının dictionary'sine eklenmemiş olabilir.
        // - Bu durumda progress update yapmayız.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item => item.KeycloakUserId == keycloakUserId
                    && item.LearningItemId == quizQuestion.LearningItemId
                    && item.IsActive,
            cancellationToken);

        LearningProgressUpdateResult? progressUpdateResult = null;

        var canAddRecommendedItemToDictionary = false;

        if (userLearningItem is null)
        {
            if (!quizQuestion.IsSystemRecommended)
            {
                throw new BusinessRuleException(
                    "The answered learning item is not saved in the current user's dictionary.",
                    "ANSWERED_ITEM_NOT_FOUND_IN_USER_DICTIONARY");
            }

            // Faz 23 kararı:
            // Sistem önerisi yanlış bilindiyse otomatik dictionary'ye eklemiyoruz.
            // Frontend'e eklenebilir bilgisini dönüyoruz.
            // Kullanıcı isterse ayrı endpoint ile dictionary'ye ekleyecek.
            canAddRecommendedItemToDictionary =
                quizRecommendationItem is not null &&
                !evaluationResult.IsCorrect &&
                !quizRecommendationItem.WasAddedToDictionary;
        }
        else
        {
            // 13. UserLearningProgress kaydını buluyoruz.
            var userLearningProgress = await _userLearningProgressRepository.FirstOrDefaultAsync(
                progress => progress.UserLearningItemId == userLearningItem.Id,
                cancellationToken);

            if (userLearningProgress is null)
            {
                throw new NotFoundException(
                    "User learning progress",
                    userLearningItem.Id);
            }

            var reviewedAt = DateTimeOffset.UtcNow;

            // 14. Confidence score hesaplanır.
            var scoreCalculationResult = _learningScoreCalculator.Calculate(
                new LearningScoreCalculationRequest
                {
                    CurrentConfidenceScore = userLearningProgress.LearningConfidenceScore,
                    IsCorrect = evaluationResult.IsCorrect,
                    QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds
                });

            // 15. Review schedule hesaplanır.
            var reviewScheduleEvent = _reviewScheduleCalculator.Calculate(
                new ReviewScheduleCalculationRequest
                {
                    IsCorrect = evaluationResult.IsCorrect,
                    NewConfidenceScore = scoreCalculationResult.NewConfidenceScore,
                    CurrentRepetitionLevel = userLearningProgress.RepetitionLevel,
                    QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds,
                    ReviewedAt = reviewedAt
                });

            // 16. Progress update sonucu hesaplanır.
            progressUpdateResult = _learningProgressUpdater.CalculateUpdate(
                new LearningProgressUpdateRequest
                {
                    UserLearningProgressId = userLearningProgress.Id,
                    UserLearningItemId = userLearningItem.Id,
                    CurrentLearningStatus = userLearningProgress.LearningStatus,
                    CurrentCorrectCount = userLearningProgress.CorrectCount,
                    CurrentWrongCount = userLearningProgress.WrongCount,
                    CurrentConsecutiveCorrectCount = userLearningProgress.ConsecutiveCorrectCount,
                    CurrentConsecutiveWrongCount = userLearningProgress.ConsecutiveWrongCount,
                    CurrentRepetitionLevel = userLearningProgress.RepetitionLevel,
                    CurrentNextReviewDate = userLearningProgress.NextReviewDate,
                    IsCorrect = evaluationResult.IsCorrect,
                    ScoreCalculationResult = scoreCalculationResult,
                    ReviewScheduleEvent = reviewScheduleEvent,
                    ReviewedAt = reviewedAt
                });

            // 17. Hesaplanan progress state'i domain entity'ye uygulanır.
            ApplyProgressUpdate(
                userLearningProgress,
                progressUpdateResult,
                evaluationResult.IsCorrect);

            // 18. Progress history oluşturulur.
            var progressHistory = new LearningProgressHistory(
                userLearningProgress.Id,
                progressUpdateResult.PreviousLearningStatus,
                progressUpdateResult.NewLearningStatus,
                progressUpdateResult.PreviousConfidenceScore,
                progressUpdateResult.NewConfidenceScore,
                progressUpdateResult.ChangeReason);

            await _learningProgressHistoryRepository.AddAsync(
                progressHistory,
                cancellationToken);
        }

        // 19. Tüm değişiklikler tek transaction/save akışında kaydedilir.
        //
        // Normal soru:
        // - QuizAnswer
        // - UserLearningProgress
        // - LearningProgressHistory
        //
        // System recommendation soru dictionary'de değilse:
        // - QuizAnswer
        // - QuizRecommendationItem.WasAnsweredCorrectly
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizMapper.ToSubmitQuizAnswerResponse(
            quizAnswer: quizAnswer,
            quizSession: quizSession,
            quizQuestion: quizQuestion,
            selectedOption: selectedOption,
            evaluationResult: evaluationResult,
            progressUpdateResult: progressUpdateResult,
            quizRecommendationItem: quizRecommendationItem,
            canAddRecommendedItemToDictionary: canAddRecommendedItemToDictionary);
    }



    /// <summary>
    /// Quiz tipine göre selected option bilgisini çözer.
    /// 
    /// Test quiz:
    /// - SelectedQuizOptionId zorunludur.
    /// 
    /// Writing quiz:
    /// - Option kullanılmaz, null döner.
    /// </summary>
    private async Task<QuizOption?> ResolveSelectedOptionForAnswerAsync(
        QuizSession quizSession,
        SubmitQuizAnswerCommand request,
        CancellationToken cancellationToken)
    {
        if (quizSession.QuizType == QuizType.Writing)
        {
            if (request.SelectedQuizOptionId.HasValue &&
                request.SelectedQuizOptionId.Value != Guid.Empty)
            {
                throw new BusinessRuleException(
                    "Selected quiz option id is not allowed for writing quiz answers.",
                    "SELECTED_OPTION_NOT_ALLOWED_FOR_WRITING_QUIZ");
            }

            return null;
        }

        if (quizSession.QuizType != QuizType.Test)
        {
            throw new BusinessRuleException(
                "Quiz type is not supported for answer submission.",
                "QUIZ_TYPE_NOT_SUPPORTED_FOR_ANSWER_SUBMISSION");
        }

        if (!request.SelectedQuizOptionId.HasValue || request.SelectedQuizOptionId.Value == Guid.Empty)
        {
            throw new BusinessRuleException(
                "Selected quiz option id is required for test quiz answers.",
                "SELECTED_QUIZ_OPTION_ID_REQUIRED_FOR_TEST_QUIZ");
        }

        var selectedOption = await _quizOptionRepository.FirstOrDefaultAsync(
            option => option.Id == request.SelectedQuizOptionId.Value,
            cancellationToken);

        if (selectedOption is null)
        {
            throw new NotFoundException(
                "Quiz option",
                request.SelectedQuizOptionId.Value);
        }

        return selectedOption;
    }

    /// <summary>
    /// Quiz tipine göre cevaplanan question bilgisini çözer.
    /// 
    /// Test quiz:
    /// - Question, selected option üzerinden bulunur.
    /// 
    /// Writing quiz:
    /// - Question, request.QuizQuestionId üzerinden bulunur.
    /// </summary>
    private async Task<QuizQuestion> ResolveQuizQuestionForAnswerAsync(
        QuizSession quizSession,
        SubmitQuizAnswerCommand request,
        QuizOption? selectedOption,
        CancellationToken cancellationToken)
    {
        if (quizSession.QuizType == QuizType.Test)
        {
            if (selectedOption is null)
            {
                throw new BusinessRuleException(
                    "Selected option is required to resolve test quiz question.",
                    "SELECTED_OPTION_REQUIRED_TO_RESOLVE_TEST_QUESTION");
            }

            var quizQuestion = await _quizQuestionRepository.FirstOrDefaultAsync(
                question => question.Id == selectedOption.QuizQuestionId,
                cancellationToken);

            if (quizQuestion is null)
            {
                throw new NotFoundException(
                    "Quiz question",
                    selectedOption.QuizQuestionId);
            }

            return quizQuestion;
        }

        if (quizSession.QuizType == QuizType.Writing)
        {
            if (!request.QuizQuestionId.HasValue || request.QuizQuestionId.Value == Guid.Empty)
            {
                throw new BusinessRuleException(
                    "Quiz question id is required for writing quiz answers.",
                    "QUIZ_QUESTION_ID_REQUIRED_FOR_WRITING_QUIZ");
            }

            if (string.IsNullOrWhiteSpace(request.UserAnswer))
            {
                throw new BusinessRuleException(
                    "User answer is required for writing quiz answers.",
                    "USER_ANSWER_REQUIRED_FOR_WRITING_QUIZ");
            }

            var quizQuestion = await _quizQuestionRepository.FirstOrDefaultAsync(
                question => question.Id == request.QuizQuestionId.Value,
                cancellationToken);

            if (quizQuestion is null)
            {
                throw new NotFoundException(
                    "Quiz question",
                    request.QuizQuestionId.Value);
            }

            return quizQuestion;
        }

        throw new BusinessRuleException(
            "Quiz type is not supported for resolving answered question.",
            "QUIZ_TYPE_NOT_SUPPORTED_FOR_QUESTION_RESOLUTION");
    }


    /// <summary>
    /// Quiz session cevap kabul edebilir durumda mı kontrol eder.
    /// 
    /// Enum değerlerine direkt sıkı bağlanmamak için status string'i üzerinden kontrol yapıyoruz.
    /// Mevcut domain enum değerleri InProgress, Completed, Cancelled şeklindedir.
    /// </summary>
    private static void EnsureQuizSessionCanAcceptAnswer(
        QuizSession quizSession)
    {
        var status = quizSession.Status.ToString();

        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Completed quiz sessions cannot accept new answers.",
                "QUIZ_SESSION_ALREADY_COMPLETED");
        }

        if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException(
                "Cancelled quiz sessions cannot accept new answers.",
                "QUIZ_SESSION_CANCELLED");
        }
    }


    /// <summary>
    /// LearningProgressUpdater tarafından hesaplanan yeni state'i UserLearningProgress entity'sine uygular.
    /// 
    /// Not:
    /// UserLearningProgress entity'si cevap sonucuna göre güncelleme yapmak için
    /// RegisterCorrectAnswer / RegisterWrongAnswer domain methodlarını içeriyor.
    /// 
    /// Mevcut entity method imzası şu mantıktadır:
    /// - yeni confidence score
    /// - yeni repetition level
    /// - yeni learning status
    /// - next review date
    /// 
    /// Bu yüzden Application tarafındaki DateTimeOffset? NextReviewDate değerini
    /// Domain tarafındaki DateTime? tipine çeviriyoruz.
    /// </summary>
    private static void ApplyProgressUpdate(
        UserLearningProgress userLearningProgress,
        LearningProgressUpdateResult progressUpdateResult,
        bool isCorrect)
    {
        var nextReviewDate = ConvertToNullableDateTime(
            progressUpdateResult.NextReviewDate);

        if (isCorrect)
        {
            userLearningProgress.RegisterCorrectAnswer(
                progressUpdateResult.NewConfidenceScore,
                progressUpdateResult.RepetitionLevel,
                progressUpdateResult.NewLearningStatus,
                nextReviewDate);

            return;
        }

        userLearningProgress.RegisterWrongAnswer(
            progressUpdateResult.NewConfidenceScore,
            progressUpdateResult.RepetitionLevel,
            progressUpdateResult.NewLearningStatus,
            nextReviewDate);
    }

    /// <summary>
    /// Application katmanında tarihleri DateTimeOffset ile taşıyoruz.
    /// Domain entity ise mevcut haliyle DateTime? bekliyor.
    /// 
    /// Bu helper, DateTimeOffset? değerini UTC DateTime? değerine çevirir.
    /// </summary>
    private static DateTime? ConvertToNullableDateTime(
        DateTimeOffset? dateTimeOffset)
    {
        return dateTimeOffset?.UtcDateTime;
    }
}
