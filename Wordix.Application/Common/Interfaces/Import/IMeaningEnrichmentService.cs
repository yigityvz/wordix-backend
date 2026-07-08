using Wordix.Application.Common.Models.Import;

namespace Wordix.Application.Common.Interfaces.Import;

/// <summary>
/// Parser'dan gelen meaning satırlarını database'deki mevcut LearningItem/Word kayıtlarıyla eşleştirip
/// Meaning entity'lerine dönüştüren enrichment servisidir.
/// 
/// Bu interface neden Application katmanında?
/// - Enrichment bir application use-case davranışıdır.
/// - Controller bu işin detayını bilmemelidir.
/// - Handler sadece bu servisi çağırarak işlemi başlatmalıdır.
/// 
/// Implementasyon da Application katmanında olabilir.
/// Çünkü bu servis repository interface'leri üzerinden çalışacak,
/// DbContext veya Infrastructure detaylarını doğrudan bilmeyecek.
/// </summary>
public interface IMeaningEnrichmentService
{
    /// <summary>
    /// Meaning enrichment işlemini çalıştırır.
    /// 
    /// Bu method:
    /// - MeaningImportRow listesini alır.
    /// - DB'de ilgili Word/LearningItem kayıtlarını bulur.
    /// - Existing meaning kontrolü yapar.
    /// - DryRun ise sadece sayım yapar.
    /// - DryRun değilse Meaning entity oluşturup batch save yapar.
    /// </summary>
    Task<MeaningEnrichmentResult> EnrichAsync(
        MeaningEnrichmentRequest request,
        CancellationToken cancellationToken = default);
}