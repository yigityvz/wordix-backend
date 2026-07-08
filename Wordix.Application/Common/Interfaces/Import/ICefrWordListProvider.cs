using Wordix.Application.Common.Models.Import;

namespace Wordix.Application.Common.Interfaces.Import;

/// <summary>
/// CEFR kelime listesi sağlayıcılarının uyması gereken sözleşmedir.
/// 
/// Bu interface Application katmanında durur.
/// Gerçek implementasyon Infrastructure katmanında yazılacaktır.
/// 
/// Neden?
/// - Application katmanı "kelime listesi oku" ihtiyacını bilir.
/// - Ama CSV nasıl okunur, dosya nereden gelir, GitHub'dan mı çekilir gibi detayları bilmez.
/// - Bu detaylar Infrastructure katmanının sorumluluğudur.
/// 
/// Örnek implementasyonlar:
/// - CefrJCsvWordListProvider
/// - LocalFileCefrWordListProvider
/// - GitHubCefrWordListProvider
/// </summary>
public interface ICefrWordListProvider
{
    /// <summary>
    /// Verilen stream üzerinden CEFR kelime listesini okur.
    /// 
    /// Stream kullanmamızın sebebi:
    /// - Dosya local diskten gelebilir.
    /// - API upload olarak gelebilir.
    /// - GitHub'dan indirilen içerik memory stream olabilir.
    /// 
    /// Application katmanı bunların hangisi olduğunu bilmek zorunda kalmaz.
    /// </summary>
    /// <param name="sourceStream">
    /// CEFR kelime listesini içeren dosya stream'i.
    /// </param>
    /// <param name="cancellationToken">
    /// İşlem iptal edilirse provider okumayı durdurabilsin diye kullanılır.
    /// </param>
    /// <returns>
    /// Okunan CEFR kelime satırlarını ve hata bilgilerini içeren result modeli.
    /// </returns>
    Task<CefrWordListProviderResult> LoadAsync(
        Stream sourceStream,
        CancellationToken cancellationToken = default);
}