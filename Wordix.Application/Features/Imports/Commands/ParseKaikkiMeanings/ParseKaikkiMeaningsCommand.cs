using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.ParseKaikkiMeanings;

/// <summary>
/// Kaikki/Wiktionary JSONL meaning parser'ını test etmek için kullanılan command.
/// 
/// Bu command database'e kayıt attırmaz.
/// Sadece provider'ın JSONL dosyasından kaç meaning çıkarabildiğini test eder.
/// </summary>
public sealed record ParseKaikkiMeaningsCommand : IRequest<ParseKaikkiMeaningsResponse>
{
    /// <summary>
    /// Swagger veya API üzerinden upload edilen JSONL dosyasının stream'idir.
    /// </summary>
    public Stream? SourceStream { get; init; }

    /// <summary>
    /// Upload edilen dosya adıdır.
    /// Şu an sadece validation/debug için tutulur.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Kaynak dil kodudur.
    /// Wordix verified word pool İngilizce olduğu için default en.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef meaning dil kodudur.
    /// Wordix için Türkçe anlam istediğimizden default tr.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Parser'ın en fazla kaç meaning row üreteceğini belirler.
    /// Smoke test endpointinde büyük dosyayı komple okumamak için default 100 kullanacağız.
    /// </summary>
    public int? MaxRows { get; init; } = 100;

    /// <summary>
    /// Response içinde kaç sample row gösterileceğini belirler.
    /// </summary>
    public int SampleSize { get; init; } = 20;

    /// <summary>
    /// Phrase/phrasal verb/idiom adayları işaretlensin mi?
    /// </summary>
    public bool IncludePhraseCandidates { get; init; } = true;
}