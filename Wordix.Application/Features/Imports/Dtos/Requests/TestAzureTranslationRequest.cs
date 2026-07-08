using Wordix.Application.Common.Constants;

namespace Wordix.Application.Features.Imports.Dtos.Requests;

/// <summary>
/// Azure Translator smoke test endpoint request modelidir.
/// 
/// Bu DTO neden var?
/// - Controller doğrudan command oluşturmak yerine request DTO alır.
/// - Request DTO -> Command dönüşümü feature mapper içinde yapılır.
/// - DTO/Mapper standardımız korunur.
/// </summary>
public sealed record TestAzureTranslationRequest
{
    /// <summary>
    /// Azure ile çevrilecek metindir.
    /// 
    /// Örnek:
    /// - give up
    /// - abandon
    /// - I abandoned the project.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// Default İngilizce.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef dil kodudur.
    /// Default Türkçe.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Test amacını belirtir.
    /// Log/debug için kullanılabilir.
    /// </summary>
    public string? Purpose { get; init; }
}