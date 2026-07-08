using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.TestAzureTranslation;

/// <summary>
/// Azure Translator provider'ını gerçek API çağrısıyla test eden command.
/// 
/// Bu command database'e dokunmaz.
/// Sadece ITranslationProvider üzerinden çeviri yapar.
/// </summary>
public sealed record TestAzureTranslationCommand : IRequest<TestAzureTranslationResponse>
{
    /// <summary>
    /// Çevrilecek metin.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodu.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef dil kodu.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    /// <summary>
    /// Test amacı.
    /// </summary>
    public string? Purpose { get; init; }
}