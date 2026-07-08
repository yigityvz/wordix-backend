namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Bir ImportJob tamamlandığında kullanılacak sayaç/request modelidir.
/// 
/// Bu model neden var?
/// - Her import/enrichment akışı farklı response üretebilir.
/// - Ama ImportJob tablosuna yazılacak sayaçlar standart olmalıdır.
/// - Handler veya service tarafında uzun parametre listesi geçirmek yerine
///   bu küçük request modeliyle daha okunabilir bir yapı kuruyoruz.
/// </summary>
public sealed record ImportJobCompletionRequest
{
    /// <summary>
    /// Provider/parser tarafından görülen toplam input satır sayısıdır.
    /// </summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Başarıyla işlenen satır sayısıdır.
    /// </summary>
    public int ProcessedRows { get; init; }

    /// <summary>
    /// Oluşturulan kayıt sayısıdır.
    /// 
    /// Örnek:
    /// Tatoeba enrichment için CreatedExampleLinkCount.
    /// CEFR import için CreatedCount.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Güncellenen kayıt sayısıdır.
    /// İlk fazlarda genelde 0 kalabilir.
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Duplicate, limit veya filtre nedeniyle atlanan kayıt sayısıdır.
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// Hata alan kayıt sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// Job sonucuyla ilgili kısa özet mesajdır.
    /// </summary>
    public string? SummaryMessage { get; init; }
}