using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Dtos.Responses;
using Wordix.Application.Features.Quizzes.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Quizzes.Commands.SaveRecommendedItemToDictionary;

/// <summary>
/// Sistem önerisi item'ını current user'ın dictionary'sine ekleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ı KeycloakUserId üzerinden alır.
/// - QuizRecommendationItem kaydını bulur.
/// - QuizSession üzerinden ownership kontrolü yapar.
/// - LearningItem aktif mi kontrol eder.
/// - Kullanıcının dictionary'sinde kayıt var mı kontrol eder.
/// - Kayıt yoksa UserLearningItem oluşturur.
/// - Kayıt pasifse tekrar aktif eder.
/// - Progress yoksa UserLearningProgress oluşturur.
/// - LearningProgressHistory kaydı oluşturur.
/// - QuizRecommendationItem.WasAddedToDictionary değerini true yapar.
/// - SearchSuggestionLog.WasSaved değerini true yapar.
/// 
/// Bu handler ne yapmaz?
/// - Controller logic'i içermez.
/// - DbContext kullanmaz.
/// - HttpContext bilmez.
/// - UserProfileId kullanmaz.
/// </summary>
public sealed class SaveRecommendedItemToDictionaryCommandHandler
    : IRequestHandler<SaveRecommendedItemToDictionaryCommand, SaveRecommendedItemToDictionaryResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<QuizRecommendationItem> _quizRecommendationItemRepository;
    private readonly IRepository<SearchSuggestionLog> _searchSuggestionLogRepository;
    private readonly IRepository<QuizSession> _quizSessionRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;
    private readonly IRepository<LearningProgressHistory> _learningProgressHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveRecommendedItemToDictionaryCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<QuizRecommendationItem> quizRecommendationItemRepository,
        IRepository<SearchSuggestionLog> searchSuggestionLogRepository,
        IRepository<QuizSession> quizSessionRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository,
        IRepository<LearningProgressHistory> learningProgressHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _quizRecommendationItemRepository = quizRecommendationItemRepository;
        _searchSuggestionLogRepository = searchSuggestionLogRepository;
        _quizSessionRepository = quizSessionRepository;
        _learningItemRepository = learningItemRepository;
        _meaningRepository = meaningRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
        _learningProgressHistoryRepository = learningProgressHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaveRecommendedItemToDictionaryResponse> Handle(
        SaveRecommendedItemToDictionaryCommand request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        var recommendationItem = await _quizRecommendationItemRepository.FirstOrDefaultAsync(
            item => item.Id == request.QuizRecommendationItemId,
            cancellationToken);

        if (recommendationItem is null)
        {
            throw new NotFoundException(
                "Quiz recommendation item",
                request.QuizRecommendationItemId);
        }

        var quizSession = await _quizSessionRepository.FirstOrDefaultAsync(
            session => session.Id == recommendationItem.QuizSessionId,
            cancellationToken);

        if (quizSession is null)
        {
            throw new NotFoundException(
                "Quiz session",
                recommendationItem.QuizSessionId);
        }

        if (!string.Equals(
                quizSession.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot save another user's quiz recommendation to your dictionary.");
        }

        var learningItem = await _learningItemRepository.FirstOrDefaultAsync(
            item => item.Id == recommendationItem.LearningItemId && item.IsActive,
            cancellationToken);

        if (learningItem is null)
        {
            throw new NotFoundException(
                "Learning item",
                recommendationItem.LearningItemId);
        }

        var selectedMeaning = await GetPrimaryMeaningOrNullAsync(
            learningItem.Id,
            cancellationToken);

        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item =>
                item.KeycloakUserId == keycloakUserId &&
                item.LearningItemId == learningItem.Id,
            cancellationToken);

        var wasAlreadySaved = false;
        var wasReactivated = false;
        var shouldCreateProgressHistory = false;
        var progressHistoryMessage = "System recommended item saved to dictionary.";

        if (userLearningItem is null)
        {
            userLearningItem = new UserLearningItem(
                keycloakUserId,
                learningItem.Id,
                selectedMeaning?.Id,
                sourceLookupHistoryId: null);

            await _userLearningItemRepository.AddAsync(
                userLearningItem,
                cancellationToken);

            shouldCreateProgressHistory = true;
        }
        else if (userLearningItem.IsActive)
        {
            wasAlreadySaved = true;
        }
        else
        {
            userLearningItem.Activate();
            wasReactivated = true;
            shouldCreateProgressHistory = true;
            progressHistoryMessage = "System recommended item reactivated in dictionary.";

            if (userLearningItem.SelectedMeaningId is null &&
                selectedMeaning is not null)
            {
                userLearningItem.ChangeSelectedMeaning(selectedMeaning.Id);
            }
        }

        var userLearningProgress = await _userLearningProgressRepository.FirstOrDefaultAsync(
            progress => progress.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        if (userLearningProgress is null)
        {
            userLearningProgress = new UserLearningProgress(userLearningItem.Id);

            await _userLearningProgressRepository.AddAsync(
                userLearningProgress,
                cancellationToken);

            shouldCreateProgressHistory = true;
        }

        if (shouldCreateProgressHistory)
        {
            var progressHistory = new LearningProgressHistory(
                userLearningProgress.Id,
                userLearningProgress.LearningStatus,
                userLearningProgress.LearningStatus,
                userLearningProgress.LearningConfidenceScore,
                userLearningProgress.LearningConfidenceScore,
                progressHistoryMessage);

            await _learningProgressHistoryRepository.AddAsync(
                progressHistory,
                cancellationToken);
        }

        recommendationItem.MarkAsAddedToDictionary();

        await MarkSuggestionLogAsSavedAsync(
            keycloakUserId,
            recommendationItem,
            quizSession,
            learningItem,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QuizMapper.ToSaveRecommendedItemToDictionaryResponse(
            recommendationItem,
            userLearningItem,
            learningItem,
            userLearningProgress,
            wasAlreadySaved,
            wasReactivated);
    }

    /// <summary>
    /// LearningItem için primary meaning bulmaya çalışır.
    /// 
    /// Word/Phrase için anlam varsa SelectedMeaningId olarak kullanılabilir.
    /// Sentence için Meaning olmayabilir; bu durumda null dönmesi normaldir.
    /// </summary>
    private async Task<Meaning?> GetPrimaryMeaningOrNullAsync(
        Guid learningItemId,
        CancellationToken cancellationToken)
    {
        var primaryMeaning = await _meaningRepository.FirstOrDefaultAsync(
            meaning => meaning.LearningItemId == learningItemId && meaning.IsPrimary,
            cancellationToken);

        if (primaryMeaning is not null)
        {
            return primaryMeaning;
        }

        var meanings = await _meaningRepository.ListAsync(
            meaning => meaning.LearningItemId == learningItemId,
            cancellationToken);

        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .FirstOrDefault();
    }

    /// <summary>
    /// SearchSuggestionLog kaydını saved olarak işaretler.
    /// 
    /// Normalde StartQuizCommandHandler sistem önerisi oluştururken log da oluşturur.
    /// Ancak defensive davranıyoruz:
    /// Log herhangi bir sebeple bulunamazsa yeni log oluşturup saved işaretliyoruz.
    /// </summary>
    private async Task MarkSuggestionLogAsSavedAsync(
        string keycloakUserId,
        QuizRecommendationItem recommendationItem,
        QuizSession quizSession,
        LearningItem learningItem,
        CancellationToken cancellationToken)
    {
        var suggestionLog = await _searchSuggestionLogRepository.FirstOrDefaultAsync(
            log =>
                log.KeycloakUserId == keycloakUserId &&
                log.QuizRecommendationItemId == recommendationItem.Id,
            cancellationToken);

        if (suggestionLog is null)
        {
            suggestionLog = new SearchSuggestionLog(
                keycloakUserId,
                learningItem.Id,
                recommendationItem.RecommendationReason,
                quizSession.Id,
                recommendationItem.Id);

            suggestionLog.MarkAsSaved();

            await _searchSuggestionLogRepository.AddAsync(
                suggestionLog,
                cancellationToken);

            return;
        }

        suggestionLog.MarkAsSaved();
    }
}