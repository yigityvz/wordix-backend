using Wordix.Application.Common.Constants;

namespace Wordix.Application.Features.Imports.Dtos.Requests;

/// <summary>
/// Provider-created phrase test endpoint request modelidir.
/// 
/// Bu DTO neyi test eder?
/// - Azure veya başka provider'dan gelmiş gibi phrase metni alır.
/// - Provider'dan gelmiş gibi meaning/translation metni alır.
/// - Bunların global catalog'a kaydedilip kaydedilemediğini test eder.
/// 
/// Bu endpoint kullanıcı dictionary'sine kayıt yapmaz.
/// Sadece global LearningItem + Phrase + Meaning oluşturmayı test eder.
/// </summary>
public sealed record TestProviderPhraseCreationRequest
{
    /// <summary>
    /// Global catalog'a eklenecek phrase metnidir.
    /// 
    /// Örnek:
    /// take care of
    /// give up
    /// look after
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Provider'dan gelmiş gibi kabul edilen Türkçe anlamdır.
    /// 
    /// Örnek:
    /// ilgilenmek
    /// vazgeçmek
    /// bakmak / ilgilenmek
    /// </summary>
    public string MeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// Default İngilizce.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef anlam dil kodudur.
    /// Default Türkçe.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;
}