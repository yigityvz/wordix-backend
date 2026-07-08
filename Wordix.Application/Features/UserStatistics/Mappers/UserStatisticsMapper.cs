using Wordix.Application.Features.UserStatistics.Dtos.Responses;
using Wordix.Application.Features.UserStatistics.Models;
using Wordix.Shared.Responses;
using Wordix.Application.Features.UserStatistics.Dtos.Requests;
using Wordix.Application.Features.UserStatistics.Queries.GetConfidenceScoreDistribution;
using Wordix.Application.Features.UserStatistics.Queries.GetDeckStatistics;
using Wordix.Application.Features.UserStatistics.Queries.GetDifficultItems;
using Wordix.Application.Features.UserStatistics.Queries.GetQuizStatistics;
using Wordix.Application.Features.UserStatistics.Queries.GetUserLearningSummary;

namespace Wordix.Application.Features.UserStatistics.Mappers;

/// <summary>
/// UserStatistics feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu mapper neden var?
/// - Handler içinde response DTO propertylerini tek tek dizmek istemiyoruz.
/// - Controller zaten sadece HTTP + MediatR akışını yönetmeli.
/// - Repository'den gelen internal modelleri API response contract'larına burada dönüştürüyoruz.
/// - AutoMapper/Mapster kullanmadan açık, okunabilir ve kontrollü mapping yapıyoruz.
/// 
/// Faz 17 sonrası kalıcı standardımıza uygundur:
/// - DTO'lar feature altında durur.
/// - Mapping feature altında explicit mapper ile yapılır.
/// - Handler use-case akışına odaklanır.
/// </summary>
public static class UserStatisticsMapper
{
    /// <summary>
    /// Kullanıcının öğrenme özet modelini API response modeline dönüştürür.
    /// 
    /// Bu response dashboard'un üst kartlarını besler.
    /// </summary>
    public static UserLearningSummaryResponse ToUserLearningSummaryResponse(
        UserLearningSummaryModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new UserLearningSummaryResponse
        {
            TotalSavedItemCount = model.TotalSavedItemCount,
            ActiveSavedItemCount = model.ActiveSavedItemCount,

            WordCount = model.WordCount,
            PhraseCount = model.PhraseCount,
            SentenceCount = model.SentenceCount,

            NewItemCount = model.NewItemCount,
            LearningItemCount = model.LearningItemCount,
            ReviewingItemCount = model.ReviewingItemCount,
            LearnedItemCount = model.LearnedItemCount,
            MasteredItemCount = model.MasteredItemCount,

            ReviewDueItemCount = model.ReviewDueItemCount,
            AverageConfidenceScore = RoundMetric(model.AverageConfidenceScore),

            FavoriteItemCount = model.FavoriteItemCount,
            DifficultItemCount = model.DifficultItemCount,
            WantMorePracticeItemCount = model.WantMorePracticeItemCount,
            IgnoredItemCount = model.IgnoredItemCount,

            TotalCorrectAnswerCount = model.TotalCorrectAnswerCount,
            TotalIncorrectAnswerCount = model.TotalIncorrectAnswerCount,
            TotalPartiallyCorrectAnswerCount = model.TotalPartiallyCorrectAnswerCount,
            TotalSkippedAnswerCount = model.TotalSkippedAnswerCount,
            OverallAccuracyRate = RoundRate(model.OverallAccuracyRate),

            LastReviewedAt = ToDateTimeOffset(model.LastReviewedAtUtc),
            NextReviewDate = ToDateTimeOffset(model.NextReviewDateUtc),
            LastQuizStartedAt = ToDateTimeOffset(model.LastQuizStartedAtUtc),

            GeneratedAt = ToRequiredDateTimeOffset(model.GeneratedAtUtc)
        };
    }

    /// <summary>
    /// Learning summary endpointi için MediatR query oluşturur.
    /// 
    /// Bu endpoint parametre almaz.
    /// Kullanıcı id handler içinde current user token'ından alınır.
    /// </summary>
    public static GetUserLearningSummaryQuery ToGetUserLearningSummaryQuery()
    {
        return new GetUserLearningSummaryQuery();
    }

    /// <summary>
    /// QuizStatisticsRequest DTO'sunu GetQuizStatisticsQuery modeline dönüştürür.
    /// 
    /// Controller query string'den gelen request DTO'yu mapper'a verir.
    /// Mapper MediatR query oluşturur.
    /// Validation, query validator içinde yapılır.
    /// </summary>
    public static GetQuizStatisticsQuery ToGetQuizStatisticsQuery(
        QuizStatisticsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetQuizStatisticsQuery(
            request.FromUtc,
            request.ToUtc,
            request.QuizType,
            request.QuizSourceType,
            request.QuizContentMode,
            request.DifficultyGroup);
    }

    /// <summary>
    /// DifficultItemsRequest DTO'sunu GetDifficultItemsQuery modeline dönüştürür.
    /// 
    /// Pagination ve filter değerleri burada sadece taşınır.
    /// Default değer verme ve enum parse etme işlemi handler/resolver tarafında yapılacaktır.
    /// Validation ise FluentValidation pipeline'da çalışır.
    /// </summary>
    public static GetDifficultItemsQuery ToGetDifficultItemsQuery(
        DifficultItemsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new GetDifficultItemsQuery(
            request.PageNumber,
            request.PageSize,
            request.Source,
            request.SortBy,
            request.ItemType,
            request.LearningStatus);
    }

    /// <summary>
    /// Deck statistics endpointi için MediatR query oluşturur.
    /// 
    /// Query parametresi yoktur.
    /// Kullanıcı id request'ten alınmaz.
    /// </summary>
    public static GetDeckStatisticsQuery ToGetDeckStatisticsQuery()
    {
        return new GetDeckStatisticsQuery();
    }

    /// <summary>
    /// Confidence score distribution endpointi için MediatR query oluşturur.
    /// 
    /// Query parametresi yoktur.
    /// </summary>
    public static GetConfidenceScoreDistributionQuery ToGetConfidenceScoreDistributionQuery()
    {
        return new GetConfidenceScoreDistributionQuery();
    }


    /// <summary>
    /// Quiz statistics modelini API response modeline dönüştürür.
    /// 
    /// filter parametresi neden gerekli?
    /// - Response içinde hangi filtrelerle rapor üretildiğini frontend'e göstermek istiyoruz.
    /// - Repository sonucu sadece aggregate sayıları taşır.
    /// - Tarih aralığı ve enum filtreleri filter modelinden gelir.
    /// </summary>
    public static QuizStatisticsResponse ToQuizStatisticsResponse(
        QuizStatisticsFilter filter,
        QuizStatisticsModel model)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(model);

        return new QuizStatisticsResponse
        {
            From = ToDateTimeOffset(filter.DateRange.FromUtc),
            To = ToDateTimeOffset(filter.DateRange.ToUtc),

            QuizType = filter.QuizType?.ToString(),
            QuizSourceType = filter.QuizSourceType?.ToString(),
            QuizContentMode = filter.QuizContentMode?.ToString(),
            DifficultyGroup = filter.DifficultyGroup?.ToString(),

            TotalQuizSessionCount = model.TotalQuizSessionCount,
            CompletedQuizSessionCount = model.CompletedQuizSessionCount,
            InProgressQuizSessionCount = model.InProgressQuizSessionCount,
            CancelledQuizSessionCount = model.CancelledQuizSessionCount,

            TestQuizSessionCount = model.TestQuizSessionCount,
            WritingQuizSessionCount = model.WritingQuizSessionCount,
            MixedQuizSessionCount = model.MixedQuizSessionCount,

            UserDictionaryQuizSessionCount = model.UserDictionaryQuizSessionCount,
            DeckQuizSessionCount = model.DeckQuizSessionCount,
            DifficultItemsQuizSessionCount = model.DifficultItemsQuizSessionCount,
            SystemRecommendationsQuizSessionCount = model.SystemRecommendationsQuizSessionCount,

            TotalQuestionCount = model.TotalQuestionCount,
            SystemRecommendedQuestionCount = model.SystemRecommendedQuestionCount,

            TotalAnswerCount = model.TotalAnswerCount,
            CorrectAnswerCount = model.CorrectAnswerCount,
            IncorrectAnswerCount = model.IncorrectAnswerCount,
            PartiallyCorrectAnswerCount = model.PartiallyCorrectAnswerCount,
            SkippedAnswerCount = model.SkippedAnswerCount,

            SystemRecommendedCorrectAnswerCount = model.SystemRecommendedCorrectAnswerCount,
            SystemRecommendedIncorrectAnswerCount = model.SystemRecommendedIncorrectAnswerCount,

            AccuracyRate = RoundRate(model.AccuracyRate),
            AverageResponseTimeMs = RoundMetric(model.AverageResponseTimeMs),

            LastQuizStartedAt = ToDateTimeOffset(model.LastQuizStartedAtUtc),
            LastAnsweredAt = ToDateTimeOffset(model.LastAnsweredAtUtc),

            GeneratedAt = ToRequiredDateTimeOffset(model.GeneratedAtUtc)
        };
    }

    /// <summary>
    /// Difficult item model listesini sayfalı API response modeline dönüştürür.
    /// 
    /// Burada PagedResult&lt;DifficultLearningItemModel&gt; alıp
    /// PagedResult&lt;DifficultLearningItemResponse&gt; döndürüyoruz.
    /// 
    /// Böylece pagination bilgisi korunur:
    /// - PageNumber
    /// - PageSize
    /// - TotalCount
    /// - TotalPages
    /// - HasNextPage
    /// - HasPreviousPage
    /// </summary>
    public static PagedResult<DifficultLearningItemResponse> ToDifficultItemsResponse(
        PagedResult<DifficultLearningItemModel> pagedResult)
    {
        ArgumentNullException.ThrowIfNull(pagedResult);

        var items = pagedResult.Items
            .Select(ToDifficultLearningItemResponse)
            .ToArray();

        return PagedResult<DifficultLearningItemResponse>.Create(
            items,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount);
    }

    /// <summary>
    /// Deck statistics model listesini API response modeline dönüştürür.
    /// </summary>
    public static DeckStatisticsResponse ToDeckStatisticsResponse(
        IReadOnlyCollection<DeckStatisticsModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);

        var items = models
            .OrderByDescending(deck => deck.LastQuizStartedAtUtc ?? deck.LastItemAddedAtUtc ?? DateTime.MinValue)
            .ThenBy(deck => deck.DeckName)
            .Select(ToDeckStatisticsItemResponse)
            .ToArray();

        return new DeckStatisticsResponse
        {
            TotalDeckCount = models.Count,
            ActiveDeckCount = models.Count(deck => deck.IsActive),
            GeneratedAt = DateTimeOffset.UtcNow,
            Items = items
        };
    }

    /// <summary>
    /// Confidence score dağılım modelini API response modeline dönüştürür.
    /// 
    /// Bu response frontend chart/grafik çizimi için uygundur.
    /// </summary>
    public static ConfidenceScoreDistributionResponse ToConfidenceScoreDistributionResponse(
        ConfidenceScoreDistributionModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new ConfidenceScoreDistributionResponse
        {
            TotalItemCount = model.TotalItemCount,
            AverageConfidenceScore = RoundMetric(model.AverageConfidenceScore),
            GeneratedAt = ToRequiredDateTimeOffset(model.GeneratedAtUtc),
            Buckets = model.Buckets
                .OrderBy(bucket => bucket.MinScore)
                .Select(ToConfidenceScoreBucketResponse)
                .ToArray()
        };
    }

    /// <summary>
    /// Tek bir difficult learning item modelini response satırına dönüştürür.
    /// </summary>
    private static DifficultLearningItemResponse ToDifficultLearningItemResponse(
        DifficultLearningItemModel model)
    {
        return new DifficultLearningItemResponse
        {
            UserLearningItemId = model.UserLearningItemId,
            LearningItemId = model.LearningItemId,
            ItemType = model.ItemType.ToString(),
            DisplayText = model.DisplayText,
            PrimaryMeaning = model.PrimaryMeaning,

            LearningStatus = model.LearningStatus.ToString(),
            ConfidenceScore = model.ConfidenceScore,
            CorrectCount = model.CorrectCount,
            WrongCount = model.WrongCount,
            ConsecutiveWrongCount = model.ConsecutiveWrongCount,
            RepetitionLevel = model.RepetitionLevel,

            IsManuallyMarkedDifficult = model.IsManuallyMarkedDifficult,
            IsProgressDifficult = model.IsProgressDifficult,
            DifficultyReason = model.DifficultyReason,

            SavedAt = ToRequiredDateTimeOffset(model.SavedAtUtc),
            LastReviewedAt = ToDateTimeOffset(model.LastReviewedAtUtc),
            NextReviewDate = ToDateTimeOffset(model.NextReviewDateUtc)
        };
    }

    /// <summary>
    /// Tek bir deck statistics modelini response satırına dönüştürür.
    /// </summary>
    private static DeckStatisticsItemResponse ToDeckStatisticsItemResponse(
        DeckStatisticsModel model)
    {
        return new DeckStatisticsItemResponse
        {
            DeckId = model.DeckId,
            DeckName = model.DeckName,
            Description = model.Description,
            IsActive = model.IsActive,

            ItemCount = model.ItemCount,
            ActiveItemCount = model.ActiveItemCount,
            AverageConfidenceScore = RoundMetric(model.AverageConfidenceScore),
            DueReviewItemCount = model.DueReviewItemCount,
            DifficultItemCount = model.DifficultItemCount,

            QuizSessionCount = model.QuizSessionCount,
            CompletedQuizSessionCount = model.CompletedQuizSessionCount,

            TotalAnswerCount = model.TotalAnswerCount,
            CorrectAnswerCount = model.CorrectAnswerCount,
            IncorrectAnswerCount = model.IncorrectAnswerCount,
            AccuracyRate = RoundRate(model.AccuracyRate),

            LastQuizStartedAt = ToDateTimeOffset(model.LastQuizStartedAtUtc),
            LastItemAddedAt = ToDateTimeOffset(model.LastItemAddedAtUtc)
        };
    }

    /// <summary>
    /// Tek bir confidence score bucket modelini response satırına dönüştürür.
    /// </summary>
    private static ConfidenceScoreBucketResponse ToConfidenceScoreBucketResponse(
        ConfidenceScoreBucketModel model)
    {
        return new ConfidenceScoreBucketResponse
        {
            Label = model.Label,
            MinScore = model.MinScore,
            MaxScore = model.MaxScore,
            ItemCount = model.ItemCount,
            Percentage = RoundRate(model.Percentage)
        };
    }

    /// <summary>
    /// Nullable UTC DateTime değerini nullable DateTimeOffset'e çevirir.
    /// 
    /// Neden gerekli?
    /// - Entity tarafında tarih alanlarımız çoğunlukla DateTime.
    /// - API response tarafında timezone bilgisini daha net ifade etmek için DateTimeOffset kullanıyoruz.
    /// - Verilerimizi UTC kabul ettiğimiz için DateTimeKind.Utc olarak işaretliyoruz.
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