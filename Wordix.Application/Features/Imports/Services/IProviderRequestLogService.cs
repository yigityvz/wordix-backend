using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Dış provider çağrılarını merkezi şekilde loglayan service sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Azure, Tatoeba, Kaikki gibi provider akışlarında aynı standartta log yazmak istiyoruz.
/// - Provider log yazma işini handler/controller içine dağıtmak istemiyoruz.
/// - İleride provider maliyeti, kota, hata oranı ve cache kullanımı analiz edilecek.
/// </summary>
public interface IProviderRequestLogService
{
    /// <summary>
    /// Yeni provider request log kaydı oluşturur.
    /// 
    /// Bu method SaveChanges çağırır.
    /// Çünkü provider log operasyonel/audit kaydıdır ve çağrı tamamlandığında
    /// database'e kalıcı olarak yazılması istenir.
    /// </summary>
    Task<ProviderRequestLog> CreateAsync(
        ProviderRequestLogCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Önceden oluşturulmuş provider log kaydını bir ImportJob ile ilişkilendirir.
    /// 
    /// Bazı akışlarda log önce, job bağlantısı sonra kurulabilir.
    /// </summary>
    Task AttachImportJobAsync(
        Guid providerRequestLogId,
        Guid importJobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Önceden oluşturulmuş provider log kaydını bir LearningItem ile ilişkilendirir.
    /// 
    /// Örnek:
    /// Azure provider çağrısı başarılı oldu ve sonrasında global Word/Phrase oluşturuldu.
    /// Bu durumda log ilgili LearningItem'a bağlanabilir.
    /// </summary>
    Task AttachLearningItemAsync(
        Guid providerRequestLogId,
        Guid learningItemId,
        CancellationToken cancellationToken = default);
}