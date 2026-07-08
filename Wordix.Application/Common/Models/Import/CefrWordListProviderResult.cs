namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// CEFR kelime listesi provider'ının okuma sonucunu temsil eder.
/// 
/// Provider dosyayı başarıyla okuyabilir veya hata alabilir.
/// Bu yüzden sadece liste dönmek yerine result modeli dönüyoruz.
/// 
/// Böylece import handler şunu anlayabilir:
/// - Okuma başarılı mı?
/// - Kaç satır geldi?
/// - Hangi hatalar oluştu?
/// </summary>
public sealed record CefrWordListProviderResult
{
    /// <summary>
    /// Provider okuma işlemi başarılı mı?
    /// 
    /// true:
    /// En azından dosya okunabildi ve satırlar parse edilebildi.
    /// 
    /// false:
    /// Dosya okunamadı, format bozuk veya kritik hata oluştu.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Provider tarafından okunan import satırlarıdır.
    /// 
    /// Bu satırlar henüz database'e kaydedilmiş değildir.
    /// Import handler bu satırları kullanarak LearningItem + Word oluşturacaktır.
    /// </summary>
    public IReadOnlyCollection<CefrWordImportRow> Rows { get; init; }
        = Array.Empty<CefrWordImportRow>();

    /// <summary>
    /// Okuma/parse sırasında oluşan hata mesajlarıdır.
    /// 
    /// Örnek:
    /// - "Line 15: CEFR level could not be parsed."
    /// - "File is empty."
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Başarılı provider sonucu oluşturur.
    /// 
    /// errors parametresi neden var?
    /// Provider genel olarak başarılı olabilir ama bazı satırlar parse edilememiş olabilir.
    /// Bu durumda Rows dolu gelir, Errors içinde ise atlanan satırların açıklaması tutulur.
    /// </summary>
    public static CefrWordListProviderResult Success(
        IReadOnlyCollection<CefrWordImportRow> rows,
        IReadOnlyCollection<string>? errors = null)
    {
        return new CefrWordListProviderResult
        {
            Succeeded = true,
            Rows = rows,
            Errors = errors ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Hatalı provider sonucu oluşturur.
    /// </summary>
    public static CefrWordListProviderResult Failure(
        IReadOnlyCollection<string> errors)
    {
        return new CefrWordListProviderResult
        {
            Succeeded = false,
            Rows = Array.Empty<CefrWordImportRow>(),
            Errors = errors
        };
    }
}