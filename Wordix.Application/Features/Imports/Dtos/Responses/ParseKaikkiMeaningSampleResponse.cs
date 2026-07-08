namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Kaikki meaning parser testinde kullanıcıya örnek olarak döndürülecek
/// küçük meaning satırını temsil eder.
/// 
/// Neden ayrı sample response var?
/// - MeaningImportRow provider'ın iç modelidir.
/// - API response içinde provider modelini doğrudan dışarı açmak istemiyoruz.
/// - Kullanıcıya test için gerekli, sade bilgileri dönüyoruz.
/// </summary>
public sealed record ParseKaikkiMeaningSampleResponse
{
    /// <summary>
    /// Kaikki/Wiktionary entry'sinden gelen İngilizce kelime veya ifadedir.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak metnin normalize edilmiş halidir.
    /// DB eşleştirme ileride bu alan üzerinden yapılacak.
    /// </summary>
    public string NormalizedSourceText { get; init; } = string.Empty;

    /// <summary>
    /// Türkçe anlam metnidir.
    /// </summary>
    public string MeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Anlam metninin normalize edilmiş halidir.
    /// Duplicate kontrolü için önemlidir.
    /// </summary>
    public string NormalizedMeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Kaikki sense/gloss bilgisinden gelen kısa açıklamadır.
    /// Her kayıtta olmayabilir.
    /// </summary>
    public string? ShortDefinition { get; init; }

    /// <summary>
    /// Kelime türüdür.
    /// Örnek: noun, verb, adjective, adverb.
    /// </summary>
    public string? PartOfSpeech { get; init; }

    /// <summary>
    /// Bu kayıt phrase/phrasal verb/idiom adayı mı?
    /// </summary>
    public bool IsPhraseCandidate { get; init; }

    /// <summary>
    /// Ham JSONL dosyasında kaçıncı satırdan geldiğini gösterir.
    /// Debug ve import logging için önemlidir.
    /// </summary>
    public int SourceRowNumber { get; init; }

    /// <summary>
    /// Aynı entry içindeki kaçıncı translation'dan geldiğini gösterir.
    /// </summary>
    public int? TranslationIndex { get; init; }

    /// <summary>
    /// Dış kaynak takip anahtarıdır.
    /// Örnek: kaikki:line-1:translation-1:abandon
    /// </summary>
    public string? ExternalSourceKey { get; init; }
}