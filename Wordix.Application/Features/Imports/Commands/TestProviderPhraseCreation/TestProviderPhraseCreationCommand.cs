using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.TestProviderPhraseCreation;

/// <summary>
/// Provider-created phrase oluşturma test command'idir.
/// 
/// Bu command database'e LearningItem + Phrase + Meaning oluşturabilir.
/// Bu yüzden sadece admin-only test endpointinden çağrılmalıdır.
/// </summary>
public sealed record TestProviderPhraseCreationCommand
    : IRequest<TestProviderPhraseCreationResponse>
{
    /// <summary>
    /// Phrase metni.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Türkçe anlam metni.
    /// </summary>
    public string MeaningText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodu.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Hedef dil kodu.
    /// </summary>
    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;
}