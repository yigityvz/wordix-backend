using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.EnrichKaikkiMeanings;

/// <summary>
/// Kaikki/Wiktionary JSONL dosyasından Türkçe meaningleri parse edip
/// mevcut imported Word/LearningItem kayıtlarına bağlayan command.
/// 
/// Bu command gerçek database enrichment akışını başlatır.
/// DryRun true ise sadece simülasyon yapar.
/// DryRun false ise Meaning entity'leri oluşturulur.
/// </summary>
public sealed record EnrichKaikkiMeaningsCommand : IRequest<EnrichKaikkiMeaningsResponse>
{
    /// <summary>
    /// Upload edilen Kaikki JSONL dosyasının stream'idir.
    /// </summary>
    public Stream? SourceStream { get; init; }

    /// <summary>
    /// Upload edilen dosya adıdır.
    /// Validation/debug için tutulur.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// Wordix imported word pool şu an İngilizce olduğu için default en.
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
    /// Büyük Kaikki dosyalarında ilk aşamada güvenlik için limit kullanıyoruz.
    /// Null verilirse provider tarafında limit uygulanmaz.
    /// </summary>
    public int? MaxRows { get; init; } = 10000;

    /// <summary>
    /// Phrase candidate satırları enrichment'e dahil edilsin mi?
    /// 
    /// Bu fazın ana hedefi Word enrichment olduğu için default false.
    /// Phrase import ayrı fazda yapılacak.
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
}