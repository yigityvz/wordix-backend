using MediatR;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;

namespace Wordix.Application.Features.Imports.Commands.ParseTatoebaExampleSentences;

/// <summary>
/// Tatoeba example sentence parser smoke test handler'ıdır.
/// 
/// Bu handler database'e dokunmaz.
/// Sadece IExampleSentenceImportProvider üzerinden parser'ı çalıştırır
/// ve sonucu response DTO'ya çevirir.
/// 
/// Neden handler içinde parser detayı yok?
/// - Handler use-case akışını yönetir.
/// - Dosya formatı/parsing teknik detayı Infrastructure provider içindedir.
/// - Böylece Clean Architecture ayrımı korunur.
/// </summary>
public sealed class ParseTatoebaExampleSentencesCommandHandler
    : IRequestHandler<ParseTatoebaExampleSentencesCommand, ParseTatoebaExampleSentencesResponse>
{
    private readonly IExampleSentenceImportProvider _exampleSentenceImportProvider;

    public ParseTatoebaExampleSentencesCommandHandler(
        IExampleSentenceImportProvider exampleSentenceImportProvider)
    {
        _exampleSentenceImportProvider = exampleSentenceImportProvider;
    }

    /// <summary>
    /// Tatoeba parser test akışını yürütür.
    /// </summary>
    public async Task<ParseTatoebaExampleSentencesResponse> Handle(
        ParseTatoebaExampleSentencesCommand request,
        CancellationToken cancellationToken)
    {
        var providerRequest = new ExampleSentenceImportProviderRequest
        {
            SourceSentencesStream = request.SourceSentencesStream,
            TargetSentencesStream = request.TargetSentencesStream,
            LinksStream = request.LinksStream,

            SourceLanguageCode = request.SourceLanguageCode,
            TargetLanguageCode = request.TargetLanguageCode,

            SourceProviderLanguageCode = request.SourceProviderLanguageCode,
            TargetProviderLanguageCode = request.TargetProviderLanguageCode,

            MaxRows = request.MaxRows,
            MaxMessages = request.MaxMessages,
            License = request.License
        };

        var providerResult = await _exampleSentenceImportProvider.LoadAsync(
            providerRequest,
            cancellationToken);

        return ParseTatoebaExampleSentencesMapper.ToResponse(
            providerResult,
            request.SampleSize);
    }
}