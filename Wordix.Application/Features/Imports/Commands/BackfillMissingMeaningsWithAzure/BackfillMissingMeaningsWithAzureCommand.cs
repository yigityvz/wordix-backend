using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.BackfillMissingMeaningsWithAzure;

/// <summary>
/// Türkçe meaning'i olmayan aktif Word kayıtlarını Azure Translator ile dolduran command'dır.
/// 
/// Bu command ne yapar?
/// - Meaning'i olmayan Word kayıtlarını bulur.
/// - DryRun ise sadece raporlar.
/// - DryRun false ise Azure Translator ile çevirip Meaning oluşturur.
/// </summary>
public sealed record BackfillMissingMeaningsWithAzureCommand
    : IRequest<AzureMissingMeaningBackfillResponse>
{
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    public string TargetLanguageCode { get; init; } = ImportConstants.LanguageCodes.Turkish;

    public int MaxItems { get; init; } = 100;

    public bool DryRun { get; init; } = true;

    public int BatchSize { get; init; } = 25;

    public int MaxMessages { get; init; } = 100;

    public IReadOnlyCollection<ContentSource> AllowedContentSources { get; init; }
        = new[]
        {
            ContentSource.CefrJ,
            ContentSource.Octanove,
            ContentSource.Manual
        };
}