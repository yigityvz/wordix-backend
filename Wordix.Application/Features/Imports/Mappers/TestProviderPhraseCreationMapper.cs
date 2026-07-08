using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Commands.TestProviderPhraseCreation;
using Wordix.Application.Features.Imports.Dtos.Requests;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Lookups.Models;

namespace Wordix.Application.Features.Imports.Mappers;

/// <summary>
/// Provider-created phrase test endpoint mapping işlemlerini yapar.
/// 
/// Neden mapper var?
/// - Controller içinde request -> command mapping yazmayız.
/// - Handler veya controller içinde result -> response mapping dağıtmayız.
/// - Feature-based explicit mapper standardımıza uyar.
/// </summary>
public static class TestProviderPhraseCreationMapper
{
    /// <summary>
    /// API request DTO'sunu command modeline çevirir.
    /// </summary>
    public static TestProviderPhraseCreationCommand ToCommand(
        TestProviderPhraseCreationRequest? request)
    {
        return new TestProviderPhraseCreationCommand
        {
            Text = request?.Text?.Trim() ?? string.Empty,
            MeaningText = request?.MeaningText?.Trim() ?? string.Empty,
            SourceLanguageCode = string.IsNullOrWhiteSpace(request?.SourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : request.SourceLanguageCode.Trim(),
            TargetLanguageCode = string.IsNullOrWhiteSpace(request?.TargetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : request.TargetLanguageCode.Trim()
        };
    }

    /// <summary>
    /// Service result modelini API response DTO'suna çevirir.
    /// </summary>
    public static TestProviderPhraseCreationResponse ToResponse(
        ProviderPhraseCreationResult result)
    {
        return new TestProviderPhraseCreationResponse
        {
            Succeeded = result.Succeeded,
            WasCreated = result.WasCreated,
            AlreadyExists = result.AlreadyExists,
            LearningItemId = result.LearningItemId,
            PhraseId = result.PhraseId,
            MeaningId = result.MeaningId,
            NormalizedText = result.NormalizedText,
            Message = result.Message
        };
    }
}