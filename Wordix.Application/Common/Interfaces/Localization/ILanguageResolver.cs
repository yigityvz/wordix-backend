using Wordix.Application.Common.Models.Localization;

namespace Wordix.Application.Common.Interfaces.Localization;

/// <summary>
/// Dil koduna göre aktif language bilgisini çözen servis sözleşmesidir.
/// 
/// Bu interface neden Application katmanında?
/// - Application use-case'leri dil bilgisine ihtiyaç duyar.
/// - Ama dil bilgisinin database'den mi, cache'ten mi, Redis'ten mi geldiğini bilmemelidir.
/// - Bu interface ihtiyacı tanımlar.
/// - Implementasyon Persistence katmanında yapılır.
/// 
/// Böylece Application katmanı EF Core, IMemoryCache veya DbContext detaylarına bağımlı olmaz.
/// </summary>
public interface ILanguageResolver
{
    /// <summary>
    /// Verilen dil koduna göre aktif language bilgisini döner.
    /// 
    /// Beklenen davranış:
    /// - languageCode normalize edilir.
    /// - aktif Language kaydı aranır.
    /// - bulunamazsa NotFoundException fırlatılır.
    /// - bulunursa LanguageLookupData döner.
    /// 
    /// Not:
    /// Bu methodun implementasyonu cache kullanabilir.
    /// Ama cache detayı Application katmanından gizlenir.
    /// </summary>
    /// <param name="languageCode">
    /// Kullanıcıdan veya sistemden gelen dil kodu.
    /// Örnek: "en", "tr", " EN "
    /// </param>
    /// <param name="cancellationToken">
    /// Async operasyon iptal token'ı.
    /// </param>
    /// <returns>
    /// Aktif dil bilgisini taşıyan sade lookup modeli.
    /// </returns>
    Task<LanguageLookupData> GetRequiredActiveLanguageByCodeAsync(
        string languageCode,
        CancellationToken cancellationToken = default);
}