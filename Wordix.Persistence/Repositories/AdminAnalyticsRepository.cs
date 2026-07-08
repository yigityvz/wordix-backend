using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.AdminAnalytics.Models;
using Wordix.Domain.Enums;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// Admin analytics endpointleri için gerekli aggregate sorguların EF Core implementasyonudur.
/// 
/// Bu repository neden var?
/// - Admin analytics sorguları normal CRUD sorguları değildir.
/// - Büyük tablolarda Count, GroupBy, Distinct, Average, Max gibi aggregate işlemler gerekir.
/// - Bu işlemler C# memory tarafında değil, SQL tarafında çalışmalıdır.
/// - Application katmanı DbContext bilmemelidir.
/// 
/// Bu sınıf Persistence katmanındadır.
/// Çünkü EF Core, DbContext ve SQL sorgu detayları Persistence sorumluluğudur.
/// </summary>
public sealed class AdminAnalyticsRepository : IAdminAnalyticsRepository
{
    private readonly WordixDbContext _dbContext;

    public AdminAnalyticsRepository(WordixDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Admin dashboard için sistem geneli özet metrikleri döndürür.
    /// 
    /// Burada her metrik ilgili tablodan SQL COUNT/SUM/AVG olarak hesaplanır.
    /// Büyük tabloları komple memory'ye çekmeyiz.
    /// </summary>
    public async Task<AdminDashboardAnalyticsModel> GetDashboardAsync(
        AdminAnalyticsDateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        var lookupQuery = ApplyCreatedAtDateRange(
            _dbContext.LookupHistories.AsNoTracking(),
            dateRange);

        var dictionaryQuery = ApplySavedAtDateRange(
            _dbContext.UserLearningItems.AsNoTracking(),
            dateRange);

        var quizSessionQuery = ApplyQuizStartedAtDateRange(
            _dbContext.QuizSessions.AsNoTracking(),
            dateRange);

        var quizAnswerQuery = ApplyAnsweredAtDateRange(
            _dbContext.QuizAnswers.AsNoTracking(),
            dateRange);

        var providerRequestQuery = ApplyCreatedAtDateRange(
            _dbContext.ProviderRequestLogs.AsNoTracking(),
            dateRange);

        var externalCacheQuery = ApplyCreatedAtDateRange(
            _dbContext.ExternalContentCaches.AsNoTracking(),
            dateRange);

        var importJobQuery = ApplyCreatedAtDateRange(
            _dbContext.ImportJobs.AsNoTracking(),
            dateRange);

        var quizQuestionQuery = _dbContext.QuizQuestions.AsNoTracking();

        var lookupCount = await lookupQuery.CountAsync(cancellationToken);

        var uniqueLookupUserCount = await lookupQuery
            .Select(lookup => lookup.KeycloakUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var databaseLookupCount = await lookupQuery
            .CountAsync(lookup => lookup.WasFoundInDatabase, cancellationToken);

        var providerLookupCount = await lookupQuery
            .CountAsync(lookup => lookup.ProviderType != null, cancellationToken);

        var providerCreatedLookupCount = await lookupQuery
            .CountAsync(lookup => lookup.WasCreatedFromProvider, cancellationToken);

        var dictionarySaveCount = await dictionaryQuery.CountAsync(cancellationToken);

        var activeDictionarySaveCount = await dictionaryQuery
            .CountAsync(item => item.IsActive, cancellationToken);

        var uniqueDictionaryUserCount = await dictionaryQuery
            .Select(item => item.KeycloakUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var quizSessionCount = await quizSessionQuery.CountAsync(cancellationToken);

        var quizAnswerCount = await quizAnswerQuery.CountAsync(cancellationToken);

        var correctAnswerCount = await quizAnswerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.Correct, cancellationToken);

        var wrongAnswerCount = await quizAnswerQuery
            .CountAsync(answer => answer.AnswerResult == AnswerResult.Incorrect, cancellationToken);

        var averageAccuracyRate = quizAnswerCount == 0
            ? 0
            : correctAnswerCount * 100.0 / quizAnswerCount;

        var systemRecommendedQuestionCount = await quizQuestionQuery
            .CountAsync(question => question.IsSystemRecommended, cancellationToken);

        var systemRecommendedWrongAnswerCount = await (
            from answer in quizAnswerQuery
            join question in quizQuestionQuery
                on answer.QuizQuestionId equals question.Id
            where question.IsSystemRecommended &&
                  answer.AnswerResult == AnswerResult.Incorrect
            select answer.Id)
            .CountAsync(cancellationToken);

        var providerRequestCount = await providerRequestQuery.CountAsync(cancellationToken);

        var providerSuccessCount = await providerRequestQuery
            .CountAsync(log => log.Status == ProviderRequestStatus.Succeeded, cancellationToken);

        var providerFailureCount = await providerRequestQuery
            .CountAsync(log => log.Status == ProviderRequestStatus.Failed, cancellationToken);

        var providerTimeoutCount = await providerRequestQuery
            .CountAsync(log => log.Status == ProviderRequestStatus.Timeout, cancellationToken);

        var providerRateLimitedCount = await providerRequestQuery
            .CountAsync(log => log.Status == ProviderRequestStatus.RateLimited, cancellationToken);

        var providerServedFromCacheCount = await providerRequestQuery
            .CountAsync(log => log.Status == ProviderRequestStatus.ServedFromCache ||
                               log.WasServedFromCache, cancellationToken);

        var averageProviderDurationMs = await providerRequestQuery
            .Where(log => log.DurationMs.HasValue)
            .Select(log => (double?)log.DurationMs!.Value)
            .AverageAsync(cancellationToken) ?? 0;

        var externalCacheEntryCount = await externalCacheQuery.CountAsync(cancellationToken);

        var activeExternalCacheEntryCount = await externalCacheQuery
            .CountAsync(cache => cache.Status == ExternalContentCacheStatus.Active, cancellationToken);

        var externalCacheHitCount = await externalCacheQuery
            .SumAsync(cache => cache.HitCount, cancellationToken);

        var importJobCount = await importJobQuery.CountAsync(cancellationToken);

        var completedImportJobCount = await importJobQuery
            .CountAsync(job => job.Status == ImportJobStatus.Completed, cancellationToken);

        var failedImportJobCount = await importJobQuery
            .CountAsync(job => job.Status == ImportJobStatus.Failed, cancellationToken);

        var runningImportJobCount = await importJobQuery
            .CountAsync(job => job.Status == ImportJobStatus.Running, cancellationToken);

        var lastLookupAtUtc = await lookupQuery
            .Select(lookup => (DateTime?)lookup.CreatedAt)
            .MaxAsync(cancellationToken);

        var lastDictionarySaveAtUtc = await dictionaryQuery
            .Select(item => (DateTime?)item.SavedAt)
            .MaxAsync(cancellationToken);

        var lastQuizStartedAtUtc = await quizSessionQuery
            .Select(session => (DateTime?)session.StartedAt)
            .MaxAsync(cancellationToken);

        var lastProviderRequestAtUtc = await providerRequestQuery
            .Select(log => (DateTime?)log.CreatedAt)
            .MaxAsync(cancellationToken);

        var lastImportJobAtUtc = await importJobQuery
            .Select(job => (DateTime?)job.CreatedAt)
            .MaxAsync(cancellationToken);

        return new AdminDashboardAnalyticsModel
        {
            LookupCount = lookupCount,
            UniqueLookupUserCount = uniqueLookupUserCount,
            DatabaseLookupCount = databaseLookupCount,
            ProviderLookupCount = providerLookupCount,
            ProviderCreatedLookupCount = providerCreatedLookupCount,

            DictionarySaveCount = dictionarySaveCount,
            ActiveDictionarySaveCount = activeDictionarySaveCount,
            UniqueDictionaryUserCount = uniqueDictionaryUserCount,

            QuizSessionCount = quizSessionCount,
            QuizAnswerCount = quizAnswerCount,
            CorrectAnswerCount = correctAnswerCount,
            WrongAnswerCount = wrongAnswerCount,
            AverageAccuracyRate = averageAccuracyRate,

            SystemRecommendedQuestionCount = systemRecommendedQuestionCount,
            SystemRecommendedWrongAnswerCount = systemRecommendedWrongAnswerCount,

            ProviderRequestCount = providerRequestCount,
            ProviderSuccessCount = providerSuccessCount,
            ProviderFailureCount = providerFailureCount,
            ProviderTimeoutCount = providerTimeoutCount,
            ProviderRateLimitedCount = providerRateLimitedCount,
            ProviderServedFromCacheCount = providerServedFromCacheCount,
            AverageProviderDurationMs = averageProviderDurationMs,

            ExternalCacheEntryCount = externalCacheEntryCount,
            ActiveExternalCacheEntryCount = activeExternalCacheEntryCount,
            ExternalCacheHitCount = externalCacheHitCount,

            ImportJobCount = importJobCount,
            CompletedImportJobCount = completedImportJobCount,
            FailedImportJobCount = failedImportJobCount,
            RunningImportJobCount = runningImportJobCount,

            LastLookupAtUtc = lastLookupAtUtc,
            LastDictionarySaveAtUtc = lastDictionarySaveAtUtc,
            LastQuizStartedAtUtc = lastQuizStartedAtUtc,
            LastProviderRequestAtUtc = lastProviderRequestAtUtc,
            LastImportJobAtUtc = lastImportJobAtUtc,

            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// En çok aranan normalize query kayıtlarını döndürür.
    /// 
    /// Ana aggregate sorgu SQL tarafında çalışır.
    /// Sonrasında sadece limit kadar query için son görülen örnek QueryText ve LearningItemId bilgisi alınır.
    /// </summary>
    public async Task<IReadOnlyCollection<TopSearchedItemAnalyticsModel>> GetTopSearchesAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        var lookupQuery = ApplyCreatedAtDateRange(
            _dbContext.LookupHistories.AsNoTracking(),
            dateRange);

        var groupedRows = await lookupQuery
            .GroupBy(lookup => new
            {
                lookup.NormalizedQueryText,
                lookup.InputType
            })
            .Select(group => new
            {
                group.Key.NormalizedQueryText,
                group.Key.InputType,
                SearchCount = group.Count(),
                UniqueUserCount = group
                    .Select(lookup => lookup.KeycloakUserId)
                    .Distinct()
                    .Count(),
                DatabaseHitCount = group.Count(lookup => lookup.WasFoundInDatabase),
                ProviderUsageCount = group.Count(lookup => lookup.ProviderType != null),
                ProviderCreatedCount = group.Count(lookup => lookup.WasCreatedFromProvider),
                NotFoundCount = group.Count(lookup => lookup.ResultCount == 0),
                LastSearchedAtUtc = group.Max(lookup => lookup.CreatedAt)
            })
            .OrderByDescending(row => row.SearchCount)
            .ThenBy(row => row.NormalizedQueryText)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (groupedRows.Count == 0)
        {
            return Array.Empty<TopSearchedItemAnalyticsModel>();
        }

        var normalizedTexts = groupedRows
            .Select(row => row.NormalizedQueryText)
            .Distinct()
            .ToArray();

        var latestLookupRows = await lookupQuery
            .Where(lookup => normalizedTexts.Contains(lookup.NormalizedQueryText))
            .Select(lookup => new
            {
                lookup.QueryText,
                lookup.NormalizedQueryText,
                lookup.InputType,
                lookup.LearningItemId,
                lookup.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var latestLookupByKey = latestLookupRows
            .GroupBy(row => BuildTopSearchKey(row.NormalizedQueryText, row.InputType))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.CreatedAt)
                    .First());

        return groupedRows
            .Select(row =>
            {
                var key = BuildTopSearchKey(row.NormalizedQueryText, row.InputType);
                latestLookupByKey.TryGetValue(key, out var latestLookup);

                return new TopSearchedItemAnalyticsModel
                {
                    QueryText = latestLookup?.QueryText ?? row.NormalizedQueryText,
                    NormalizedQueryText = row.NormalizedQueryText,
                    InputType = row.InputType,

                    SearchCount = row.SearchCount,
                    UniqueUserCount = row.UniqueUserCount,
                    DatabaseHitCount = row.DatabaseHitCount,
                    ProviderUsageCount = row.ProviderUsageCount,
                    ProviderCreatedCount = row.ProviderCreatedCount,
                    NotFoundCount = row.NotFoundCount,

                    LearningItemId = latestLookup?.LearningItemId,
                    LastSearchedAtUtc = row.LastSearchedAtUtc
                };
            })
            .ToArray();
    }

    /// <summary>
    /// En çok dictionary'ye kaydedilen LearningItem kayıtlarını döndürür.
    /// 
    /// Önce UserLearningItems tablosunda aggregate alınır.
    /// Sonra sadece top N LearningItem için Word/Phrase/Sentence/Meaning detayları çekilir.
    /// Böylece tüm catalog memory'ye alınmaz.
    /// </summary>
    public async Task<IReadOnlyCollection<TopSavedLearningItemAnalyticsModel>> GetTopSavedLearningItemsAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        var dictionaryQuery = ApplySavedAtDateRange(
            _dbContext.UserLearningItems.AsNoTracking(),
            dateRange);

        var aggregateRows = await dictionaryQuery
            .GroupBy(item => item.LearningItemId)
            .Select(group => new
            {
                LearningItemId = group.Key,
                SaveCount = group.Count(),
                ActiveSaveCount = group.Count(item => item.IsActive),
                UniqueUserCount = group
                    .Select(item => item.KeycloakUserId)
                    .Distinct()
                    .Count(),
                LastSavedAtUtc = group.Max(item => item.SavedAt)
            })
            .OrderByDescending(row => row.SaveCount)
            .ThenBy(row => row.LearningItemId)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (aggregateRows.Count == 0)
        {
            return Array.Empty<TopSavedLearningItemAnalyticsModel>();
        }

        var learningItemIds = aggregateRows
            .Select(row => row.LearningItemId)
            .ToArray();

        var details = await GetLearningItemDetailsAsync(
            learningItemIds,
            cancellationToken);

        return aggregateRows
            .Where(row => details.ContainsKey(row.LearningItemId))
            .Select(row =>
            {
                var detail = details[row.LearningItemId];

                return new TopSavedLearningItemAnalyticsModel
                {
                    LearningItemId = row.LearningItemId,
                    ItemType = detail.ItemType,
                    DisplayText = detail.DisplayText,
                    PrimaryMeaning = detail.PrimaryMeaning,

                    SaveCount = row.SaveCount,
                    ActiveSaveCount = row.ActiveSaveCount,
                    UniqueUserCount = row.UniqueUserCount,

                    ContentSource = detail.ContentSource,
                    QualityStatus = detail.QualityStatus,
                    CefrLevel = detail.CefrLevel,
                    DifficultyGroup = detail.DifficultyGroup,

                    LastSavedAtUtc = row.LastSavedAtUtc
                };
            })
            .ToArray();
    }

    /// <summary>
    /// Quizlerde en çok yanlış yapılan LearningItem kayıtlarını döndürür.
    /// 
    /// Yanlış cevap hesabı QuizAnswer.AnswerResult üzerinden yapılır.
    /// LearningItem bilgisi QuizQuestion üzerinden gelir.
    /// </summary>
    public async Task<IReadOnlyCollection<MostWrongLearningItemAnalyticsModel>> GetMostWrongLearningItemsAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        var quizAnswerQuery = ApplyAnsweredAtDateRange(
            _dbContext.QuizAnswers.AsNoTracking(),
            dateRange);

        var quizQuestionQuery = _dbContext.QuizQuestions.AsNoTracking();

        var answerQuestionQuery =
            from answer in quizAnswerQuery
            join question in quizQuestionQuery
                on answer.QuizQuestionId equals question.Id
            select new
            {
                question.LearningItemId,
                question.IsSystemRecommended,
                answer.AnswerResult,
                answer.ResponseTimeMilliseconds,
                answer.AnsweredAt
            };

        var aggregateRows = await answerQuestionQuery
            .GroupBy(row => row.LearningItemId)
            .Select(group => new
            {
                LearningItemId = group.Key,
                WrongAnswerCount = group.Count(row => row.AnswerResult == AnswerResult.Incorrect),
                CorrectAnswerCount = group.Count(row => row.AnswerResult == AnswerResult.Correct),
                TotalAnswerCount = group.Count(),
                AverageResponseTimeInMilliseconds = group.Average(row => (double)row.ResponseTimeMilliseconds),
                SystemRecommendedWrongAnswerCount = group.Count(row =>
                    row.IsSystemRecommended &&
                    row.AnswerResult == AnswerResult.Incorrect),
                LastWrongAtUtc = group
                    .Where(row => row.AnswerResult == AnswerResult.Incorrect)
                    .Max(row => (DateTime?)row.AnsweredAt)
            })
            .Where(row => row.WrongAnswerCount > 0)
            .OrderByDescending(row => row.WrongAnswerCount)
            .ThenByDescending(row => row.TotalAnswerCount)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (aggregateRows.Count == 0)
        {
            return Array.Empty<MostWrongLearningItemAnalyticsModel>();
        }

        var learningItemIds = aggregateRows
            .Select(row => row.LearningItemId)
            .ToArray();

        var details = await GetLearningItemDetailsAsync(
            learningItemIds,
            cancellationToken);

        return aggregateRows
            .Where(row => details.ContainsKey(row.LearningItemId))
            .Select(row =>
            {
                var detail = details[row.LearningItemId];

                var wrongRate = row.TotalAnswerCount == 0
                    ? 0
                    : row.WrongAnswerCount * 100.0 / row.TotalAnswerCount;

                return new MostWrongLearningItemAnalyticsModel
                {
                    LearningItemId = row.LearningItemId,
                    ItemType = detail.ItemType,
                    DisplayText = detail.DisplayText,

                    WrongAnswerCount = row.WrongAnswerCount,
                    CorrectAnswerCount = row.CorrectAnswerCount,
                    TotalAnswerCount = row.TotalAnswerCount,
                    WrongRate = wrongRate,
                    AverageResponseTimeInMilliseconds = row.AverageResponseTimeInMilliseconds,
                    SystemRecommendedWrongAnswerCount = row.SystemRecommendedWrongAnswerCount,
                    LastWrongAtUtc = row.LastWrongAtUtc ?? DateTime.UtcNow
                };
            })
            .ToArray();
    }

    /// <summary>
    /// Provider + operation bazlı provider/cache/import istatistiklerini döndürür.
    /// 
    /// ProviderRequestLogs ana kaynak olarak kullanılır.
    /// ExternalContentCaches ve ImportJobs aggregate sonuçları aynı provider/source ismine göre birleştirilir.
    /// </summary>
    public async Task<IReadOnlyCollection<ProviderStatAnalyticsModel>> GetProviderStatsAsync(
        AdminAnalyticsDateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        var providerRequestQuery = ApplyCreatedAtDateRange(
            _dbContext.ProviderRequestLogs.AsNoTracking(),
            dateRange);

        var externalCacheQuery = ApplyCreatedAtDateRange(
            _dbContext.ExternalContentCaches.AsNoTracking(),
            dateRange);

        var importJobQuery = ApplyCreatedAtDateRange(
            _dbContext.ImportJobs.AsNoTracking(),
            dateRange);

        var providerRows = await providerRequestQuery
            .GroupBy(log => new
            {
                log.ProviderName,
                log.ProviderType,
                log.OperationName
            })
            .Select(group => new
            {
                group.Key.ProviderName,
                group.Key.ProviderType,
                group.Key.OperationName,
                TotalRequestCount = group.Count(),
                SucceededCount = group.Count(log => log.Status == ProviderRequestStatus.Succeeded),
                FailedCount = group.Count(log => log.Status == ProviderRequestStatus.Failed),
                TimeoutCount = group.Count(log => log.Status == ProviderRequestStatus.Timeout),
                RateLimitedCount = group.Count(log => log.Status == ProviderRequestStatus.RateLimited),
                ServedFromCacheCount = group.Count(log =>
                    log.Status == ProviderRequestStatus.ServedFromCache ||
                    log.WasServedFromCache),
                AverageDurationMs = group
                    .Where(log => log.DurationMs.HasValue)
                    .Select(log => (double?)log.DurationMs!.Value)
                    .Average(),
                LastRequestAtUtc = group.Max(log => log.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        var cacheRows = await externalCacheQuery
            .GroupBy(cache => new
            {
                cache.ProviderName,
                cache.ProviderType,
                cache.OperationName
            })
            .Select(group => new
            {
                group.Key.ProviderName,
                group.Key.ProviderType,
                group.Key.OperationName,
                CacheEntryCount = group.Count(),
                CacheHitCount = group.Sum(cache => cache.HitCount)
            })
            .ToListAsync(cancellationToken);

        var importRows = await importJobQuery
            .GroupBy(job => new
            {
                ProviderName = job.SourceName,
                OperationName = job.JobType.ToString()
            })
            .Select(group => new
            {
                group.Key.ProviderName,
                group.Key.OperationName,
                ImportJobCount = group.Count(),
                FailedImportJobCount = group.Count(job => job.Status == ImportJobStatus.Failed)
            })
            .ToListAsync(cancellationToken);

        var allKeys = new HashSet<string>();

        foreach (var row in providerRows)
        {
            allKeys.Add(BuildProviderStatsKey(
                row.ProviderName,
                row.ProviderType,
                row.OperationName));
        }

        foreach (var row in cacheRows)
        {
            allKeys.Add(BuildProviderStatsKey(
                row.ProviderName,
                row.ProviderType,
                row.OperationName));
        }

        foreach (var row in importRows)
        {
            allKeys.Add(BuildProviderStatsKey(
                row.ProviderName,
                ProviderType.Import,
                row.OperationName));
        }

        var cacheByKey = cacheRows.ToDictionary(
            row => BuildProviderStatsKey(row.ProviderName, row.ProviderType, row.OperationName),
            row => row);

        var importByKey = importRows.ToDictionary(
            row => BuildProviderStatsKey(row.ProviderName, ProviderType.Import, row.OperationName),
            row => row);

        var providerByKey = providerRows.ToDictionary(
            row => BuildProviderStatsKey(row.ProviderName, row.ProviderType, row.OperationName),
            row => row);

        return allKeys
            .Select(key =>
            {
                providerByKey.TryGetValue(key, out var provider);
                cacheByKey.TryGetValue(key, out var cache);
                importByKey.TryGetValue(key, out var import);

                return new ProviderStatAnalyticsModel
                {
                    ProviderName = provider?.ProviderName
                                   ?? cache?.ProviderName
                                   ?? import?.ProviderName
                                   ?? string.Empty,

                    ProviderType = provider?.ProviderType
                                   ?? cache?.ProviderType
                                   ?? ProviderType.Import,

                    OperationName = provider?.OperationName
                                    ?? cache?.OperationName
                                    ?? import?.OperationName
                                    ?? string.Empty,

                    TotalRequestCount = provider?.TotalRequestCount ?? 0,
                    SucceededCount = provider?.SucceededCount ?? 0,
                    FailedCount = provider?.FailedCount ?? 0,
                    TimeoutCount = provider?.TimeoutCount ?? 0,
                    RateLimitedCount = provider?.RateLimitedCount ?? 0,
                    ServedFromCacheCount = provider?.ServedFromCacheCount ?? 0,
                    AverageDurationMs = provider?.AverageDurationMs ?? 0,
                    CacheEntryCount = cache?.CacheEntryCount ?? 0,
                    CacheHitCount = cache?.CacheHitCount ?? 0,
                    ImportJobCount = import?.ImportJobCount ?? 0,
                    FailedImportJobCount = import?.FailedImportJobCount ?? 0,
                    LastRequestAtUtc = provider?.LastRequestAtUtc
                };
            })
            .OrderByDescending(row => row.TotalRequestCount)
            .ThenByDescending(row => row.ImportJobCount)
            .ThenBy(row => row.ProviderName)
            .ThenBy(row => row.OperationName)
            .ToArray();
    }

    /// <summary>
    /// LearningItem detaylarını getirir.
    /// 
    /// Bu helper neden var?
    /// - TopSaved ve MostWrong endpointleri önce aggregate olarak LearningItemId üretir.
    /// - Sonra sadece bu top N item için display text, meaning ve metadata gerekir.
    /// - Böylece tüm Word/Phrase/Sentence catalog memory'ye çekilmez.
    /// </summary>
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
                item.ItemType,
                item.ContentSource,
                item.QualityStatus,
                item.CefrLevel,
                item.DifficultyGroup
            })
            .ToListAsync(cancellationToken);

        var wordTexts = await _dbContext.Words
            .AsNoTracking()
            .Where(word => ids.Contains(word.LearningItemId))
            .Select(word => new
            {
                word.LearningItemId,
                word.Text
            })
            .ToDictionaryAsync(
                word => word.LearningItemId,
                word => word.Text,
                cancellationToken);

        var phraseTexts = await _dbContext.Phrases
            .AsNoTracking()
            .Where(phrase => ids.Contains(phrase.LearningItemId))
            .Select(phrase => new
            {
                phrase.LearningItemId,
                phrase.Text
            })
            .ToDictionaryAsync(
                phrase => phrase.LearningItemId,
                phrase => phrase.Text,
                cancellationToken);

        var sentenceTexts = await _dbContext.Sentences
            .AsNoTracking()
            .Where(sentence => sentence.LearningItemId.HasValue &&
                               ids.Contains(sentence.LearningItemId.Value))
            .Select(sentence => new
            {
                LearningItemId = sentence.LearningItemId!.Value,
                sentence.Text
            })
            .ToDictionaryAsync(
                sentence => sentence.LearningItemId,
                sentence => sentence.Text,
                cancellationToken);

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
                    wordTexts,
                    phraseTexts,
                    sentenceTexts);

                primaryMeaningByLearningItemId.TryGetValue(
                    item.Id,
                    out var primaryMeaning);

                return new LearningItemDetails
                {
                    LearningItemId = item.Id,
                    ItemType = item.ItemType,
                    DisplayText = displayText,
                    PrimaryMeaning = primaryMeaning,
                    ContentSource = item.ContentSource,
                    QualityStatus = item.QualityStatus,
                    CefrLevel = item.CefrLevel,
                    DifficultyGroup = item.DifficultyGroup
                };
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.DisplayText))
            .ToDictionary(
                item => item.LearningItemId,
                item => item);
    }

    /// <summary>
    /// LearningItem tipine göre gösterilecek metni çözer.
    /// 
    /// Word için Words.Text,
    /// Phrase için Phrases.Text,
    /// Sentence için Sentences.Text kullanılır.
    /// </summary>
    private static string ResolveDisplayText(
        Guid learningItemId,
        LearningItemType itemType,
        IReadOnlyDictionary<Guid, string> wordTexts,
        IReadOnlyDictionary<Guid, string> phraseTexts,
        IReadOnlyDictionary<Guid, string> sentenceTexts)
    {
        return itemType switch
        {
            LearningItemType.Word => wordTexts.GetValueOrDefault(learningItemId) ?? string.Empty,
            LearningItemType.Phrase => phraseTexts.GetValueOrDefault(learningItemId) ?? string.Empty,
            LearningItemType.Sentence => sentenceTexts.GetValueOrDefault(learningItemId) ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// CreatedAt alanı olan AuditableEntity tabanlı sorgulara tarih filtresi uygular.
    /// 
    /// Bu helper LookupHistory, ProviderRequestLog, ImportJob, ExternalContentCache gibi
    /// CreatedAt üzerinden raporlanacak tablolarda kullanılır.
    /// </summary>
    private static IQueryable<TEntity> ApplyCreatedAtDateRange<TEntity>(
        IQueryable<TEntity> query,
        AdminAnalyticsDateRange dateRange)
        where TEntity : Domain.Common.AuditableEntity
    {
        if (dateRange.FromUtc.HasValue)
        {
            query = query.Where(entity => entity.CreatedAt >= dateRange.FromUtc.Value);
        }

        if (dateRange.ToUtc.HasValue)
        {
            query = query.Where(entity => entity.CreatedAt <= dateRange.ToUtc.Value);
        }

        return query;
    }

    /// <summary>
    /// UserLearningItem.SavedAt alanına tarih filtresi uygular.
    /// 
    /// Dictionary save analytics için CreatedAt yerine SavedAt kullanmak daha doğru.
    /// Çünkü iş anlamı "kullanıcı ne zaman kaydetti?" bilgisidir.
    /// </summary>
    private static IQueryable<Domain.Entities.UserLearningItem> ApplySavedAtDateRange(
        IQueryable<Domain.Entities.UserLearningItem> query,
        AdminAnalyticsDateRange dateRange)
    {
        if (dateRange.FromUtc.HasValue)
        {
            query = query.Where(item => item.SavedAt >= dateRange.FromUtc.Value);
        }

        if (dateRange.ToUtc.HasValue)
        {
            query = query.Where(item => item.SavedAt <= dateRange.ToUtc.Value);
        }

        return query;
    }

    /// <summary>
    /// QuizSession.StartedAt alanına tarih filtresi uygular.
    /// </summary>
    private static IQueryable<Domain.Entities.QuizSession> ApplyQuizStartedAtDateRange(
        IQueryable<Domain.Entities.QuizSession> query,
        AdminAnalyticsDateRange dateRange)
    {
        if (dateRange.FromUtc.HasValue)
        {
            query = query.Where(session => session.StartedAt >= dateRange.FromUtc.Value);
        }

        if (dateRange.ToUtc.HasValue)
        {
            query = query.Where(session => session.StartedAt <= dateRange.ToUtc.Value);
        }

        return query;
    }

    /// <summary>
    /// QuizAnswer.AnsweredAt alanına tarih filtresi uygular.
    /// </summary>
    private static IQueryable<Domain.Entities.QuizAnswer> ApplyAnsweredAtDateRange(
        IQueryable<Domain.Entities.QuizAnswer> query,
        AdminAnalyticsDateRange dateRange)
    {
        if (dateRange.FromUtc.HasValue)
        {
            query = query.Where(answer => answer.AnsweredAt >= dateRange.FromUtc.Value);
        }

        if (dateRange.ToUtc.HasValue)
        {
            query = query.Where(answer => answer.AnsweredAt <= dateRange.ToUtc.Value);
        }

        return query;
    }

    private static string BuildTopSearchKey(
        string normalizedQueryText,
        InputType inputType)
    {
        return $"{(int)inputType}::{normalizedQueryText}";
    }

    private static string BuildProviderStatsKey(
        string providerName,
        ProviderType providerType,
        string operationName)
    {
        return $"{providerName.Trim().ToLowerInvariant()}::{(int)providerType}::{operationName.Trim().ToLowerInvariant()}";
    }

    /// <summary>
    /// Top saved / most wrong gibi endpointlerde kullanılacak LearningItem detay modelidir.
    /// 
    /// Bu model sadece repository içinde kullanılır.
    /// Application katmanına açık değildir.
    /// </summary>
    private sealed class LearningItemDetails
    {
        public Guid LearningItemId { get; init; }

        public LearningItemType ItemType { get; init; }

        public string DisplayText { get; init; } = string.Empty;

        public string? PrimaryMeaning { get; init; }

        public ContentSource ContentSource { get; init; }

        public ContentQualityStatus QualityStatus { get; init; }

        public CefrLevel CefrLevel { get; init; }

        public DifficultyGroup DifficultyGroup { get; init; }
    }
}