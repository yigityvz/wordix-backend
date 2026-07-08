using Wordix.Application.Common.Models.Import;

namespace Wordix.Application.Common.Interfaces.Import;

/// <summary>
/// Dış meaning kaynaklarından Wordix'in anlayacağı standart meaning satırları üretir.
/// 
/// Bu interface Application katmanındadır.
/// Çünkü Application katmanı "bana meaning import satırları üret" ihtiyacını tanımlar.
/// 
/// Gerçek implementasyon Infrastructure katmanında olacaktır.
/// Örneğin:
/// - KaikkiMeaningImportProvider
/// - ileride farklı dictionary provider'ları
/// </summary>
public interface IMeaningImportProvider
{
    /// <summary>
    /// Verilen kaynak stream'i okuyarak standart meaning import satırları üretir.
    /// 
    /// Bu method provider'ın teknik detaylarını dışarıya sızdırmaz.
    /// Kaikki provider JSONL okuyabilir,
    /// başka bir provider JSON okuyabilir,
    /// başka biri API'den veri çekebilir.
    /// 
    /// Application katmanı sadece MeaningImportProviderResult sonucunu bilir.
    /// </summary>
    Task<MeaningImportProviderResult> LoadAsync(
        MeaningImportProviderRequest request,
        CancellationToken cancellationToken = default);
}