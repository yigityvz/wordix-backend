using Wordix.Application.Features.AdminAnalytics.Models;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Admin analytics ekranları için gerekli aggregate/raporlama sorgularını temsil eden repository abstraction'ıdır.
/// 
/// Bu interface neden var?
/// - Admin analytics sorguları basit CRUD sorguları değildir.
/// - LookupHistory, UserLearningItem, QuizAnswer, ProviderRequestLog gibi büyük tablolarda
///   GroupBy, Count, Distinct, Average, Max gibi aggregate sorgular çalıştırılır.
/// - Bu sorguları handler içinde yazarsak Application katmanı DbContext bilmek zorunda kalır.
/// - Bu sorguları generic repository ile yaparsak ya çok fazla veri memory'ye çekilir
///   ya da generic repository'nin sorumluluğu bozulur.
/// 
/// Mimari karar:
/// - Application katmanı sadece bu abstraction'ı bilir.
/// - EF Core / SQL sorgu detayları Persistence katmanındaki AdminAnalyticsRepository içinde yazılır.
/// - Böylece Clean Architecture korunur.
/// </summary>
public interface IAdminAnalyticsRepository
{
    /// <summary>
    /// Admin dashboard için sistem geneli özet metrikleri döndürür.
    /// 
    /// Bu metrikler örnek olarak şunları kapsar:
    /// - Toplam lookup sayısı
    /// - Toplam dictionary save sayısı
    /// - Quiz session / answer sayısı
    /// - Doğru/yanlış cevap oranları
    /// - Provider request/cache/import job özetleri
    /// 
    /// dateRange:
    /// - FromUtc ve ToUtc doluysa sadece ilgili aralıktaki kayıtlar analiz edilir.
    /// - Handler tarafında varsayılan tarih aralığı üretileceği için repository sadece gelen filtreyi uygular.
    /// </summary>
    Task<AdminDashboardAnalyticsModel> GetDashboardAsync(
        AdminAnalyticsDateRange dateRange,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// En çok aranan metinleri döndürür.
    /// 
    /// Kaynak tablo:
    /// - LookupHistories
    /// 
    /// Bu sorgu şunları hesaplar:
    /// - Aynı normalize query kaç kez aranmış?
    /// - Kaç farklı kullanıcı aramış?
    /// - Kaçında database sonucu bulunmuş?
    /// - Kaçında provider kullanılmış?
    /// - Kaçında provider yeni global içerik oluşturmuş?
    /// 
    /// limit:
    /// - Dönecek maksimum kayıt sayısıdır.
    /// - Validator ve handler tarafından güvenli aralıkta tutulacaktır.
    /// </summary>
    Task<IReadOnlyCollection<TopSearchedItemAnalyticsModel>> GetTopSearchesAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcılar tarafından en çok dictionary'ye kaydedilen LearningItem kayıtlarını döndürür.
    /// 
    /// Kaynak tablolar:
    /// - UserLearningItems
    /// - LearningItems
    /// - Words
    /// - Phrases
    /// - Sentences
    /// - Meanings
    /// 
    /// Bu sorgu LearningItem merkezli çalışır.
    /// Böylece sadece Word değil, Phrase ve ileride Sentence içerikleri de aynı analytics akışına girebilir.
    /// </summary>
    Task<IReadOnlyCollection<TopSavedLearningItemAnalyticsModel>> GetTopSavedLearningItemsAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Quizlerde en çok yanlış yapılan LearningItem kayıtlarını döndürür.
    /// 
    /// Kaynak tablolar:
    /// - QuizAnswers
    /// - QuizQuestions
    /// - LearningItems
    /// - Words
    /// - Phrases
    /// - Sentences
    /// 
    /// Bu sorgu şunları hesaplar:
    /// - Yanlış cevap sayısı
    /// - Toplam cevap sayısı
    /// - Yanlış oranı
    /// - Ortalama cevap süresi
    /// - Sistem önerisi olarak gelen sorularda yanlış sayısı
    /// </summary>
    Task<IReadOnlyCollection<MostWrongLearningItemAnalyticsModel>> GetMostWrongLearningItemsAsync(
        AdminAnalyticsDateRange dateRange,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Provider, cache ve import job istatistiklerini döndürür.
    /// 
    /// Kaynak tablolar:
    /// - ProviderRequestLogs
    /// - ExternalContentCaches
    /// - ImportJobs
    /// 
    /// Bu sorgu provider + operation bazlı çalışır.
    /// Örnek satırlar:
    /// - AzureTranslator / Translation / Translate
    /// - Tatoeba / ExampleSentence / EnrichTatoebaExampleSentences
    /// - FreeDict / Dictionary / EnrichFreeDictMeanings
    /// </summary>
    Task<IReadOnlyCollection<ProviderStatAnalyticsModel>> GetProviderStatsAsync(
        AdminAnalyticsDateRange dateRange,
        CancellationToken cancellationToken = default);
}