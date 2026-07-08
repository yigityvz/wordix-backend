namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// CEFR kelime import işlemi tamamlandığında API tarafına dönecek response modelidir.
/// 
/// Bu DTO neden var?
/// - Controller doğrudan domain entity döndürmemeli.
/// - Kullanıcıya/import çağrısını yapan kişiye sadece işlem özeti dönmeliyiz.
/// - Kaç satır okundu, kaç kelime eklendi, kaç kelime atlandı gibi bilgiler burada tutulur.
/// </summary>
public sealed record ImportCefrWordsResponse
{
    /// <summary>
    /// Bu import/enrichment işlemini takip eden ImportJob id değeridir.
    /// 
    /// Admin panel veya SQL diagnostic sırasında bu id üzerinden
    /// ImportJobs tablosundaki job kaydı bulunabilir.
    /// </summary>
    public Guid? ImportJobId { get; init; }

    /// <summary>
    /// Provider tarafından başarıyla parse edilen toplam satır sayısıdır.
    /// 
    /// Örneğin CSV içinde 9500 geçerli kelime varsa 9500 olabilir.
    /// </summary>
    public int TotalParsedRows { get; init; }

    /// <summary>
    /// Database'e yeni eklenen kelime sayısıdır.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Zaten database'de olduğu için atlanan kelime sayısıdır.
    /// 
    /// Örnek:
    /// "achieve" zaten varsa tekrar eklenmez.
    /// </summary>
    public int SkippedExistingCount { get; init; }

    /// <summary>
    /// Hatalı veya işlenemeyen satır sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// Import işlemi gerçekten database'e yazdı mı, yoksa sadece ön izleme mi yaptı?
    /// </summary>
    public bool DryRun { get; init; }

    /// <summary>
    /// DryRun aktifken database'e yazılmayacak ama yazılabilir görünen kelime sayısıdır.
    /// 
    /// DryRun false ise genelde 0 kalır.
    /// </summary>
    public int WouldCreateCount { get; init; }

    /// <summary>
    /// Provider veya import sırasında oluşan uyarı/hata mesajlarıdır.
    /// 
    /// Örnek:
    /// - Line 25: CEFR değeri parse edilemedi.
    /// - Word already exists: achieve
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; } = Array.Empty<string>();
}