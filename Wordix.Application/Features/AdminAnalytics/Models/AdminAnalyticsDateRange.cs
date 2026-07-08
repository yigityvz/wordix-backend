namespace Wordix.Application.Features.AdminAnalytics.Models;

/// <summary>
/// Admin analytics sorgularında kullanılacak tarih aralığı modelidir.
/// 
/// Bu model API DTO değildir.
/// Repository ve handler arasında tarih filtresini taşımak için kullanılır.
/// 
/// Neden DateTimeOffset değil DateTime?
/// - Entity kayıtlarımız CreatedAt/SavedAt/AnsweredAt gibi alanlarda DateTime kullanıyor.
/// - Repository tarafında EF Core sorgularında DateTime ile filtrelemek daha doğrudan olur.
/// - API response tarafında gerekirse DateTimeOffset'e mapper ile dönüştürürüz.
/// </summary>
public sealed class AdminAnalyticsDateRange
{
    public DateTime? FromUtc { get; init; }

    public DateTime? ToUtc { get; init; }
}