using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Models.Translation;
using Wordix.Application.Features.Imports.Commands.TestAzureTranslation;
using Wordix.Application.Features.Imports.Dtos.Requests;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Azure translation test endpoint mapping işlemlerini yapar.
/// 
/// Neden mapper var?
/// - Controller içinde request -> command mapping yazmayız.
/// - Handler içinde result -> response mapping dağınık durmaz.
/// - Feature-based explicit mapper standardımız korunur.
/// </summary>
public static class TestAzureTranslationMapper
{
    /// <summary>
    /// API request DTO'sunu command modeline çevirir.
    /// </summary>
    public static TestAzureTranslationCommand ToCommand(
        TestAzureTranslationRequest? request)
    {
        return new TestAzureTranslationCommand
        {
            Text = request?.Text?.Trim() ?? string.Empty,
            SourceLanguageCode = string.IsNullOrWhiteSpace(request?.SourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : request.SourceLanguageCode.Trim(),
            TargetLanguageCode = string.IsNullOrWhiteSpace(request?.TargetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : request.TargetLanguageCode.Trim(),
            Purpose = string.IsNullOrWhiteSpace(request?.Purpose)
                ? "azure-translation-smoke-test"
                : request.Purpose.Trim()
        };
    }

    /// <summary>
    /// TranslationResult modelini API response DTO'suna çevirir.
    /// </summary>
    public static TestAzureTranslationResponse ToResponse(
        TranslationResult result)
    {
        return new TestAzureTranslationResponse
        {
            Succeeded = result.Succeeded,
            SourceText = result.SourceText,
            TranslatedText = result.TranslatedText,
            NormalizedTranslatedText = result.NormalizedTranslatedText,
            SourceLanguageCode = result.SourceLanguageCode,
            TargetLanguageCode = result.TargetLanguageCode,
            ContentSource = result.ContentSource.ToString(),
            QualityStatus = result.QualityStatus.ToString(),
            SourceProvider = result.SourceProvider,
            CharacterCount = result.CharacterCount,
            ErrorMessage = result.ErrorMessage
        };
    }
}