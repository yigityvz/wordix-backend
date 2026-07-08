using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.ParseTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence parser'ını test etmek için kullanılan command modelidir.
/// 
/// Bu command database'e kayıt attırmaz.
/// Sadece üç Tatoeba dosyasını parser'a gönderir:
/// - source sentences dosyası
/// - target sentences dosyası
/// - links dosyası
/// 
/// Parser sonucu İngilizce-Türkçe cümle çiftleri olarak response'a döner.
/// </summary>
public sealed record ParseTatoebaExampleSentencesCommand
    : IRequest<ParseTatoebaExampleSentencesResponse>
{
    /// <summary>
    /// Kaynak dil cümle dosyası stream'idir.
    /// 
    /// Örnek:
    /// eng_sentences.tsv
    /// </summary>
    public Stream? SourceSentencesStream { get; init; }

    /// <summary>
    /// Hedef dil cümle dosyası stream'idir.
    /// 
    /// Örnek:
    /// tur_sentences.tsv
    /// </summary>
    public Stream? TargetSentencesStream { get; init; }

    /// <summary>
    /// Source ve target sentence id değerlerini bağlayan link dosyası stream'idir.
    /// 
    /// Örnek:
    /// links.csv
    /// </summary>
    public Stream? LinksStream { get; init; }

    /// <summary>
    /// Upload edilen kaynak cümle dosyasının adıdır.
    /// Sadece validation/debug için tutulur.
    /// </summary>
    public string? SourceFileName { get; init; }

    /// <summary>
    /// Upload edilen hedef cümle dosyasının adıdır.
    /// Sadece validation/debug için tutulur.
    /// </summary>
    public string? TargetFileName { get; init; }

    /// <summary>
    /// Upload edilen link dosyasının adıdır.
    /// Sadece validation/debug için tutulur.
    /// </summary>
    public string? LinksFileName { get; init; }

    /// <summary>
    /// Wordix içindeki kaynak dil kodudur.
    /// 
    /// Wordix tarafında İngilizce "en" olarak tutulur.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Wordix içindeki hedef dil kodudur.
    /// 
    /// Wordix tarafında Türkçe "tr" olarak tutulur.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Tatoeba dosyasındaki kaynak dil kodudur.
    /// 
    /// Tatoeba İngilizce için genellikle "eng" kullanır.
    /// </summary>
    public string SourceProviderLanguageCode { get; init; } = "eng";

    /// <summary>
    /// Tatoeba dosyasındaki hedef dil kodudur.
    /// 
    /// Tatoeba Türkçe için genellikle "tur" kullanır.
    /// </summary>
    public string TargetProviderLanguageCode { get; init; } = "tur";

    /// <summary>
    /// Parser'ın en fazla kaç eşleşmiş cümle çifti döndüreceğini belirler.
    /// 
    /// Parse-test endpointinde büyük dosyayı komple response'a dökmemek için default 100 kullanacağız.
    /// </summary>
    public int? MaxRows { get; init; } = 100;

    /// <summary>
    /// Response içinde kaç sample row gösterileceğini belirler.
    /// </summary>
    public int SampleSize { get; init; } = 20;

    /// <summary>
    /// Provider hata/uyarı mesajlarından en fazla kaç tanesinin response'a alınacağını belirler.
    /// </summary>
    public int MaxMessages { get; init; } = 100;

    /// <summary>
    /// Kaynak lisans/attribution bilgisidir.
    /// 
    /// Tatoeba için ileride DB kayıtlarına taşınabilir.
    /// Bu parse-test aşamasında sadece row modeline geçirilir.
    /// </summary>
    public string? License { get; init; }
}