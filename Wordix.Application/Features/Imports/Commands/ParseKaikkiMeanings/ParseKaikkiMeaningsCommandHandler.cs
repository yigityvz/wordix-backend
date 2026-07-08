using MediatR;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;

namespace Wordix.Application.Features.Imports.Commands.ParseKaikkiMeanings;

/// <summary>
/// Kaikki/Wiktionary meaning parser smoke test handler'ı.
/// 
/// Bu handler database'e dokunmaz.
/// Sadece IMeaningImportProvider üzerinden JSONL parser'ı çalıştırır
/// ve sonucu response DTO'ya çevirir.
/// </summary>
public sealed class ParseKaikkiMeaningsCommandHandler
    : IRequestHandler<ParseKaikkiMeaningsCommand, ParseKaikkiMeaningsResponse>
{
    private readonly IMeaningImportProvider _meaningImportProvider;

    public ParseKaikkiMeaningsCommandHandler(
        IMeaningImportProvider meaningImportProvider)
    {
        _meaningImportProvider = meaningImportProvider;
    }

    /// <summary>
    /// Kaikki meaning parser test akışını yürütür.
    /// </summary>
    public async Task<ParseKaikkiMeaningsResponse> Handle(
        ParseKaikkiMeaningsCommand request,
        CancellationToken cancellationToken)
    {
        var providerRequest = new MeaningImportProviderRequest
        {
            SourceStream = request.SourceStream,
            SourceLanguageCode = request.SourceLanguageCode,
            TargetLanguageCode = request.TargetLanguageCode,
            MaxRows = request.MaxRows,
            IncludePhraseCandidates = request.IncludePhraseCandidates
        };

        var providerResult = await _meaningImportProvider.LoadAsync(
            providerRequest,
            cancellationToken);

        return ParseKaikkiMeaningsMapper.ToResponse(
            providerResult,
            request.SampleSize);
    }
}