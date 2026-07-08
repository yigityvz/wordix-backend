namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Meaning import provider sonucunu temsil eder.
/// 
/// Provider tamamen başarılı olabilir,
/// tamamen başarısız olabilir
/// veya bazı satırlar atlanıp bazıları başarıyla parse edilebilir.
/// 
/// Bu yüzden sonuç modelinde hem Rows hem Errors taşıyoruz.
/// </summary>
public sealed record MeaningImportProviderResult
{
    /// <summary>
    /// Provider'ın genel olarak başarılı olup olmadığını belirtir.
    /// 
    /// Örneğin dosya okunamadıysa veya JSON formatı tamamen bozuksa false döner.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Başarıyla parse edilen meaning satırlarıdır.
    /// </summary>
    public IReadOnlyCollection<MeaningImportRow> Rows { get; init; }
        = Array.Empty<MeaningImportRow>();

    /// <summary>
    /// Provider çalışırken oluşan hata veya uyarı mesajlarıdır.
    /// 
    /// Örnek:
    /// - Line 45: JSON parse edilemedi.
    /// - Line 120: Türkçe translation bulunamadı.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Başarılı result üretmek için kullanılır.
    /// </summary>
    public static MeaningImportProviderResult Success(
        IReadOnlyCollection<MeaningImportRow> rows,
        IReadOnlyCollection<string>? errors = null)
    {
        return new MeaningImportProviderResult
        {
            Succeeded = true,
            Rows = rows,
            Errors = errors ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Başarısız result üretmek için kullanılır.
    /// </summary>
    public static MeaningImportProviderResult Failure(
        IReadOnlyCollection<string> errors)
    {
        return new MeaningImportProviderResult
        {
            Succeeded = false,
            Rows = Array.Empty<MeaningImportRow>(),
            Errors = errors
        };
    }
}