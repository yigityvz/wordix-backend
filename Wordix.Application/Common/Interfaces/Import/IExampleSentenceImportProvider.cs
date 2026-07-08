using Wordix.Application.Common.Models.Import;

namespace Wordix.Application.Common.Interfaces.Import;

/// <summary>
/// Dış example sentence kaynaklarından Wordix'in anlayacağı standart örnek cümle satırları üretir.
/// 
/// Bu interface Application katmanındadır.
/// Çünkü Application katmanı sadece ihtiyacı tanımlar:
/// "Bana dış kaynaktan parse edilmiş örnek cümle satırları ver."
/// 
/// Gerçek dosya okuma/parsing implementasyonu Infrastructure katmanında olacaktır.
/// Örneğin:
/// - TatoebaExampleSentenceImportProvider
/// - ileride farklı sentence/example provider'ları
/// </summary>
public interface IExampleSentenceImportProvider
{
    /// <summary>
    /// Verilen source, target ve link stream'lerini okuyarak standart example sentence import satırları üretir.
    /// 
    /// Bu method provider'ın teknik detayını dışarıya sızdırmaz.
    /// Tatoeba provider üç dosya okuyabilir,
    /// başka bir provider tek JSON dosyası okuyabilir,
    /// başka biri API'den veri çekebilir.
    /// 
    /// Application katmanı sadece ExampleSentenceImportProviderResult sonucunu bilir.
    /// </summary>
    Task<ExampleSentenceImportProviderResult> LoadAsync(
        ExampleSentenceImportProviderRequest request,
        CancellationToken cancellationToken = default);
}