namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Tatoeba example sentence parser smoke test sonucudur.
/// 
/// Bu response database'e yazılan kayıtları değil,
/// sadece parser'ın ürettiği İngilizce-Türkçe cümle eşleşmelerini raporlar.
/// </summary>
public sealed record ParseTatoebaExampleSentencesResponse
{
    /// <summary>
    /// Provider genel olarak başarılı çalıştı mı?
    /// </summary>
    public bool ProviderSucceeded { get; init; }

    /// <summary>
    /// Parser tarafından üretilen toplam cümle çifti sayısıdır.
    /// </summary>
    public int TotalParsedRows { get; init; }

    /// <summary>
    /// Wordix kaynak dil kodudur.
    /// Örnek: en
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Wordix hedef dil kodudur.
    /// Örnek: tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Provider kaynak dil kodudur.
    /// Örnek: eng
    /// </summary>
    public string SourceProviderLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Provider hedef dil kodudur.
    /// Örnek: tur
    /// </summary>
    public string TargetProviderLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Provider'ın ürettiği hata/uyarı mesajı sayısıdır.
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// API response içinde gösterilecek örnek cümle çiftleridir.
    /// Tüm satırları dönmeyerek Swagger response'unu küçük tutuyoruz.
    /// </summary>
    public IReadOnlyCollection<ParseTatoebaExampleSentenceSampleResponse> SampleRows { get; init; }
        = Array.Empty<ParseTatoebaExampleSentenceSampleResponse>();

    /// <summary>
    /// Provider hata/uyarı mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; }
        = Array.Empty<string>();
}