namespace Wordix.Application.Features.UserStatistics.Dtos.Requests;

/// <summary>
/// Kullanıcının zorlandığı learning item listesini filtrelemek ve sayfalamak için kullanılan request DTO'sudur.
/// 
/// Endpoint hedefi:
/// GET /api/user-statistics/difficult-items
/// 
/// Bu endpoint liste döndürdüğü için pagination zorunlu tutulacaktır.
/// Production notlarında list endpointleri için pagination'ın gerekli olduğu özellikle belirtilmişti.
/// </summary>
public sealed class DifficultItemsRequest
{
    /// <summary>
    /// Sayfa numarasıdır.
    /// 
    /// Null gelirse DefaultPageNumber kullanılacaktır.
    /// İlk sayfa 1'dir.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Bir sayfada kaç kayıt döneceğidir.
    /// 
    /// Null gelirse DefaultPageSize kullanılacaktır.
    /// MaxPageSize üstüne çıkmasına izin verilmeyecektir.
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// Difficult item kaynağı filtresidir.
    /// 
    /// Desteklenen değerler:
    /// - both
    /// - manual
    /// - progress
    /// 
    /// both:
    /// Manuel Difficult flag'i veya progress sinyali olan itemlar.
    /// 
    /// manual:
    /// Sadece UserLearningFlagType.Difficult olan itemlar.
    /// 
    /// progress:
    /// Sadece düşük confidence / yanlış sayısı / due review gibi sistemsel sinyaller.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Liste sıralama değeridir.
    /// 
    /// Desteklenen değerler:
    /// - confidenceAsc
    /// - wrongCountDesc
    /// - consecutiveWrongDesc
    /// - nextReviewAsc
    /// - savedAtDesc
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>
    /// İçerik tipi filtresidir.
    /// 
    /// Desteklenen örnek değerler:
    /// - Word
    /// - Phrase
    /// - Sentence
    /// 
    /// Null gelirse tüm içerik tipleri döner.
    /// </summary>
    public string? ItemType { get; init; }

    /// <summary>
    /// Öğrenme durumu filtresidir.
    /// 
    /// Desteklenen örnek değerler:
    /// - New
    /// - Learning
    /// - Reviewing
    /// - Learned
    /// - Mastered
    /// 
    /// Null gelirse tüm status değerleri döner.
    /// </summary>
    public string? LearningStatus { get; init; }
}