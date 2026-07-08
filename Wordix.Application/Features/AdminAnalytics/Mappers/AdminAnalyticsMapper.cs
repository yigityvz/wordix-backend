using Wordix.Application.Features.AdminAnalytics.Dtos.Responses;
using Wordix.Application.Features.AdminAnalytics.Models;
using Wordix.Application.Features.AdminAnalytics.Dtos.Requests;
using Wordix.Application.Features.AdminAnalytics.Queries.GetAdminDashboard;
using Wordix.Application.Features.AdminAnalytics.Queries.GetMostWrongLearningItems;
using Wordix.Application.Features.AdminAnalytics.Queries.GetProviderStats;
using Wordix.Application.Features.AdminAnalytics.Queries.GetTopSavedLearningItems;
using Wordix.Application.Features.AdminAnalytics.Queries.GetTopSearches;

namespace Wordix.Application.Features.AdminAnalytics.Mappers;

/// <summary>
/// AdminAnalytics feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu mapper neden var?
/// - Handler içinde response DTO propertylerini tek tek dizmek istemiyoruz.
/// - Controller zaten sadece HTTP + MediatR akışını yönetmeli.
/// - Repository'den gelen internal analytics modellerini API response contract'larına burada dönüştürüyoruz.
/// - AutoMapper/Mapster kullanmadan, açık ve okunabilir mapping yapıyoruz.
/// 
/// Bu yapı Faz 17 sonrası kalıcı mapper standardımıza uygundur:
/// - DTO'lar feature altında durur.
/// - Mapping feature altında explicit mapper ile yapılır.
/// - Handler use-case akışına odaklanır.
/// </summary>
public static class AdminAnalyticsMapper
{
    /// <summary>
    /// Dashboard internal modelini API response modeline dönüştürür.
    /// 
    /// Dashboard response tekil bir özet modeldir.
    /// Bu yüzden date range parametresi almaz; repository zaten dashboard için
    /// ilgili aggregate değerleri model içinde hesaplamış olur.
    /// </summary>
    public static AdminDashboardAnalyticsResponse ToDashboardResponse(
        AdminDashboardAnalyticsModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AdminDashboardAnalyticsResponse
        {
            LookupCount = model.LookupCount,
            UniqueLookupUserCount = model.UniqueLookupUserCount,
            DatabaseLookupCount = model.DatabaseLookupCount,
            ProviderLookupCount = model.ProviderLookupCount,
            ProviderCreatedLookupCount = model.ProviderCreatedLookupCount,

            DictionarySaveCount = model.DictionarySaveCount,
            ActiveDictionarySaveCount = model.ActiveDictionarySaveCount,
            UniqueDictionaryUserCount = model.UniqueDictionaryUserCount,

            QuizSessionCount = model.QuizSessionCount,
            QuizAnswerCount = model.QuizAnswerCount,
            CorrectAnswerCount = model.CorrectAnswerCount,
            WrongAnswerCount = model.WrongAnswerCount,
            AverageAccuracyRate = RoundRate(model.AverageAccuracyRate),

            SystemRecommendedQuestionCount = model.SystemRecommendedQuestionCount,
            SystemRecommendedWrongAnswerCount = model.SystemRecommendedWrongAnswerCount,

            ProviderRequestCount = model.ProviderRequestCount,
            ProviderSuccessCount = model.ProviderSuccessCount,
            ProviderFailureCount = model.ProviderFailureCount,
            ProviderTimeoutCount = model.ProviderTimeoutCount,
            ProviderRateLimitedCount = model.ProviderRateLimitedCount,
            ProviderServedFromCacheCount = model.ProviderServedFromCacheCount,
            AverageProviderDurationMs = RoundMetric(model.AverageProviderDurationMs),

            ExternalCacheEntryCount = model.ExternalCacheEntryCount,
            ActiveExternalCacheEntryCount = model.ActiveExternalCacheEntryCount,
            ExternalCacheHitCount = model.ExternalCacheHitCount,

            ImportJobCount = model.ImportJobCount,
            CompletedImportJobCount = model.CompletedImportJobCount,
            FailedImportJobCount = model.FailedImportJobCount,
            RunningImportJobCount = model.RunningImportJobCount,

            LastLookupAt = ToDateTimeOffset(model.LastLookupAtUtc),
            LastDictionarySaveAt = ToDateTimeOffset(model.LastDictionarySaveAtUtc),
            LastQuizStartedAt = ToDateTimeOffset(model.LastQuizStartedAtUtc),
            LastProviderRequestAt = ToDateTimeOffset(model.LastProviderRequestAtUtc),
            LastImportJobAt = ToDateTimeOffset(model.LastImportJobAtUtc),

            GeneratedAt = ToRequiredDateTimeOffset(model.GeneratedAtUtc)
        };
    }


    /// <summary>
    /// Admin dashboard request DTO'sunu MediatR query modeline dönüştürür.
    /// 
    /// Controller neden direkt query oluşturmaz?
    /// - Controller'ın görevi HTTP request almak ve MediatR'a iletmektir.
    /// - Request DTO -> Query mapping işi feature mapper içinde merkezi durmalıdır.
    /// - Böylece controller sade kalır ve mapping standardımız korunur.
    /// </summary>
    public static GetAdminDashboardQuery ToGetAdminDashboardQuery(
        AdminAnalyticsDateRangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetAdminDashboardQuery(
            request.FromUtc,
            request.ToUtc);
    }

    /// <summary>
    /// Top searches request DTO'sunu MediatR query modeline dönüştürür.
    /// 
    /// Bu endpoint:
    /// - LookupHistory tablosundan en çok aranan queryleri analiz eder.
    /// - Limit boşsa handler default limit kullanır.
    /// - Tarih aralığı boşsa handler default son 30 gün kullanır.
    /// </summary>
    public static GetTopSearchesQuery ToGetTopSearchesQuery(
        AdminAnalyticsListRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetTopSearchesQuery(
            request.FromUtc,
            request.ToUtc,
            request.Limit);
    }

    /// <summary>
    /// Top saved request DTO'sunu MediatR query modeline dönüştürür.
    /// 
    /// Bu endpoint:
    /// - UserLearningItems tablosundan en çok kaydedilen LearningItem kayıtlarını analiz eder.
    /// - Word/Phrase/Sentence ayrımını LearningItem üzerinden destekler.
    /// </summary>
    public static GetTopSavedLearningItemsQuery ToGetTopSavedLearningItemsQuery(
        AdminAnalyticsListRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetTopSavedLearningItemsQuery(
            request.FromUtc,
            request.ToUtc,
            request.Limit);
    }

    /// <summary>
    /// Most wrong request DTO'sunu MediatR query modeline dönüştürür.
    /// 
    /// Bu endpoint:
    /// - QuizAnswer + QuizQuestion üzerinden en çok yanlış yapılan LearningItem kayıtlarını analiz eder.
    /// - Yanlış oranı, toplam cevap sayısı ve ortalama cevap süresi gibi metrikleri döndürür.
    /// </summary>
    public static GetMostWrongLearningItemsQuery ToGetMostWrongLearningItemsQuery(
        AdminAnalyticsListRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetMostWrongLearningItemsQuery(
            request.FromUtc,
            request.ToUtc,
            request.Limit);
    }

    /// <summary>
    /// Provider stats request DTO'sunu MediatR query modeline dönüştürür.
    /// 
    /// Bu endpoint:
    /// - ProviderRequestLogs
    /// - ExternalContentCaches
    /// - ImportJobs
    /// verilerinden provider/cache/import istatistikleri üretir.
    /// </summary>
    public static GetProviderStatsQuery ToGetProviderStatsQuery(
        AdminAnalyticsDateRangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetProviderStatsQuery(
            request.FromUtc,
            request.ToUtc);
    }

    /// <summary>
    /// Top searches analytics sonucunu API response modeline dönüştürür.
    /// </summary>
    public static TopSearchesAnalyticsResponse ToTopSearchesResponse(
        AdminAnalyticsDateRange dateRange,
        int limit,
        IReadOnlyCollection<TopSearchedItemAnalyticsModel> items)
    {
        ArgumentNullException.ThrowIfNull(dateRange);
        ArgumentNullException.ThrowIfNull(items);

        return new TopSearchesAnalyticsResponse
        {
            From = ToDateTimeOffset(dateRange.FromUtc),
            To = ToDateTimeOffset(dateRange.ToUtc),
            Limit = limit,
            GeneratedAt = DateTimeOffset.UtcNow,

            Items = items
                .OrderByDescending(item => item.SearchCount)
                .ThenBy(item => item.NormalizedQueryText)
                .Select(ToTopSearchedItemResponse)
                .ToArray()
        };
    }

    /// <summary>
    /// Top saved analytics sonucunu API response modeline dönüştürür.
    /// </summary>
    public static TopSavedLearningItemsAnalyticsResponse ToTopSavedResponse(
        AdminAnalyticsDateRange dateRange,
        int limit,
        IReadOnlyCollection<TopSavedLearningItemAnalyticsModel> items)
    {
        ArgumentNullException.ThrowIfNull(dateRange);
        ArgumentNullException.ThrowIfNull(items);

        return new TopSavedLearningItemsAnalyticsResponse
        {
            From = ToDateTimeOffset(dateRange.FromUtc),
            To = ToDateTimeOffset(dateRange.ToUtc),
            Limit = limit,
            GeneratedAt = DateTimeOffset.UtcNow,

            Items = items
                .OrderByDescending(item => item.SaveCount)
                .ThenBy(item => item.DisplayText)
                .Select(ToTopSavedLearningItemResponse)
                .ToArray()
        };
    }

    /// <summary>
    /// Most wrong analytics sonucunu API response modeline dönüştürür.
    /// </summary>
    public static MostWrongLearningItemsAnalyticsResponse ToMostWrongResponse(
        AdminAnalyticsDateRange dateRange,
        int limit,
        IReadOnlyCollection<MostWrongLearningItemAnalyticsModel> items)
    {
        ArgumentNullException.ThrowIfNull(dateRange);
        ArgumentNullException.ThrowIfNull(items);

        return new MostWrongLearningItemsAnalyticsResponse
        {
            From = ToDateTimeOffset(dateRange.FromUtc),
            To = ToDateTimeOffset(dateRange.ToUtc),
            Limit = limit,
            GeneratedAt = DateTimeOffset.UtcNow,

            Items = items
                .OrderByDescending(item => item.WrongAnswerCount)
                .ThenByDescending(item => item.WrongRate)
                .ThenBy(item => item.DisplayText)
                .Select(ToMostWrongLearningItemResponse)
                .ToArray()
        };
    }

    /// <summary>
    /// Provider stats analytics sonucunu API response modeline dönüştürür.
    /// 
    /// Not:
    /// Total değerleri item listesinden hesaplıyoruz.
    /// Repository provider + operation bazlı satırlar dönecek.
    /// Response ise hem satırları hem de genel toplamı gösterecek.
    /// </summary>
    public static ProviderStatsAnalyticsResponse ToProviderStatsResponse(
        AdminAnalyticsDateRange dateRange,
        IReadOnlyCollection<ProviderStatAnalyticsModel> items)
    {
        ArgumentNullException.ThrowIfNull(dateRange);
        ArgumentNullException.ThrowIfNull(items);

        var itemResponses = items
            .OrderByDescending(item => item.TotalRequestCount)
            .ThenBy(item => item.ProviderName)
            .ThenBy(item => item.OperationName)
            .Select(ToProviderStatItemResponse)
            .ToArray();

        return new ProviderStatsAnalyticsResponse
        {
            From = ToDateTimeOffset(dateRange.FromUtc),
            To = ToDateTimeOffset(dateRange.ToUtc),
            GeneratedAt = DateTimeOffset.UtcNow,

            TotalProviderRequestCount = itemResponses.Sum(item => item.TotalRequestCount),
            TotalExternalCacheEntryCount = itemResponses.Sum(item => item.CacheEntryCount),
            TotalExternalCacheHitCount = itemResponses.Sum(item => item.CacheHitCount),
            TotalImportJobCount = itemResponses.Sum(item => item.ImportJobCount),

            Items = itemResponses
        };
    }

    /// <summary>
    /// Tek bir top search analytics modelini response satırına dönüştürür.
    /// </summary>
    private static TopSearchedItemResponse ToTopSearchedItemResponse(
        TopSearchedItemAnalyticsModel model)
    {
        return new TopSearchedItemResponse
        {
            QueryText = model.QueryText,
            NormalizedQueryText = model.NormalizedQueryText,
            InputType = model.InputType.ToString(),

            SearchCount = model.SearchCount,
            UniqueUserCount = model.UniqueUserCount,
            DatabaseHitCount = model.DatabaseHitCount,
            ProviderUsageCount = model.ProviderUsageCount,
            ProviderCreatedCount = model.ProviderCreatedCount,
            NotFoundCount = model.NotFoundCount,

            LearningItemId = model.LearningItemId,
            LastSearchedAt = ToRequiredDateTimeOffset(model.LastSearchedAtUtc)
        };
    }

    /// <summary>
    /// Tek bir top saved analytics modelini response satırına dönüştürür.
    /// </summary>
    private static TopSavedLearningItemResponse ToTopSavedLearningItemResponse(
        TopSavedLearningItemAnalyticsModel model)
    {
        return new TopSavedLearningItemResponse
        {
            LearningItemId = model.LearningItemId,
            ItemType = model.ItemType.ToString(),
            DisplayText = model.DisplayText,
            PrimaryMeaning = model.PrimaryMeaning,

            SaveCount = model.SaveCount,
            ActiveSaveCount = model.ActiveSaveCount,
            UniqueUserCount = model.UniqueUserCount,

            ContentSource = model.ContentSource.ToString(),
            QualityStatus = model.QualityStatus.ToString(),
            CefrLevel = model.CefrLevel.ToString(),
            DifficultyGroup = model.DifficultyGroup.ToString(),

            LastSavedAt = ToRequiredDateTimeOffset(model.LastSavedAtUtc)
        };
    }

    /// <summary>
    /// Tek bir most wrong analytics modelini response satırına dönüştürür.
    /// </summary>
    private static MostWrongLearningItemResponse ToMostWrongLearningItemResponse(
        MostWrongLearningItemAnalyticsModel model)
    {
        return new MostWrongLearningItemResponse
        {
            LearningItemId = model.LearningItemId,
            ItemType = model.ItemType.ToString(),
            DisplayText = model.DisplayText,

            WrongAnswerCount = model.WrongAnswerCount,
            CorrectAnswerCount = model.CorrectAnswerCount,
            TotalAnswerCount = model.TotalAnswerCount,
            WrongRate = RoundRate(model.WrongRate),
            AverageResponseTimeInMilliseconds = RoundMetric(model.AverageResponseTimeInMilliseconds),

            SystemRecommendedWrongAnswerCount = model.SystemRecommendedWrongAnswerCount,
            LastWrongAt = ToRequiredDateTimeOffset(model.LastWrongAtUtc)
        };
    }

    /// <summary>
    /// Tek bir provider stats analytics modelini response satırına dönüştürür.
    /// </summary>
    private static ProviderStatItemResponse ToProviderStatItemResponse(
        ProviderStatAnalyticsModel model)
    {
        return new ProviderStatItemResponse
        {
            ProviderName = model.ProviderName,
            ProviderType = model.ProviderType.ToString(),
            OperationName = model.OperationName,

            TotalRequestCount = model.TotalRequestCount,
            SucceededCount = model.SucceededCount,
            FailedCount = model.FailedCount,
            TimeoutCount = model.TimeoutCount,
            RateLimitedCount = model.RateLimitedCount,
            ServedFromCacheCount = model.ServedFromCacheCount,

            AverageDurationMs = RoundMetric(model.AverageDurationMs),

            CacheEntryCount = model.CacheEntryCount,
            CacheHitCount = model.CacheHitCount,

            ImportJobCount = model.ImportJobCount,
            FailedImportJobCount = model.FailedImportJobCount,

            LastRequestAt = ToDateTimeOffset(model.LastRequestAtUtc)
        };
    }

    /// <summary>
    /// Nullable UTC DateTime değerini nullable DateTimeOffset'e çevirir.
    /// 
    /// Neden gerekli?
    /// - Entity tarafında tarih alanlarımız çoğunlukla DateTime.
    /// - API response tarafında timezone bilgisini daha net ifade etmek için DateTimeOffset kullanıyoruz.
    /// - Verilerimizi UTC tuttuğumuz için DateTimeKind.Utc olarak işaretliyoruz.
    /// </summary>
    private static DateTimeOffset? ToDateTimeOffset(
        DateTime? utcDateTime)
    {
        if (utcDateTime is null)
        {
            return null;
        }

        return ToRequiredDateTimeOffset(utcDateTime.Value);
    }

    /// <summary>
    /// UTC DateTime değerini DateTimeOffset'e çevirir.
    /// </summary>
    private static DateTimeOffset ToRequiredDateTimeOffset(
        DateTime utcDateTime)
    {
        var utcValue = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return new DateTimeOffset(utcValue);
    }

    /// <summary>
    /// Oran değerlerini response için 2 basamağa yuvarlar.
    /// 
    /// Örnek:
    /// 73.333333 => 73.33
    /// </summary>
    private static double RoundRate(
        double value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Süre/ortalama gibi metrik değerlerini response için 2 basamağa yuvarlar.
    /// </summary>
    private static double RoundMetric(
        double value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}