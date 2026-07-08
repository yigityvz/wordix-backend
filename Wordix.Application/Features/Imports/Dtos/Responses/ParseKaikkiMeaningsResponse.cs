namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Kaikki/Wiktionary meaning parser smoke test sonucudur.
/// 
/// Bu response database'e yazılan kayıtları değil,
/// sadece parser'ın ürettiği meaning satırlarını raporlar.
/// </summary>
public sealed record ParseKaikkiMeaningsResponse
{
    /// <summary>
    /// Provider genel olarak başarılı çalıştı mı?
    /// </summary>
    public bool ProviderSucceeded { get; init; }

    /// <summary>
    /// Parser tarafından üretilen toplam meaning satırı sayısıdır.
    /// </summary>
    public int TotalParsedRows { get; init; }

    /// <summary>
    /// Word olarak görünen meaning satırı sayısıdır.
    /// Phrase candidate olmayan satırlar sayılır.
    /// </summary>
    public int WordMeaningCount { get; init; }

    /// <summary>
    /// Phrase/phrasal verb/idiom adayı olan meaning satırı sayısıdır.
    /// </summary>
    public int PhraseCandidateCount { get; init; }

    /// <summary>
    /// Provider'ın ürettiği hata/uyarı mesajı sayısıdır.
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// API response içinde gösterilecek örnek meaning satırlarıdır.
    /// Tüm satırları dönmeyerek Swagger response'unu küçük tutuyoruz.
    /// </summary>
    public IReadOnlyCollection<ParseKaikkiMeaningSampleResponse> SampleRows { get; init; }
        = Array.Empty<ParseKaikkiMeaningSampleResponse>();

    /// <summary>
    /// Provider hata/uyarı mesajlarıdır.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; init; }
        = Array.Empty<string>();
}