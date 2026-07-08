namespace Wordix.Shared.Responses;

/// <summary>
/// Sayfalı liste response'ları için kullanılan ortak modeldir.
/// 
/// Bu class neden Shared katmanında?
/// - Pagination sadece UserStatistics'e özel değildir.
/// - Dictionary, deck, quiz history, admin listeleri gibi birçok endpoint ileride sayfalı dönebilir.
/// - ApiResponse gibi dış dünyaya dönen ortak response modelleri Shared katmanında durur.
/// 
/// Bu model ne yapar?
/// - Liste itemlarını taşır.
/// - Toplam kayıt sayısını taşır.
/// - Sayfa numarası ve sayfa boyutunu taşır.
/// - Frontend'in pagination UI kurabilmesi için toplam sayfa ve next/previous bilgisini hesaplar.
/// </summary>
/// <typeparam name="TItem">
/// Sayfalı listede dönecek item tipidir.
/// Örnek:
/// DifficultLearningItemResponse
/// </typeparam>
public sealed class PagedResult<TItem>
{
    /// <summary>
    /// EF Core veya mapper kullanımı için boş constructor.
    /// Genelde static Create methodu tercih edilmelidir.
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Sayfalı sonucu güvenli şekilde oluşturur.
    /// </summary>
    public PagedResult(
        IReadOnlyCollection<TItem> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException(
                "PageNumber 0'dan büyük olmalıdır.",
                nameof(pageNumber));
        }

        if (pageSize <= 0)
        {
            throw new ArgumentException(
                "PageSize 0'dan büyük olmalıdır.",
                nameof(pageSize));
        }

        if (totalCount < 0)
        {
            throw new ArgumentException(
                "TotalCount negatif olamaz.",
                nameof(totalCount));
        }

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Bu sayfada dönen kayıtlar.
    /// </summary>
    public IReadOnlyCollection<TItem> Items { get; init; }
        = Array.Empty<TItem>();

    /// <summary>
    /// Aktif sayfa numarasıdır.
    /// 
    /// 1 tabanlıdır.
    /// Yani ilk sayfa 1'dir.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Bir sayfada kaç kayıt döneceğini belirtir.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Filtreye uyan toplam kayıt sayısıdır.
    /// 
    /// Sadece bu sayfadaki kayıt sayısı değildir.
    /// Frontend toplam sayfa sayısını buradan hesaplayabilir.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Toplam sayfa sayısıdır.
    /// 
    /// Örnek:
    /// TotalCount = 45
    /// PageSize = 20
    /// TotalPages = 3
    /// </summary>
    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Önceki sayfa var mı?
    /// </summary>
    public bool HasPreviousPage =>
        PageNumber > 1;

    /// <summary>
    /// Sonraki sayfa var mı?
    /// </summary>
    public bool HasNextPage =>
        PageNumber < TotalPages;

    /// <summary>
    /// PagedResult oluşturmayı okunabilir hale getiren factory method.
    /// </summary>
    public static PagedResult<TItem> Create(
        IReadOnlyCollection<TItem> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedResult<TItem>(
            items,
            pageNumber,
            pageSize,
            totalCount);
    }
}