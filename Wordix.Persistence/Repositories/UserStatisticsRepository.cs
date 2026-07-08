using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserStatistics.Models;
using Wordix.Domain.Enums;
using Wordix.Persistence.Contexts;
using Wordix.Shared.Responses;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// Current user'ın öğrenme statistics/dashboard verilerini EF Core ile üreten repository implementasyonudur.
/// 
/// Bu repository neden var?
/// - User statistics sorguları basit CRUD değildir.
/// - Birden fazla tabloyu join eder.
/// - Count, Average, GroupBy, pagination ve bucket hesaplamaları yapar.
/// - Bu sorgular Application handler içine yazılırsa Application katmanı DbContext bilmek zorunda kalır.
/// 
/// Mimari karar:
/// - Application katmanı sadece IUserStatisticsRepository abstraction'ını bilir.
/// - EF Core ve SQL detayları Persistence katmanında kalır.
/// - Tüm sorgular KeycloakUserId ile filtrelenir.
/// - Client request üzerinden user id göndermez.
/// </summary>
public sealed class UserStatisticsRepository : IUserStatisticsRepository
{
    private readonly WordixDbContext _dbContext;

    public UserStatisticsRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Current user'ın genel öğrenme özetini döndürür.
    /// 
    /// Kaynaklar:
    /// - UserLearningItems
    /// - UserLearningProgresses
    /// - UserLearningFlags
    /// - LearningItems
    /// - QuizSessions
    /// - QuizAnswers
    /// </summary>
    public async Task<UserLearningSummaryModel> GetLearningSummaryAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureKeycloakUserId(keycloakUserId);

        var nowUtc = DateTime.UtcNow;

        var userLearningItemQuery = _dbContext.UserLearningItems
            .AsNoTracking()
            .Where(item => item.KeycloakUserId == keycloakUserId);

        var activeLearningItemQuery =
            from userItem in userLearningItemQuery
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on userItem.LearningItemId equals learningItem.Id
            where userItem.IsActive && learningItem.IsActive
            select new
            {
                UserLearningItem = userItem,
                LearningItem = learningItem
            };

        var progressQuery =
            from userItem in userLearningItemQuery
            join progress in _dbContext.UserLearningProgresses.AsNoTracking()
                on userItem.Id equals progress.UserLearningItemId
            where userItem.IsActive
            select progress;

        var flagQuery =
            from flag in _dbContext.UserLearningFlags.AsNoTracking()
            join userItem in userLearningItemQuery
                on flag.UserLearningItemId equals userItem.Id
            where userItem.IsActive
            select flag;

        var answerQuery = _dbContext.QuizAnswers
            .AsNoTracking()
            .Where(answer => answer.KeycloakUserId == keycloakUserId);

        var quizSessionQuery = _dbContext.QuizSessions
            .AsNoTracking()
            .Where(session => session.KeycloakUserId == keycloakUserId);

        var totalSavedItemCount = await userLearningItemQuery
            .CountAsync(cancellationToken);

        var activeSavedItemCount = await userLearningItemQuery
            .CountAsync(item => item.IsActive, cancellationToken);

        var wordCount = await activeLearningItemQuery
            .CountAsync(row => row.LearningItem.ItemType == LearningItemType.Word, cancellationToken);

        var phraseCount = await activeLearningItemQuery
            .CountAsync(row => row.LearningItem.ItemType == LearningItemType.Phrase, cancellationToken);

        var sentenceCount = await activeLearningItemQuery
            .CountAsync(row => row.LearningItem.ItemType == LearningItemType.Sentence, cancellationToken);

        var newItemCount = await progressQuery
            .CountAsync(progress => progress.LearningStatus == LearningStatus.New, cancellationToken);

        var learningItemCount = await progressQuery
            .CountAsync(progress => progress.LearningStatus == LearningStatus.Learning, cancellationToken);

        var reviewingItemCount = await progressQuery
            .CountAsync(progress => progress.LearningStatus == LearningStatus.Reviewing, cancellationToken);

        var learnedItemCount = await progressQuery
            .CountAsync(progress => progress.LearningStatus == LearningStatus.Learned, cancellationToken);

        var masteredItemCount = await progressQuery
            .CountAsync(progress => progress.LearningStatus == LearningStatus.Mastered, cancellationToken);

        var reviewDueItemCount = await progressQuery
            .CountAsync(progress =>
                progress.NextReviewDate.HasValue &&
                progress.NextReviewDate.Value <= nowUtc,
                cancellationToken);

        var averageConfidenceScore = await progressQuery
            .Select(progress => (double?)progress.LearningConfidenceScore)
            .AverageAsync(cancellationToken) ?? 0;

        var favoriteItemCount = await flagQuery
            .CountAsync(flag => flag.FlagType == UserLearningFlagType.Favorite, cancellationToken);

        var difficultItemCount = await flagQuery
            .CountAsync(flag => flag.FlagType == UserLearningFlagType.Difficult, cancellationToken);

        var wantMorePracticeItemCount = await flagQuery
            .CountAsync(flag => flag.FlagType == UserLearningFlagType.WantMorePractice, cancellationToken);

        var ignoredItemCount = await flagQuery
            .CountAsync(flag => flag.FlagType == UserLearningFlagType.Ignored, cancellationToken);

        var totalCorrectAnswerCount = await answerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.Correct, cancellationToken);

        var totalIncorrectAnswerCount = await answerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.Incorrect, cancellationToken);

        var totalPartiallyCorrectAnswerCount = await answerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.PartiallyCorrect, cancellationToken);

        var totalSkippedAnswerCount = await answerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.Skipped, cancellationToken);

        var totalAnswerCount =
            totalCorrectAnswerCount +
            totalIncorrectAnswerCount +
            totalPartiallyCorrectAnswerCount +
            totalSkippedAnswerCount;

        var overallAccuracyRate = totalAnswerCount == 0
            ? 0
            : totalCorrectAnswerCount * 100.0 / totalAnswerCount;

        var lastReviewedAtUtc = await progressQuery
            .Select(progress => progress.LastReviewedAt)
            .MaxAsync(cancellationToken);

        var nextReviewDateUtc = await progressQuery
            .Where(progress => progress.NextReviewDate.HasValue)
            .Select(progress => progress.NextReviewDate)
            .MinAsync(cancellationToken);

        var lastQuizStartedAtUtc = await quizSessionQuery
            .Select(session => (DateTime?)session.StartedAt)
            .MaxAsync(cancellationToken);

        return new UserLearningSummaryModel
        {
            TotalSavedItemCount = totalSavedItemCount,
            ActiveSavedItemCount = activeSavedItemCount,

            WordCount = wordCount,
            PhraseCount = phraseCount,
            SentenceCount = sentenceCount,

            NewItemCount = newItemCount,
            LearningItemCount = learningItemCount,
            ReviewingItemCount = reviewingItemCount,
            LearnedItemCount = learnedItemCount,
            MasteredItemCount = masteredItemCount,

            ReviewDueItemCount = reviewDueItemCount,
            AverageConfidenceScore = averageConfidenceScore,

            FavoriteItemCount = favoriteItemCount,
            DifficultItemCount = difficultItemCount,
            WantMorePracticeItemCount = wantMorePracticeItemCount,
            IgnoredItemCount = ignoredItemCount,

            TotalCorrectAnswerCount = totalCorrectAnswerCount,
            TotalIncorrectAnswerCount = totalIncorrectAnswerCount,
            TotalPartiallyCorrectAnswerCount = totalPartiallyCorrectAnswerCount,
            TotalSkippedAnswerCount = totalSkippedAnswerCount,
            OverallAccuracyRate = overallAccuracyRate,

            LastReviewedAtUtc = lastReviewedAtUtc,
            NextReviewDateUtc = nextReviewDateUtc,
            LastQuizStartedAtUtc = lastQuizStartedAtUtc,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Current user'ın quiz performans istatistiklerini döndürür.
    /// </summary>
    public async Task<QuizStatisticsModel> GetQuizStatisticsAsync(
        string keycloakUserId,
        QuizStatisticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        EnsureKeycloakUserId(keycloakUserId);
        ArgumentNullException.ThrowIfNull(filter);

        var sessionQuery = _dbContext.QuizSessions
            .AsNoTracking()
            .Where(session => session.KeycloakUserId == keycloakUserId);

        sessionQuery = ApplyQuizStatisticsFilter(sessionQuery, filter);

        var questionQuery =
            from question in _dbContext.QuizQuestions.AsNoTracking()
            join session in sessionQuery
                on question.QuizSessionId equals session.Id
            select question;

        var answerQuery =
            from answer in _dbContext.QuizAnswers.AsNoTracking()
            join question in questionQuery
                on answer.QuizQuestionId equals question.Id
            where answer.KeycloakUserId == keycloakUserId
            select new
            {
                Answer = answer,
                Question = question
            };

        var totalQuizSessionCount = await sessionQuery.CountAsync(cancellationToken);

        var completedQuizSessionCount = await sessionQuery
            .CountAsync(session => session.Status == QuizSessionStatus.Completed, cancellationToken);

        var inProgressQuizSessionCount = await sessionQuery
            .CountAsync(session => session.Status == QuizSessionStatus.InProgress, cancellationToken);

        var cancelledQuizSessionCount = await sessionQuery
            .CountAsync(session => session.Status == QuizSessionStatus.Cancelled, cancellationToken);

        var testQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizType == QuizType.Test, cancellationToken);

        var writingQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizType == QuizType.Writing, cancellationToken);

        var mixedQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizType == QuizType.Mixed, cancellationToken);

        var userDictionaryQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizSourceType == QuizSourceType.UserDictionary, cancellationToken);

        var deckQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizSourceType == QuizSourceType.Deck, cancellationToken);

        var difficultItemsQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizSourceType == QuizSourceType.DifficultItems, cancellationToken);

        var systemRecommendationsQuizSessionCount = await sessionQuery
            .CountAsync(session => session.QuizSourceType == QuizSourceType.SystemRecommendations, cancellationToken);

        var totalQuestionCount = await questionQuery.CountAsync(cancellationToken);

        var systemRecommendedQuestionCount = await questionQuery
            .CountAsync(question => question.IsSystemRecommended, cancellationToken);

        var totalAnswerCount = await answerQuery.CountAsync(cancellationToken);

        var correctAnswerCount = await answerQuery
            .CountAsync(row => row.Answer.AnswerResult == AnswerResult.Correct, cancellationToken);

        var incorrectAnswerCount = await answerQuery
            .CountAsync(row => row.Answer.AnswerResult == AnswerResult.Incorrect, cancellationToken);

        var partiallyCorrectAnswerCount = await answerQuery
            .CountAsync(row => row.Answer.AnswerResult == AnswerResult.PartiallyCorrect, cancellationToken);

        var skippedAnswerCount = await answerQuery
            .CountAsync(row => row.Answer.AnswerResult == AnswerResult.Skipped, cancellationToken);

        var systemRecommendedCorrectAnswerCount = await answerQuery
            .CountAsync(row =>
                row.Question.IsSystemRecommended &&
                row.Answer.AnswerResult == AnswerResult.Correct,
                cancellationToken);

        var systemRecommendedIncorrectAnswerCount = await answerQuery
            .CountAsync(row =>
                row.Question.IsSystemRecommended &&
                row.Answer.AnswerResult == AnswerResult.Incorrect,
                cancellationToken);

        var accuracyRate = totalAnswerCount == 0
            ? 0
            : correctAnswerCount * 100.0 / totalAnswerCount;

        var averageResponseTimeMs = await answerQuery
            .Select(row => (double?)row.Answer.ResponseTimeMilliseconds)
            .AverageAsync(cancellationToken) ?? 0;

        var lastQuizStartedAtUtc = await sessionQuery
            .Select(session => (DateTime?)session.StartedAt)
            .MaxAsync(cancellationToken);

        var lastAnsweredAtUtc = await answerQuery
            .Select(row => (DateTime?)row.Answer.AnsweredAt)
            .MaxAsync(cancellationToken);

        return new QuizStatisticsModel
        {
            TotalQuizSessionCount = totalQuizSessionCount,
            CompletedQuizSessionCount = completedQuizSessionCount,
            InProgressQuizSessionCount = inProgressQuizSessionCount,
            CancelledQuizSessionCount = cancelledQuizSessionCount,

            TestQuizSessionCount = testQuizSessionCount,
            WritingQuizSessionCount = writingQuizSessionCount,
            MixedQuizSessionCount = mixedQuizSessionCount,

            UserDictionaryQuizSessionCount = userDictionaryQuizSessionCount,
            DeckQuizSessionCount = deckQuizSessionCount,
            DifficultItemsQuizSessionCount = difficultItemsQuizSessionCount,
            SystemRecommendationsQuizSessionCount = systemRecommendationsQuizSessionCount,

            TotalQuestionCount = totalQuestionCount,
            SystemRecommendedQuestionCount = systemRecommendedQuestionCount,

            TotalAnswerCount = totalAnswerCount,
            CorrectAnswerCount = correctAnswerCount,
            IncorrectAnswerCount = incorrectAnswerCount,
            PartiallyCorrectAnswerCount = partiallyCorrectAnswerCount,
            SkippedAnswerCount = skippedAnswerCount,

            SystemRecommendedCorrectAnswerCount = systemRecommendedCorrectAnswerCount,
            SystemRecommendedIncorrectAnswerCount = systemRecommendedIncorrectAnswerCount,

            AccuracyRate = accuracyRate,
            AverageResponseTimeMs = averageResponseTimeMs,
            LastQuizStartedAtUtc = lastQuizStartedAtUtc,
            LastAnsweredAtUtc = lastAnsweredAtUtc,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Current user'ın zorlandığı learning itemları sayfalı şekilde döndürür.
    /// </summary>
    public async Task<PagedResult<DifficultLearningItemModel>> GetDifficultItemsAsync(
        string keycloakUserId,
        DifficultItemsFilter filter,
        CancellationToken cancellationToken = default)
    {
        EnsureKeycloakUserId(keycloakUserId);
        ArgumentNullException.ThrowIfNull(filter);

        var nowUtc = DateTime.UtcNow;

        var query =
            from userItem in _dbContext.UserLearningItems.AsNoTracking()
            join progress in _dbContext.UserLearningProgresses.AsNoTracking()
                on userItem.Id equals progress.UserLearningItemId
            join learningItem in _dbContext.LearningItems.AsNoTracking()
                on userItem.LearningItemId equals learningItem.Id
            let isManuallyMarkedDifficult = _dbContext.UserLearningFlags
                .AsNoTracking()
                .Any(flag =>
                    flag.UserLearningItemId == userItem.Id &&
                    flag.FlagType == UserLearningFlagType.Difficult)
            let isProgressDifficult =
                progress.LearningConfidenceScore <= 60 ||
                progress.ConsecutiveWrongCount > 0 ||
                progress.WrongCount > progress.CorrectCount ||
                progress.NextReviewDate.HasValue && progress.NextReviewDate.Value <= nowUtc
            where userItem.KeycloakUserId == keycloakUserId &&
                  userItem.IsActive &&
                  learningItem.IsActive
            select new DifficultLearningItemQueryRow
            {
                UserLearningItemId = userItem.Id,
                LearningItemId = userItem.LearningItemId,
                ItemType = learningItem.ItemType,
                LearningStatus = progress.LearningStatus,
                ConfidenceScore = progress.LearningConfidenceScore,
                CorrectCount = progress.CorrectCount,
                WrongCount = progress.WrongCount,
                ConsecutiveWrongCount = progress.ConsecutiveWrongCount,
                RepetitionLevel = progress.RepetitionLevel,
                IsManuallyMarkedDifficult = isManuallyMarkedDifficult,
                IsProgressDifficult = isProgressDifficult,
                SavedAtUtc = userItem.SavedAt,
                LastReviewedAtUtc = progress.LastReviewedAt,
                NextReviewDateUtc = progress.NextReviewDate
            };

        query = ApplyDifficultItemsFilter(query, filter);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyDifficultItemsSorting(query, filter.SortBy);

        var skip = (filter.PageNumber - 1) * filter.PageSize;

        var pageRows = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        if (pageRows.Count == 0)
        {
            return PagedResult<DifficultLearningItemModel>.Create(
                Array.Empty<DifficultLearningItemModel>(),
                filter.PageNumber,
                filter.PageSize,
                totalCount);
        }

        var learningItemIds = pageRows
            .Select(row => row.LearningItemId)
            .Distinct()
            .ToArray();

        var details = await GetLearningItemDetailsAsync(
            learningItemIds,
            cancellationToken);

        var items = pageRows
            .Select(row =>
            {
                details.TryGetValue(row.LearningItemId, out var detail);

                return new DifficultLearningItemModel
                {
                    UserLearningItemId = row.UserLearningItemId,
                    LearningItemId = row.LearningItemId,
                    ItemType = row.ItemType,
                    DisplayText = detail?.DisplayText ?? string.Empty,
                    PrimaryMeaning = detail?.PrimaryMeaning,

                    LearningStatus = row.LearningStatus,
                    ConfidenceScore = row.ConfidenceScore,
                    CorrectCount = row.CorrectCount,
                    WrongCount = row.WrongCount,
                    ConsecutiveWrongCount = row.ConsecutiveWrongCount,
                    RepetitionLevel = row.RepetitionLevel,

                    IsManuallyMarkedDifficult = row.IsManuallyMarkedDifficult,
                    IsProgressDifficult = row.IsProgressDifficult,
                    DifficultyReason = ResolveDifficultyReason(row),

                    SavedAtUtc = row.SavedAtUtc,
                    LastReviewedAtUtc = row.LastReviewedAtUtc,
                    NextReviewDateUtc = row.NextReviewDateUtc
                };
            })
            .ToArray();

        return PagedResult<DifficultLearningItemModel>.Create(
            items,
            filter.PageNumber,
            filter.PageSize,
            totalCount);
    }

    /// <summary>
    /// Current user'ın deck bazlı statistics verilerini döndürür.
    /// </summary>
    public async Task<IReadOnlyCollection<DeckStatisticsModel>> GetDeckStatisticsAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureKeycloakUserId(keycloakUserId);

        var nowUtc = DateTime.UtcNow;

        var deckRows = await _dbContext.Decks
            .AsNoTracking()
            .Where(deck => deck.KeycloakUserId == keycloakUserId)
            .Select(deck => new
            {
                deck.Id,
                deck.Name,
                deck.Description,
                deck.IsActive
            })
            .ToListAsync(cancellationToken);

        if (deckRows.Count == 0)
        {
            return Array.Empty<DeckStatisticsModel>();
        }

        var deckIds = deckRows
            .Select(deck => deck.Id)
            .ToArray();

        var deckItemStats = await (
            from deckItem in _dbContext.DeckItems.AsNoTracking()
            join userItem in _dbContext.UserLearningItems.AsNoTracking()
                on deckItem.UserLearningItemId equals userItem.Id
            join progress in _dbContext.UserLearningProgresses.AsNoTracking()
                on userItem.Id equals progress.UserLearningItemId
            where deckIds.Contains(deckItem.DeckId) &&
                  userItem.KeycloakUserId == keycloakUserId
            let isDifficultFlag = _dbContext.UserLearningFlags
                .AsNoTracking()
                .Any(flag =>
                    flag.UserLearningItemId == userItem.Id &&
                    flag.FlagType == UserLearningFlagType.Difficult)
            let isProgressDifficult =
                progress.LearningConfidenceScore <= 60 ||
                progress.ConsecutiveWrongCount > 0 ||
                progress.WrongCount > progress.CorrectCount ||
                progress.NextReviewDate.HasValue && progress.NextReviewDate.Value <= nowUtc
            group new
            {
                userItem,
                progress,
                isDifficultFlag,
                isProgressDifficult,
                deckItem
            }
            by deckItem.DeckId
            into groupRow
            select new
            {
                DeckId = groupRow.Key,
                ItemCount = groupRow.Count(),
                ActiveItemCount = groupRow.Count(row => row.userItem.IsActive),
                AverageConfidenceScore = groupRow
                    .Select(row => (double?)row.progress.LearningConfidenceScore)
                    .Average() ?? 0,
                DueReviewItemCount = groupRow.Count(row =>
                    row.progress.NextReviewDate.HasValue &&
                    row.progress.NextReviewDate.Value <= nowUtc),
                DifficultItemCount = groupRow.Count(row =>
                    row.isDifficultFlag || row.isProgressDifficult),
                LastItemAddedAtUtc = groupRow
                    .Select(row => (DateTime?)row.deckItem.AddedAt)
                    .Max()
            })
            .ToListAsync(cancellationToken);

        var deckQuizSessionStats = await _dbContext.QuizSessions
            .AsNoTracking()
            .Where(session =>
                session.KeycloakUserId == keycloakUserId &&
                session.DeckId.HasValue &&
                deckIds.Contains(session.DeckId.Value))
            .GroupBy(session => session.DeckId!.Value)
            .Select(group => new
            {
                DeckId = group.Key,
                QuizSessionCount = group.Count(),
                CompletedQuizSessionCount = group.Count(session => session.Status == QuizSessionStatus.Completed),
                LastQuizStartedAtUtc = group.Select(session => (DateTime?)session.StartedAt).Max()
            })
            .ToListAsync(cancellationToken);

        var deckAnswerStats = await (
            from session in _dbContext.QuizSessions.AsNoTracking()
            join question in _dbContext.QuizQuestions.AsNoTracking()
                on session.Id equals question.QuizSessionId
            join answer in _dbContext.QuizAnswers.AsNoTracking()
                on question.Id equals answer.QuizQuestionId
            where session.KeycloakUserId == keycloakUserId &&
                  answer.KeycloakUserId == keycloakUserId &&
                  session.DeckId.HasValue &&
                  deckIds.Contains(session.DeckId.Value)
            group answer
            by session.DeckId!.Value
            into groupRow
            select new
            {
                DeckId = groupRow.Key,
                TotalAnswerCount = groupRow.Count(),
                CorrectAnswerCount = groupRow.Count(answer => answer.AnswerResult == AnswerResult.Correct),
                IncorrectAnswerCount = groupRow.Count(answer => answer.AnswerResult == AnswerResult.Incorrect)
            })
            .ToListAsync(cancellationToken);

        var deckItemStatsByDeckId = deckItemStats.ToDictionary(
            row => row.DeckId,
            row => row);

        var deckQuizStatsByDeckId = deckQuizSessionStats.ToDictionary(
            row => row.DeckId,
            row => row);

        var deckAnswerStatsByDeckId = deckAnswerStats.ToDictionary(
            row => row.DeckId,
            row => row);

        return deckRows
            .Select(deck =>
            {
                deckItemStatsByDeckId.TryGetValue(deck.Id, out var itemStats);
                deckQuizStatsByDeckId.TryGetValue(deck.Id, out var quizStats);
                deckAnswerStatsByDeckId.TryGetValue(deck.Id, out var answerStats);

                var totalAnswerCount = answerStats?.TotalAnswerCount ?? 0;
                var correctAnswerCount = answerStats?.CorrectAnswerCount ?? 0;

                var accuracyRate = totalAnswerCount == 0
                    ? 0
                    : correctAnswerCount * 100.0 / totalAnswerCount;

                return new DeckStatisticsModel
                {
                    DeckId = deck.Id,
                    DeckName = deck.Name,
                    Description = deck.Description,
                    IsActive = deck.IsActive,

                    ItemCount = itemStats?.ItemCount ?? 0,
                    ActiveItemCount = itemStats?.ActiveItemCount ?? 0,
                    AverageConfidenceScore = itemStats?.AverageConfidenceScore ?? 0,
                    DueReviewItemCount = itemStats?.DueReviewItemCount ?? 0,
                    DifficultItemCount = itemStats?.DifficultItemCount ?? 0,

                    QuizSessionCount = quizStats?.QuizSessionCount ?? 0,
                    CompletedQuizSessionCount = quizStats?.CompletedQuizSessionCount ?? 0,

                    TotalAnswerCount = totalAnswerCount,
                    CorrectAnswerCount = correctAnswerCount,
                    IncorrectAnswerCount = answerStats?.IncorrectAnswerCount ?? 0,
                    AccuracyRate = accuracyRate,

                    LastQuizStartedAtUtc = quizStats?.LastQuizStartedAtUtc,
                    LastItemAddedAtUtc = itemStats?.LastItemAddedAtUtc
                };
            })
            .ToArray();
    }

    /// <summary>
    /// Current user'ın confidence score dağılımını döndürür.
    /// </summary>
    public async Task<ConfidenceScoreDistributionModel> GetConfidenceScoreDistributionAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default)
    {
        EnsureKeycloakUserId(keycloakUserId);

        var scoreQuery =
            from userItem in _dbContext.UserLearningItems.AsNoTracking()
            join progress in _dbContext.UserLearningProgresses.AsNoTracking()
                on userItem.Id equals progress.UserLearningItemId
            where userItem.KeycloakUserId == keycloakUserId &&
                  userItem.IsActive
            select progress.LearningConfidenceScore;

        var totalItemCount = await scoreQuery.CountAsync(cancellationToken);

        var averageConfidenceScore = await scoreQuery
            .Select(score => (double?)score)
            .AverageAsync(cancellationToken) ?? 0;

        var veryLowCount = await scoreQuery
            .CountAsync(score => score >= 0 && score <= 20, cancellationToken);

        var lowCount = await scoreQuery
            .CountAsync(score => score >= 21 && score <= 40, cancellationToken);

        var mediumCount = await scoreQuery
            .CountAsync(score => score >= 41 && score <= 60, cancellationToken);

        var highCount = await scoreQuery
            .CountAsync(score => score >= 61 && score <= 80, cancellationToken);

        var veryHighCount = await scoreQuery
            .CountAsync(score => score >= 81 && score <= 100, cancellationToken);

        var buckets = new[]
        {
            CreateConfidenceBucket(
                UserStatisticsConstants.ConfidenceBucketLabels.VeryLow,
                minScore: 0,
                maxScore: 20,
                itemCount: veryLowCount,
                totalItemCount: totalItemCount),

            CreateConfidenceBucket(
                UserStatisticsConstants.ConfidenceBucketLabels.Low,
                minScore: 21,
                maxScore: 40,
                itemCount: lowCount,
                totalItemCount: totalItemCount),

            CreateConfidenceBucket(
                UserStatisticsConstants.ConfidenceBucketLabels.Medium,
                minScore: 41,
                maxScore: 60,
                itemCount: mediumCount,
                totalItemCount: totalItemCount),

            CreateConfidenceBucket(
                UserStatisticsConstants.ConfidenceBucketLabels.High,
                minScore: 61,
                maxScore: 80,
                itemCount: highCount,
                totalItemCount: totalItemCount),

            CreateConfidenceBucket(
                UserStatisticsConstants.ConfidenceBucketLabels.VeryHigh,
                minScore: 81,
                maxScore: 100,
                itemCount: veryHighCount,
                totalItemCount: totalItemCount)
        };

        return new ConfidenceScoreDistributionModel
        {
            TotalItemCount = totalItemCount,
            AverageConfidenceScore = averageConfidenceScore,
            GeneratedAtUtc = DateTime.UtcNow,
            Buckets = buckets
        };
    }

    private static IQueryable<Domain.Entities.QuizSession> ApplyQuizStatisticsFilter(
        IQueryable<Domain.Entities.QuizSession> query,
        QuizStatisticsFilter filter)
    {
        if (filter.DateRange.FromUtc.HasValue)
        {
            query = query.Where(session => session.StartedAt >= filter.DateRange.FromUtc.Value);
        }

        if (filter.DateRange.ToUtc.HasValue)
        {
            query = query.Where(session => session.StartedAt <= filter.DateRange.ToUtc.Value);
        }

        if (filter.QuizType.HasValue)
        {
            query = query.Where(session => session.QuizType == filter.QuizType.Value);
        }

        if (filter.QuizSourceType.HasValue)
        {
            query = query.Where(session => session.QuizSourceType == filter.QuizSourceType.Value);
        }

        if (filter.QuizContentMode.HasValue)
        {
            query = query.Where(session => session.QuizContentMode == filter.QuizContentMode.Value);
        }

        if (filter.DifficultyGroup.HasValue)
        {
            query = query.Where(session => session.DifficultyGroup == filter.DifficultyGroup.Value);
        }

        return query;
    }

    private static IQueryable<DifficultLearningItemQueryRow> ApplyDifficultItemsFilter(
        IQueryable<DifficultLearningItemQueryRow> query,
        DifficultItemsFilter filter)
    {
        if (filter.ItemType.HasValue)
        {
            query = query.Where(row => row.ItemType == filter.ItemType.Value);
        }

        if (filter.LearningStatus.HasValue)
        {
            query = query.Where(row => row.LearningStatus == filter.LearningStatus.Value);
        }

        query = filter.Source switch
        {
            UserStatisticsConstants.DifficultItemSources.Manual =>
                query.Where(row => row.IsManuallyMarkedDifficult),

            UserStatisticsConstants.DifficultItemSources.Progress =>
                query.Where(row => row.IsProgressDifficult),

            _ =>
                query.Where(row => row.IsManuallyMarkedDifficult || row.IsProgressDifficult)
        };

        return query;
    }

    private static IQueryable<DifficultLearningItemQueryRow> ApplyDifficultItemsSorting(
        IQueryable<DifficultLearningItemQueryRow> query,
        string sortBy)
    {
        return sortBy switch
        {
            UserStatisticsConstants.DifficultItemSortBy.WrongCountDesc =>
                query
                    .OrderByDescending(row => row.WrongCount)
                    .ThenBy(row => row.ConfidenceScore)
                    .ThenBy(row => row.SavedAtUtc),

            UserStatisticsConstants.DifficultItemSortBy.ConsecutiveWrongDesc =>
                query
                    .OrderByDescending(row => row.ConsecutiveWrongCount)
                    .ThenBy(row => row.ConfidenceScore)
                    .ThenBy(row => row.SavedAtUtc),

            UserStatisticsConstants.DifficultItemSortBy.NextReviewAsc =>
                query
                    .OrderBy(row => row.NextReviewDateUtc ?? DateTime.MaxValue)
                    .ThenBy(row => row.ConfidenceScore)
                    .ThenBy(row => row.SavedAtUtc),

            UserStatisticsConstants.DifficultItemSortBy.SavedAtDesc =>
                query
                    .OrderByDescending(row => row.SavedAtUtc)
                    .ThenBy(row => row.ConfidenceScore),

            _ =>
                query
                    .OrderBy(row => row.ConfidenceScore)
                    .ThenByDescending(row => row.WrongCount)
                    .ThenBy(row => row.SavedAtUtc)
        };
    }

    private async Task<IReadOnlyDictionary<Guid, LearningItemDetails>> GetLearningItemDetailsAsync(
        IReadOnlyCollection<Guid> learningItemIds,
        CancellationToken cancellationToken)
    {
        if (learningItemIds.Count == 0)
        {
            return new Dictionary<Guid, LearningItemDetails>();
        }

        var ids = learningItemIds.ToArray();

        var learningItems = await _dbContext.LearningItems
            .AsNoTracking()
            .Where(item => ids.Contains(item.Id))
            .Select(item => new
            {
                item.Id,
                item.ItemType
            })
            .ToListAsync(cancellationToken);

        var wordRows = await _dbContext.Words
            .AsNoTracking()
            .Where(word => ids.Contains(word.LearningItemId))
            .Select(word => new
            {
                word.LearningItemId,
                word.Text
            })
            .ToListAsync(cancellationToken);

        var phraseRows = await _dbContext.Phrases
            .AsNoTracking()
            .Where(phrase => ids.Contains(phrase.LearningItemId))
            .Select(phrase => new
            {
                phrase.LearningItemId,
                phrase.Text
            })
            .ToListAsync(cancellationToken);

        var sentenceRows = await _dbContext.Sentences
            .AsNoTracking()
            .Where(sentence =>
                sentence.LearningItemId.HasValue &&
                ids.Contains(sentence.LearningItemId.Value))
            .Select(sentence => new
            {
                LearningItemId = sentence.LearningItemId!.Value,
                sentence.Text
            })
            .ToListAsync(cancellationToken);

        var meaningRows = await _dbContext.Meanings
            .AsNoTracking()
            .Where(meaning => ids.Contains(meaning.LearningItemId))
            .OrderByDescending(meaning => meaning.IsPrimary)
            .ThenBy(meaning => meaning.DisplayOrder)
            .Select(meaning => new
            {
                meaning.LearningItemId,
                meaning.MeaningText
            })
            .ToListAsync(cancellationToken);

        var wordTextByLearningItemId = wordRows
            .GroupBy(word => word.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.First().Text);

        var phraseTextByLearningItemId = phraseRows
            .GroupBy(phrase => phrase.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.First().Text);

        var sentenceTextByLearningItemId = sentenceRows
            .GroupBy(sentence => sentence.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.First().Text);

        var primaryMeaningByLearningItemId = meaningRows
            .GroupBy(meaning => meaning.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.First().MeaningText);

        return learningItems
            .Select(item =>
            {
                var displayText = ResolveDisplayText(
                    item.Id,
                    item.ItemType,
                    wordTextByLearningItemId,
                    phraseTextByLearningItemId,
                    sentenceTextByLearningItemId);

                primaryMeaningByLearningItemId.TryGetValue(
                    item.Id,
                    out var primaryMeaning);

                return new LearningItemDetails
                {
                    LearningItemId = item.Id,
                    DisplayText = displayText,
                    PrimaryMeaning = primaryMeaning
                };
            })
            .ToDictionary(
                item => item.LearningItemId,
                item => item);
    }

    private static string ResolveDisplayText(
        Guid learningItemId,
        LearningItemType itemType,
        IReadOnlyDictionary<Guid, string> wordTexts,
        IReadOnlyDictionary<Guid, string> phraseTexts,
        IReadOnlyDictionary<Guid, string> sentenceTexts)
    {
        return itemType switch
        {
            LearningItemType.Word when wordTexts.TryGetValue(learningItemId, out var wordText) =>
                wordText,

            LearningItemType.Phrase when phraseTexts.TryGetValue(learningItemId, out var phraseText) =>
                phraseText,

            LearningItemType.Sentence when sentenceTexts.TryGetValue(learningItemId, out var sentenceText) =>
                sentenceText,

            _ => string.Empty
        };
    }

    private static ConfidenceScoreBucketModel CreateConfidenceBucket(
        string label,
        int minScore,
        int maxScore,
        int itemCount,
        int totalItemCount)
    {
        var percentage = totalItemCount == 0
            ? 0
            : itemCount * 100.0 / totalItemCount;

        return new ConfidenceScoreBucketModel
        {
            Label = label,
            MinScore = minScore,
            MaxScore = maxScore,
            ItemCount = itemCount,
            Percentage = percentage
        };
    }

    private static string ResolveDifficultyReason(
        DifficultLearningItemQueryRow row)
    {
        if (row.IsManuallyMarkedDifficult)
        {
            return "Manually marked as difficult";
        }

        if (row.ConfidenceScore <= 60)
        {
            return "Low confidence score";
        }

        if (row.ConsecutiveWrongCount > 0)
        {
            return "Consecutive wrong answers";
        }

        if (row.WrongCount > row.CorrectCount)
        {
            return "More incorrect answers than correct answers";
        }

        if (row.NextReviewDateUtc.HasValue &&
            row.NextReviewDateUtc.Value <= DateTime.UtcNow)
        {
            return "Review is due";
        }

        return "Difficult by learning progress";
    }

    private static void EnsureKeycloakUserId(
        string keycloakUserId)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException(
                "KeycloakUserId boş olamaz.",
                nameof(keycloakUserId));
        }
    }

    private sealed class DifficultLearningItemQueryRow
    {
        public Guid UserLearningItemId { get; init; }

        public Guid LearningItemId { get; init; }

        public LearningItemType ItemType { get; init; }

        public LearningStatus LearningStatus { get; init; }

        public int ConfidenceScore { get; init; }

        public int CorrectCount { get; init; }

        public int WrongCount { get; init; }

        public int ConsecutiveWrongCount { get; init; }

        public int RepetitionLevel { get; init; }

        public bool IsManuallyMarkedDifficult { get; init; }

        public bool IsProgressDifficult { get; init; }

        public DateTime SavedAtUtc { get; init; }

        public DateTime? LastReviewedAtUtc { get; init; }

        public DateTime? NextReviewDateUtc { get; init; }
    }

    private sealed class LearningItemDetails
    {
        public Guid LearningItemId { get; init; }

        public string DisplayText { get; init; } = string.Empty;

        public string? PrimaryMeaning { get; init; }
    }
}