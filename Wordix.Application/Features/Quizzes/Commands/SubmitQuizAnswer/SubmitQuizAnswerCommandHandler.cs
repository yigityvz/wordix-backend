using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Models;
using Wordix.Application.Features.Quizzes.Responses;
using Wordix.Application.Features.Quizzes.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

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

        // 4. Kullanıcının seçtiği option'ı buluyoruz.
        var selectedOption = await _quizOptionRepository.FirstOrDefaultAsync(
            option => option.Id == request.SelectedQuizOptionId,
            cancellationToken);

        if (selectedOption is null)
        {
            throw new NotFoundException(
                "Quiz option",
                request.SelectedQuizOptionId);
        }

        // 5. Option'ın ait olduğu question'ı buluyoruz.
        var quizQuestion = await _quizQuestionRepository.FirstOrDefaultAsync(
            question => question.Id == selectedOption.QuizQuestionId,
            cancellationToken);

        if (quizQuestion is null)
        {
            throw new NotFoundException(
                "Quiz question",
                selectedOption.QuizQuestionId);
        }

        // 6. Question route'taki quiz session'a mı ait?
        // Bu kontrol, başka session'daki option id'nin bu session'a gönderilmesini engeller.
        if (quizQuestion.QuizSessionId != quizSession.Id)
        {
            throw new BusinessRuleException(
                "Selected option does not belong to the specified quiz session.",
                "SELECTED_OPTION_DOES_NOT_BELONG_TO_QUIZ_SESSION");
        }

        // 7. Aynı soru daha önce cevaplanmış mı?
        // İlk prototipte her QuizQuestion sadece bir kez cevaplanabilir.
        //
        // Yeni mimaride bu kontrol UserProfileId ile değil KeycloakUserId ile yapılır.
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

        // 8. Cevap değerlendirme request'i hazırlanır.
        var evaluationRequest = new QuizAnswerEvaluationRequest
        {
            QuizSessionId = quizSession.Id,
            QuizQuestionId = quizQuestion.Id,
            SelectedQuizOptionId = selectedOption.Id,
            SelectedOptionText = selectedOption.OptionText,
            SelectedOptionIsCorrect = selectedOption.IsCorrect,
            CorrectAnswerText = quizQuestion.CorrectAnswer,
            QuestionResponseTimeInMilliseconds = request.QuestionResponseTimeInMilliseconds
        };

        // 9. Doğru/yanlış değerlendirmesi ayrı servisle yapılır.
        var evaluationResult = _quizAnswerEvaluator.Evaluate(evaluationRequest);

        // 10. QuizAnswer entity oluşturulur.
        // Multiple choice quiz için:
        // - SelectedQuizOptionId doludur.
        // - UserAnswer olarak seçilen option text saklanır.
        // - CorrectAnswer quiz question üzerindeki doğru cevap metnidir.
        // - AnswerResult backend tarafından hesaplanır.
        //
        // Dikkat:
        // Burada correctAnswer alanına doğru cevap metnini,
        // userAnswer alanına kullanıcının seçtiği option metnini yazıyoruz.
        var answerResult = ResolveAnswerResult(evaluationResult.IsCorrect);

        var quizAnswer = new QuizAnswer(
            quizQuestion.Id,
            keycloakUserId,
            evaluationResult.CorrectAnswerText,
            answerResult,
            evaluationResult.QuestionResponseTimeInMilliseconds ?? 0,
            selectedOption.Id,
            evaluationResult.SelectedOptionText);

        await _quizAnswerRepository.AddAsync(
            quizAnswer,
            cancellationToken);

        // 11. Bu question hangi LearningItem'dan üretildiyse,
        // current user'ın UserLearningItem kaydını buluyoruz.
        //
        // Yeni mimaride dictionary ownership kontrolü UserProfileId ile değil,
        // KeycloakUserId ile yapılır.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item => item.KeycloakUserId == keycloakUserId
                    && item.LearningItemId == quizQuestion.LearningItemId
                    && item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new BusinessRuleException(
                "The answered learning item is not saved in the current user's dictionary.",
                "ANSWERED_ITEM_NOT_FOUND_IN_USER_DICTIONARY");
        }

        // 12. UserLearningProgress kaydını buluyoruz.
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

        // 13. Confidence score hesaplanır.
        var scoreCalculationResult = _learningScoreCalculator.Calculate(
            new LearningScoreCalculationRequest
            {
                CurrentConfidenceScore = userLearningProgress.LearningConfidenceScore,
                IsCorrect = evaluationResult.IsCorrect,
                QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds
            });

        // 14. Review schedule hesaplanır.
        var reviewScheduleEvent = _reviewScheduleCalculator.Calculate(
            new ReviewScheduleCalculationRequest
            {
                IsCorrect = evaluationResult.IsCorrect,
                NewConfidenceScore = scoreCalculationResult.NewConfidenceScore,
                CurrentRepetitionLevel = userLearningProgress.RepetitionLevel,
                QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds,
                ReviewedAt = reviewedAt
            });

        // 15. Progress update sonucu hesaplanır.
        var progressUpdateResult = _learningProgressUpdater.CalculateUpdate(
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

        // 16. Hesaplanan progress state'i domain entity'ye uygulanır.
        // Entity update işi burada yapılır, hesaplama logic'i ise servislerde kalır.
        ApplyProgressUpdate(
            userLearningProgress,
            progressUpdateResult,
            evaluationResult.IsCorrect);

        // 17. Progress history oluşturulur.
        // Daha önce Faz 14'te kullandığımız constructor düzeniyle uyumludur.
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

        // 18. Tüm değişiklikler tek transaction/save akışında kaydedilir.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 19. Response hazırlanır.
        return new SubmitQuizAnswerResponse
        {
            QuizAnswerId = quizAnswer.Id,
            QuizSessionId = quizSession.Id,
            QuizQuestionId = quizQuestion.Id,
            SelectedQuizOptionId = selectedOption.Id,
            IsCorrect = evaluationResult.IsCorrect,
            SelectedOptionText = evaluationResult.SelectedOptionText,
            CorrectAnswerText = evaluationResult.CorrectAnswerText,
            QuestionResponseTimeInMilliseconds = evaluationResult.QuestionResponseTimeInMilliseconds,
            AnsweredAt = quizAnswer.AnsweredAt,

            CorrectCount = progressUpdateResult.CorrectCount,
            WrongCount = progressUpdateResult.WrongCount,
            ConsecutiveCorrectCount = progressUpdateResult.ConsecutiveCorrectCount,
            ConsecutiveWrongCount = progressUpdateResult.ConsecutiveWrongCount,

            PreviousLearningStatus = progressUpdateResult.PreviousLearningStatus.ToString(),
            CurrentLearningStatus = progressUpdateResult.NewLearningStatus.ToString(),

            PreviousConfidenceScore = progressUpdateResult.PreviousConfidenceScore,
            CurrentConfidenceScore = progressUpdateResult.NewConfidenceScore,

            NextReviewDate = progressUpdateResult.NextReviewDate
        };
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
    /// Boolean doğru/yanlış bilgisini domain AnswerResult enum değerine çevirir.
    /// 
    /// Enum değerleri projede Correct/Wrong, Correct/Incorrect gibi farklı isimlenmiş olabilir.
    /// Bu yüzden alias tabanlı güvenli resolver kullanıyoruz.
    /// </summary>
    private static AnswerResult ResolveAnswerResult(
        bool isCorrect)
    {
        var aliases = isCorrect
            ? ["Correct", "Right", "Success"]
            : new[] { "Wrong", "Incorrect", "False", "Failed" };

        foreach (var alias in aliases)
        {
            if (Enum.TryParse<AnswerResult>(
                    alias,
                    ignoreCase: true,
                    out var parsedResult))
            {
                return parsedResult;
            }
        }

        var supportedValues = string.Join(
            ", ",
            Enum.GetNames<AnswerResult>());

        throw new BusinessRuleException(
            $"Could not resolve answer result. Supported answer results: {supportedValues}.",
            "ANSWER_RESULT_NOT_SUPPORTED");
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