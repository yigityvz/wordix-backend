using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.EnrichTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence verisini parse edip mevcut Word/Phrase LearningItem'larla eşleştiren command modelidir.
/// 
/// Bu command ne yapar?
/// - Source sentences dosyasını alır.
/// - Target sentences dosyasını alır.
/// - Links dosyasını alır.
/// - Önce Tatoeba parser'ını çalıştırır.
/// - Sonra ExampleSentenceEnrichmentService ile eşleşen cümleleri DB'ye bağlar.
/// 
/// DryRun true ise:
/// - Database'e kayıt atılmaz.
/// - Sadece kaç kayıt oluşturulabileceği hesaplanır.
/// 
/// DryRun false ise:
/// - Sentence oluşturulabilir.
/// - SentenceTranslation oluşturulabilir.
/// - LearningItemExampleSentence bağlantısı oluşturulabilir.
/// </summary>
public sealed record EnrichTatoebaExampleSentencesCommand
    : IRequest<EnrichTatoebaExampleSentencesResponse>
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
    /// Örnek:
    /// en
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Wordix içindeki hedef dil kodudur.
    /// 
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Tatoeba dosyasındaki kaynak dil kodudur.
    /// 
    /// Örnek:
    /// eng
    /// </summary>
    public string SourceProviderLanguageCode { get; init; } = "eng";

    /// <summary>
    /// Tatoeba dosyasındaki hedef dil kodudur.
    /// 
    /// Örnek:
    /// tur
    /// </summary>
    public string TargetProviderLanguageCode { get; init; } = "tur";

    /// <summary>
    /// Parser'ın en fazla kaç eşleşmiş Tatoeba row üreteceğini belirtir.
    /// 
    /// Bu limit parser aşamasındadır.
    /// Yani önce kaç Tatoeba cümle çifti okunacağını sınırlar.
    /// </summary>
    public int? ParserMaxRows { get; init; } = 1000;

    /// <summary>
    /// True ise database'e kayıt atılmaz.
    /// False ise uygun eşleşmeler database'e yazılır.
    /// </summary>
    public bool DryRun { get; init; } = true;

    /// <summary>
    /// Bir LearningItem için en fazla kaç örnek cümle bağlanacağını belirtir.
    /// </summary>
    public int MaxExamplesPerLearningItem { get; init; } = 3;

    /// <summary>
    /// Toplamda en fazla kaç example link oluşturulabileceğini belirtir.
    /// 
    /// Null ise service sadece MaxExamplesPerLearningItem limitine göre ilerler.
    /// </summary>
    public int? MaxCreatedItems { get; init; }

    /// <summary>
    /// Hangi LearningItem tipleri için enrichment yapılacağını belirtir.
    /// 
    /// İlk aşamada Word ve Phrase destekliyoruz.
    /// </summary>
    public IReadOnlyCollection<LearningItemType> AllowedItemTypes { get; init; }
        = new[]
        {
            LearningItemType.Word,
            LearningItemType.Phrase
        };

    /// <summary>
    /// Hangi ContentSource'lardan gelen Word/Phrase içeriklerin zenginleştirileceğini belirtir.
    /// 
    /// Boş bırakılırsa service güvenli default kullanır:
    /// Manual, CefrJ, Octanove, WiktionaryKaikki.
    /// </summary>
    public IReadOnlyCollection<ContentSource> AllowedContentSources { get; init; }
        = Array.Empty<ContentSource>();

    /// <summary>
    /// Response içinde kaç sample item gösterileceğini belirtir.
    /// </summary>
    public int SampleSize { get; init; } = 20;

    /// <summary>
    /// Provider/enrichment mesajlarından en fazla kaç tanesinin response'a alınacağını belirtir.
    /// </summary>
    public int MaxMessages { get; init; } = 100;

    /// <summary>
    /// Kaynak lisans/attribution bilgisidir.
    /// Tatoeba verisi için ileride DB kayıtlarına taşınabilir.
    /// </summary>
    public string? License { get; init; }
}