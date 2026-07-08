using Wordix.Application.Common.Constants;
using Wordix.Application.Features.AdminAnalytics.Models;

namespace Wordix.Application.Features.AdminAnalytics.Services;

/// <summary>
/// Admin analytics query parametrelerini handler içinde güvenli varsayılanlara dönüştüren yardımcı sınıftır.
/// 
/// Bu class neden var?
/// - Bütün admin analytics handlerlarında fromUtc/toUtc/limit çözme kodunu tekrar etmek istemiyoruz.
/// - Tarih aralığı verilmezse son 30 gün varsayımını tek yerden yönetiyoruz.
/// - Limit verilmezse DefaultLimit değerini tek yerden uyguluyoruz.
/// 
/// Not:
/// Validation hâlâ FluentValidation ile yapılır.
/// Bu sınıf validation değil, default değer üretme sorumluluğuna sahiptir.
/// </summary>
public static class AdminAnalyticsQueryParameterResolver
{
    /// <summary>
    /// Query'den gelen nullable tarihleri kullanılabilir AdminAnalyticsDateRange modeline çevirir.
    /// 
    /// Kurallar:
    /// - ToUtc boşsa DateTime.UtcNow kullanılır.
    /// - FromUtc boşsa ToUtc değerinden DefaultDateRangeDays kadar geriye gidilir.
    /// - Tarihler UTC kabul edilir.
    /// 
    /// Örnek:
    /// Query hiç tarih göndermediyse:
    /// FromUtc = şimdi - 30 gün
    /// ToUtc = şimdi
    /// </summary>
    public static AdminAnalyticsDateRange ResolveDateRange(
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        var resolvedToUtc = NormalizeAsUtc(toUtc ?? DateTime.UtcNow);

        var resolvedFromUtc = NormalizeAsUtc(
            fromUtc ?? resolvedToUtc.AddDays(-AdminAnalyticsConstants.DefaultDateRangeDays));

        return new AdminAnalyticsDateRange
        {
            FromUtc = resolvedFromUtc,
            ToUtc = resolvedToUtc
        };
    }

    /// <summary>
    /// Liste endpointlerinde limit boş gelirse default limit döndürür.
    /// 
    /// Limit değerinin üst/alt sınır kontrolü validator tarafından yapılır.
    /// </summary>
    public static int ResolveLimit(
        int? limit)
    {
        return limit ?? AdminAnalyticsConstants.DefaultLimit;
    }

    /// <summary>
    /// DateTime değerini UTC kabul edilebilir hale getirir.
    /// 
    /// Frontend query string ile tarih gönderdiğinde DateTimeKind Unspecified gelebilir.
    /// Biz admin analytics tarafında bütün tarihleri UTC kabul ediyoruz.
    /// </summary>
    private static DateTime NormalizeAsUtc(
        DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}