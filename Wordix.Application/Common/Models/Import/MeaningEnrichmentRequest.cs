using Wordix.Application.Common.Constants;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// Meaning enrichment işlemi için kullanılan request modelidir.
/// 
/// Bu model neyi temsil eder?
/// - Kaikki/Wiktionary parser'dan gelen meaning satırlarını alır.
/// - Database'deki mevcut Word/LearningItem kayıtlarıyla eşleştirmek için gerekli ayarları taşır.
/// - DryRun, batch size, kaynak dil, hedef dil gibi enrichment davranışlarını belirler.
/// 
/// Bu model Application katmanındadır çünkü enrichment bir use-case davranışıdır.
/// Controller veya Infrastructure detayı değildir.
/// </summary>
public sealed record MeaningEnrichmentRequest
{
    /// <summary>
    /// Provider/parser tarafından üretilmiş meaning satırlarıdır.
    /// 
    /// Örnek:
    /// abandon -> terk etmek
    /// book -> kitap
    /// give up -> vazgeçmek
    /// 
    /// Bu fazda özellikle mevcut Word kayıtlarıyla eşleşen satırları Meaning tablosuna ekleyeceğiz.
    /// </summary>
    public IReadOnlyCollection<MeaningImportRow> MeaningRows { get; init; }
        = Array.Empty<MeaningImportRow>();

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Şu an imported word pool İngilizce olduğu için default "en".
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef meaning dil kodudur.
    /// 
    /// Wordix için Türkçe anlam hedeflediğimizden default "tr".
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Hangi LearningItem source'larına enrichment uygulanacağını belirtir.
    /// 
    /// Şimdilik sadece verified import havuzunu enrich etmek istiyoruz:
    /// - CefrJ
    /// - Octanove
    /// 
    /// Manual seed veya provider-added içerikleri bu fazda zorunlu olarak enrich etmiyoruz.
    /// </summary>
    public IReadOnlyCollection<ContentSource> AllowedLearningItemSources { get; init; }
        = new[]
        {
            ContentSource.CefrJ,
            ContentSource.Octanove
        };

    /// <summary>
    /// Enrichment işlemi sadece word kayıtlarına mı uygulansın?
    /// 
    /// Şu an 24G'nin ilk hedefi imported Word havuzuna Türkçe anlam bağlamak.
    /// Phrase candidate satırlarını şimdilik meaning enrichment'e dahil etmeyeceğiz.
    /// Phrase import ayrı fazda yapılacak.
    /// </summary>
    public bool IncludePhraseCandidates { get; init; }

    /// <summary>
    /// Mevcut meaning varsa ne yapılacağını belirler.
    /// 
    /// true:
    /// - Eğer aynı LearningItem + hedef dil + meaning text zaten varsa skip edilir.
    /// - Mevcut anlamlar korunur.
    /// 
    /// false:
    /// - İleride merge/update stratejisi eklemek istersek kullanılabilir.
    /// 
    /// İlk sürümde güvenli olan yaklaşım true'dur.
    /// </summary>
    public bool SkipExistingMeanings { get; init; } = true;

    /// <summary>
    /// İşlem gerçek database insert yapmadan simülasyon olarak çalışsın mı?
    /// 
    /// true ise:
    /// - Kaç meaning ekleneceği hesaplanır.
    /// - Database'e kayıt atılmaz.
    /// 
    /// false ise:
    /// - Meaning entity'leri oluşturulup database'e kaydedilir.
    /// </summary>
    public bool DryRun { get; init; } = true;

    /// <summary>
    /// Kaç kayıtlık gruplarla database'e yazılacağını belirtir.
    /// 
    /// Büyük importlarda her satırda SaveChanges çağırmak performans açısından kötüdür.
    /// Bu yüzden batch save kullanacağız.
    /// </summary>
    public int BatchSize { get; init; } = ImportConstants.DefaultBatchSize;

    /// <summary>
    /// Çok büyük response oluşmasını engellemek için maksimum mesaj sayısını belirtir.
    /// 
    /// ImportJob logging gelene kadar response mesajlarını sınırlı tutacağız.
    /// </summary>
    public int MaxMessages { get; init; } = 100;
}