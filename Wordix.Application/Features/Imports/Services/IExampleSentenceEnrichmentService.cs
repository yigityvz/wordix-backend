using Wordix.Application.Common.Models.Import;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Parser'dan gelen example sentence satırlarını mevcut Word/Phrase LearningItem'larla eşleştirip
/// Sentence, SentenceTranslation ve LearningItemExampleSentence kayıtlarına dönüştürecek service sözleşmesidir.
/// 
/// Bu interface neden Application katmanında?
/// - Example sentence enrichment bir use-case/domain uygulama akışıdır.
/// - Controller bunu doğrudan bilmez.
/// - Infrastructure parser sadece dış dosyayı parse eder.
/// - Persistence sadece veri erişimini yapar.
/// - Bu service ise "hangi cümle hangi Word/Phrase'e bağlanmalı?" kararını yönetir.
/// 
/// Bu interface ne yapmaz?
/// - HTTP dosya upload bilmez.
/// - DbContext bilmez.
/// - Tatoeba TSV/CSV formatı bilmez.
/// - Controller veya Swagger detayı bilmez.
/// </summary>
public interface IExampleSentenceEnrichmentService
{
    /// <summary>
    /// Parser'dan gelen example sentence import satırlarını mevcut LearningItem havuzuyla eşleştirir.
    /// 
    /// DryRun true ise:
    /// - Database'e kayıt atılmaz.
    /// - Sadece kaç Sentence / SentenceTranslation / ExampleLink oluşturulabileceği hesaplanır.
    /// 
    /// DryRun false ise:
    /// - Uygun cümleler Sentence tablosuna eklenir.
    /// - Türkçe çeviriler SentenceTranslation tablosuna eklenir.
    /// - Word/Phrase LearningItem ile Sentence arasında LearningItemExampleSentence bağlantısı oluşturulur.
    /// 
    /// Bu method idempotent çalışacak şekilde tasarlanmalıdır:
    /// - Aynı sentence tekrar eklenmemeli.
    /// - Aynı translation tekrar eklenmemeli.
    /// - Aynı LearningItem + Sentence bağlantısı tekrar oluşturulmamalı.
    /// </summary>
    Task<ExampleSentenceEnrichmentResult> EnrichAsync(
        ExampleSentenceEnrichmentRequest request,
        CancellationToken cancellationToken = default);
}