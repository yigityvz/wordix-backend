namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Example sentence import provider sonucunu temsil eder.
/// 
/// Provider tamamen başarılı olabilir,
/// tamamen başarısız olabilir
/// veya bazı satırlar atlanıp bazıları başarıyla parse edilebilir.
/// 
/// Bu yüzden result modelinde hem Rows hem Errors taşıyoruz.
/// </summary>
public sealed record ExampleSentenceImportProviderResult
{
    /// <summary>
    /// Provider genel olarak başarılı çalıştı mı?
    /// 
    /// Örneğin gerekli stream'lerden biri yoksa false döner.
    /// Ama dosyada birkaç bozuk satır varsa provider yine true dönebilir,
    /// bozuk satırlar Errors içinde raporlanır.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Başarıyla parse edilen example sentence eşleşmeleridir.
    /// </summary>
    public IReadOnlyCollection<ExampleSentenceImportRow> Rows { get; init; }
        = Array.Empty<ExampleSentenceImportRow>();

    /// <summary>
    /// Provider çalışırken oluşan hata veya uyarı mesajlarıdır.
    /// 
    /// Örnek:
    /// - Source line 45: format invalid.
    /// - Link line 120: source or target sentence not found.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// Başarılı result üretmek için kullanılır.
    /// </summary>
    public static ExampleSentenceImportProviderResult Success(
        IReadOnlyCollection<ExampleSentenceImportRow> rows,
        IReadOnlyCollection<string>? errors = null)
    {
        return new ExampleSentenceImportProviderResult
        {
            Succeeded = true,
            Rows = rows,
            Errors = errors ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Başarısız result üretmek için kullanılır.
    /// </summary>
    public static ExampleSentenceImportProviderResult Failure(
        IReadOnlyCollection<string> errors)
    {
        return new ExampleSentenceImportProviderResult
        {
            Succeeded = false,
            Rows = Array.Empty<ExampleSentenceImportRow>(),
            Errors = errors
        };
    }
}