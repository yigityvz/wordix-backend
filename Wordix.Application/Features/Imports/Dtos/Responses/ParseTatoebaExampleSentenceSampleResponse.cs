namespace Wordix.Application.Features.Imports.Dtos.Responses;

/// <summary>
/// Tatoeba parser testinde kullanıcıya örnek olarak döndürülecek
/// küçük cümle çifti response modelidir.
/// 
/// Neden ayrı sample response var?
/// - ExampleSentenceImportRow provider'ın iç modelidir.
/// - API response içinde provider modelini doğrudan dışarı açmak istemiyoruz.
/// - Kullanıcıya test için gerekli, sade bilgileri dönüyoruz.
/// </summary>
public sealed record ParseTatoebaExampleSentenceSampleResponse
{
    /// <summary>
    /// Dış kaynakta kaynak cümlenin id değeridir.
    /// </summary>
    public string SourceSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Dış kaynakta hedef/çeviri cümlenin id değeridir.
    /// </summary>
    public string TargetSentenceExternalId { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dildeki cümle metnidir.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak cümlenin normalize edilmiş halidir.
    /// </summary>
    public string NormalizedSourceText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dildeki çeviri metnidir.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Çeviri cümlenin normalize edilmiş halidir.
    /// </summary>
    public string NormalizedTranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Provider adıdır.
    /// Örnek: Tatoeba
    /// </summary>
    public string SourceProvider { get; init; } = string.Empty;

    /// <summary>
    /// Dış kaynak takip anahtarıdır.
    /// </summary>
    public string? ExternalSourceKey { get; init; }

    /// <summary>
    /// Source sentences dosyasındaki satır numarasıdır.
    /// </summary>
    public int SourceSentenceRowNumber { get; init; }

    /// <summary>
    /// Target sentences dosyasındaki satır numarasıdır.
    /// </summary>
    public int TargetSentenceRowNumber { get; init; }

    /// <summary>
    /// Links dosyasındaki satır numarasıdır.
    /// </summary>
    public int LinkRowNumber { get; init; }
}