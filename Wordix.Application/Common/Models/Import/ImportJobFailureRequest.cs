namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Bir ImportJob hata ile bittiğinde kullanılacak request modelidir.
/// 
/// Hata durumunda da sayaçları tutmak istiyoruz.
/// Çünkü job yarıda patlamış olsa bile kaç satıra kadar işlediğini bilmek değerlidir.
/// </summary>
public sealed record ImportJobFailureRequest
{
    /// <summary>
    /// Hatanın admin/developer tarafından okunabilir mesajıdır.
    /// 
    /// Buraya secret, connection string, API key veya token yazılmamalıdır.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Provider/parser tarafından görülen toplam input satır sayısıdır.
    /// </summary>
    public int TotalRows { get; init; }

    /// <summary>
    /// Hata oluşana kadar işlenen satır sayısıdır.
    /// </summary>
    public int ProcessedRows { get; init; }

    /// <summary>
    /// Hata oluşana kadar oluşturulan kayıt sayısıdır.
    /// </summary>
    public int CreatedCount { get; init; }

    /// <summary>
    /// Hata oluşana kadar güncellenen kayıt sayısıdır.
    /// </summary>
    public int UpdatedCount { get; init; }

    /// <summary>
    /// Hata oluşana kadar skip edilen kayıt sayısıdır.
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// Hata alan kayıt sayısıdır.
    /// </summary>
    public int FailedCount { get; init; }
}