using MediatR;
using Wordix.Application.Common.Interfaces.Translation;
using Wordix.Application.Common.Models.Translation;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;

namespace Wordix.Application.Features.Imports.Commands.TestAzureTranslation;

/// <summary>
/// Azure Translator smoke test command handler'ı.
/// 
/// Bu handler:
/// - Command içindeki metni TranslationRequest'e çevirir.
/// - ITranslationProvider üzerinden Azure provider'ı çağırır.
/// - TranslationResult sonucunu response DTO'ya map eder.
/// </summary>
public sealed class TestAzureTranslationCommandHandler
    : IRequestHandler<TestAzureTranslationCommand, TestAzureTranslationResponse>
{
    private readonly ITranslationProvider _translationProvider;

    public TestAzureTranslationCommandHandler(
        ITranslationProvider translationProvider)
    {
        _translationProvider = translationProvider;
    }

    /// <summary>
    /// Azure çeviri testini çalıştırır.
    /// </summary>
    public async Task<TestAzureTranslationResponse> Handle(
        TestAzureTranslationCommand request,
        CancellationToken cancellationToken)
    {
        var translationRequest = new TranslationRequest
        {
            Text = request.Text,
            SourceLanguageCode = request.SourceLanguageCode,
            TargetLanguageCode = request.TargetLanguageCode,
            Purpose = request.Purpose,
            Context = "Azure Translator smoke test. Return a natural Turkish translation."
        };

        var translationResult = await _translationProvider.TranslateAsync(
            translationRequest,
            cancellationToken);

        return TestAzureTranslationMapper.ToResponse(translationResult);
    }
}