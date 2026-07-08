using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.EnrichFreeDictMeanings;

/// <summary>
/// FreeDict English-Turkish TEI XML dosyasından Türkçe meaningleri parse edip
/// mevcut imported Word/LearningItem kayıtlarına bağlayan command.
/// 
/// Bu command gerçek database enrichment akışını başlatır.
/// DryRun true ise sadece simülasyon yapar.
/// DryRun false ise Meaning entity'leri oluşturulur.
/// </summary>
public sealed record EnrichFreeDictMeaningsCommand : IRequest<EnrichFreeDictMeaningsResponse>
{
    /// <summary>
    /// Upload edilen FreeDict TEI XML dosyasının stream'idir.
    /// </summary>
    public Stream? SourceStream { get; init; }

    /// <summary>
    /// Upload edilen dosya adıdır.
    /// Validation/debug için tutulur.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// FreeDict eng-tur dosyası İngilizce kaynaklı olduğu için default en.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef meaning dil kodudur.
    /// Wordix için Türkçe anlam istediğimizden default tr.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Parser'ın en fazla kaç meaning row üreteceğini belirtir.
    /// 
    /// İlk testlerde kontrollü çalışmak için MaxRows kullanıyoruz.
    /// Null verilirse provider tarafında limit uygulanmaz.
    /// </summary>
    public int? MaxRows { get; init; } = 10000;

    /// <summary>
    /// Phrase candidate satırları enrichment'e dahil edilsin mi?
    /// 
    /// Bu fazın ana hedefi Word enrichment olduğu için default false.
    /// </summary>
    public bool IncludePhraseCandidates { get; init; }

    /// <summary>
    /// İşlem database'e yazmadan simülasyon olarak mı çalışsın?
    /// 
    /// Güvenlik için default true.
    /// Önce dryRun ile sonucu görüp sonra false yapılmalı.
    /// </summary>
    public bool DryRun { get; init; } = true;

    /// <summary>
    /// Kaç meaning entity biriktikten sonra SaveChanges yapılacağını belirtir.
    /// </summary>
    public int BatchSize { get; init; } = ImportConstants.DefaultBatchSize;

    /// <summary>
    /// Response içindeki maksimum mesaj sayısıdır.
    /// </summary>
    public int MaxMessages { get; init; } = 100;

    /// <summary>
    /// FreeDict meaning'lerinin hangi LearningItem kaynaklarına bağlanabileceğini belirtir.
    /// 
    /// Burada AzureTranslator yoktur.
    /// Çünkü Azure/UserLookup içerikleri sistem öneri havuzu için kullanılmayacak.
    /// </summary>
    public IReadOnlyCollection<ContentSource> AllowedLearningItemSources { get; init; }
        = new[]
        {
            ContentSource.CefrJ,
            ContentSource.Octanove,
            ContentSource.Manual
        };
}