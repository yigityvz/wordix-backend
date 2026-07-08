using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Commands.EnrichKaikkiMeanings;
using Wordix.Application.Features.Imports.Commands.ImportCefrWords;
using Wordix.Application.Features.Imports.Commands.ParseKaikkiMeanings;
using Wordix.Application.Features.Imports.Commands.ParseTatoebaExampleSentences;
using Wordix.Application.Features.Imports.Commands.TestAzureTranslation;
using Wordix.Application.Features.Imports.Commands.TestProviderPhraseCreation;
using Wordix.Application.Features.Imports.Dtos.Requests;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Mappers;
using Wordix.Shared.Responses;
using Wordix.Application.Features.Imports.Commands.BackfillMissingMeaningsWithAzure;
using Wordix.Application.Features.Imports.Commands.EnrichFreeDictMeanings;
using Wordix.Application.Features.Imports.Commands.EnrichTatoebaExampleSentences;
using Wordix.Domain.Enums;

namespace Wordix.Api.Controllers;



/// <summary>
/// Import işlemleriyle ilgili endpointleri yöneten controller.
/// 
/// Bu controller ne yapar?
/// - CSV dosyasını HTTP form-data üzerinden alır.
/// - Dosyanın stream'ini Application command modeline aktarır.
/// - MediatR üzerinden import handler'ı çalıştırır.
/// - Import sonucunu standart ApiResponse formatında döner.
/// 
/// Bu controller ne yapmaz?
/// - CSV parse etmez.
/// - CEFR level hesaplamaz.
/// - Database'e kayıt atmaz.
/// - Duplicate kontrolü yapmaz.
/// - Provider logic çalıştırmaz.
/// 
/// Bunların tamamı Application/Infrastructure/Persistence katmanlarında yapılır.
/// </summary>
[ApiController]
[Route("api/imports")]
[Authorize(Policy = "AdminOnly")]
public sealed class ImportsController : ControllerBase
{
    private readonly ISender _sender;

    public ImportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// CEFR/word profile CSV dosyasını global Wordix kelime havuzuna import eder.
    /// 
    /// Endpoint:
    /// POST /api/imports/cefr-words
    /// 
    /// Form-data alanları:
    /// - file: CSV dosyası
    /// - importSource: cefrj veya octanove
    /// - sourceVersion: opsiyonel, örn 1.5 veya 1.0
    /// - sourceLanguageCode: default en
    /// - batchSize: opsiyonel, default 100
    /// - dryRun: true/false
    /// 
    /// Örnek:
    /// importSource = cefrj
    /// file = cefrj-vocabulary-profile-1.5.csv
    /// 
    /// Örnek:
    /// importSource = octanove
    /// file = octanove-vocabulary-profile-c1c2-1.0.csv
    /// 
    /// Güvenlik notu:
    /// Şu an [Authorize] kullanıyoruz.
    /// Production aşamasında bu endpoint admin/panel rolüyle sınırlandırılmalıdır.
    /// Çünkü global sistem verisini değiştirir.
    /// </summary>
    [HttpPost("cefr-words")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ImportCefrWordsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ImportCefrWordsResponse>>> ImportCefrWords(
        IFormFile? file,
        [FromForm] string? importSource,
        [FromForm] string? sourceVersion,
        [FromForm] string? sourceLanguageCode,
        [FromForm] int? batchSize,
        [FromForm] bool dryRun,
        CancellationToken cancellationToken)
    {
        // Dosya stream'i HTTP boundary bilgisidir.
        // CSV parse veya database işlemi burada yapılmaz.
        using var sourceStream = file?.OpenReadStream();

        var command = new ImportCefrWordsCommand
        {
            SourceStream = sourceStream,
            FileName = file?.FileName,

            // Kullanıcı importSource göndermezse CEFR-J default kabul edilir.
            ImportSource = string.IsNullOrWhiteSpace(importSource)
                ? ImportConstants.ImportSources.CefrJ
                : importSource.Trim(),

            SourceVersion = string.IsNullOrWhiteSpace(sourceVersion)
                ? null
                : sourceVersion.Trim(),

            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),

            BatchSize = batchSize,
            DryRun = dryRun
        };

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(ApiResponse<ImportCefrWordsResponse>.Ok(
            data: response,
            message: "CEFR words import completed successfully."));
    }


    /// <summary>
    /// Kaikki/Wiktionary JSONL meaning parser smoke test endpoint'i.
    /// 
    /// Bu endpoint database'e kayıt atmaz.
    /// Sadece upload edilen küçük JSONL dosyasını parser'dan geçirir
    /// ve örnek meaning satırlarını döndürür.
    /// </summary>
    [HttpPost("meanings/kaikki/parse-test")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ParseKaikkiMeaningsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ParseKaikkiMeaningsResponse>>> ParseKaikkiMeanings(
        IFormFile? file,
        [FromForm] string? sourceLanguageCode,
        [FromForm] string? targetLanguageCode,
        [FromForm] int? maxRows,
        [FromForm] int? sampleSize,
        [FromForm] bool? includePhraseCandidates,
        CancellationToken cancellationToken)
    {
        using var sourceStream = file?.OpenReadStream();

        var command = new ParseKaikkiMeaningsCommand
        {
            SourceStream = sourceStream,
            FileName = file?.FileName,
            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),
            TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : targetLanguageCode.Trim(),
            MaxRows = maxRows ?? 100,
            SampleSize = sampleSize ?? 20,
            IncludePhraseCandidates = includePhraseCandidates ?? true
        };

        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<ParseKaikkiMeaningsResponse>.Ok(
            data: response,
            message: "Kaikki meaning parser test completed successfully."));
    }

    /// <summary>
    /// Kaikki/Wiktionary JSONL datasından Türkçe meaningleri parse edip
    /// mevcut imported Word/LearningItem kayıtlarına bağlar.
    /// 
    /// DryRun true ise database'e kayıt atmaz, sadece kaç meaning ekleneceğini hesaplar.
    /// DryRun false ise Meaning kayıtları database'e eklenir.
    /// </summary>
    [HttpPost("meanings/kaikki/enrich")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(500_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500_000_000)]
    [ProducesResponseType(typeof(ApiResponse<EnrichKaikkiMeaningsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrichKaikkiMeaningsResponse>>> EnrichKaikkiMeanings(
        IFormFile? file,
        [FromForm] string? sourceLanguageCode,
        [FromForm] string? targetLanguageCode,
        [FromForm] int? maxRows,
        [FromForm] bool? includePhraseCandidates,
        [FromForm] bool? dryRun,
        [FromForm] int? batchSize,
        [FromForm] int? maxMessages,
        CancellationToken cancellationToken)
    {
        using var sourceStream = file?.OpenReadStream();

        var command = new EnrichKaikkiMeaningsCommand
        {
            SourceStream = sourceStream,
            FileName = file?.FileName,
            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),
            TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : targetLanguageCode.Trim(),
            MaxRows = maxRows ?? 10000,
            IncludePhraseCandidates = includePhraseCandidates ?? false,
            DryRun = dryRun ?? true,
            BatchSize = batchSize ?? ImportConstants.DefaultBatchSize,
            MaxMessages = maxMessages ?? 100
        };

        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<EnrichKaikkiMeaningsResponse>.Ok(
            data: response,
            message: "Kaikki meaning enrichment completed successfully."));
    }

    /// <summary>
    /// Azure Translator provider'ını gerçek API çağrısıyla test eder.
    /// 
    /// Bu endpoint database'e kayıt atmaz.
    /// Sadece Azure provider'ın doğru configure edilip edilmediğini kontrol eder.
    /// </summary>
    [HttpPost("translations/azure/test")]
    [ProducesResponseType(typeof(ApiResponse<TestAzureTranslationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TestAzureTranslationResponse>>> TestAzureTranslation(
        [FromBody] TestAzureTranslationRequest? request,
        CancellationToken cancellationToken)
    {
        var command = TestAzureTranslationMapper.ToCommand(request);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<TestAzureTranslationResponse>.Ok(
            data: response,
            message: "Azure translation test completed successfully."));
    }

    /// <summary>
    /// Provider-created phrase global catalog kayıt akışını test eder.
    /// 
    /// Bu endpoint gerçek database insert yapabilir.
    /// Ama kullanıcı dictionary'sine kayıt yapmaz.
    /// Sadece LearningItem + Phrase + Meaning oluşturma service'ini test eder.
    /// </summary>
    [HttpPost("phrases/provider-create-test")]
    [ProducesResponseType(typeof(ApiResponse<TestProviderPhraseCreationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TestProviderPhraseCreationResponse>>> TestProviderPhraseCreation(
        [FromBody] TestProviderPhraseCreationRequest? request,
        CancellationToken cancellationToken)
    {
        var command = TestProviderPhraseCreationMapper.ToCommand(request);

        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<TestProviderPhraseCreationResponse>.Ok(
            data: response,
            message: "Provider phrase creation test completed successfully."));
    }


    /// <summary>
    /// Tatoeba example sentence parser smoke test endpoint'i.
    /// 
    /// Bu endpoint database'e kayıt atmaz.
    /// Sadece upload edilen source sentences, target sentences ve links dosyalarını parser'dan geçirir.
    /// 
    /// Beklenen dosyalar:
    /// - sourceSentencesFile: eng_sentences.tsv
    /// - targetSentencesFile: tur_sentences.tsv
    /// - linksFile: links.csv veya links.tsv
    /// 
    /// Bu endpoint 24I parser test aşaması içindir.
    /// DB'ye Sentence/SentenceTranslation/LearningItemExampleSentence kayıtları 24J enrichment fazında yapılacaktır.
    /// </summary>
    [HttpPost("examples/tatoeba/parse-test")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ParseTatoebaExampleSentencesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ParseTatoebaExampleSentencesResponse>>> ParseTatoebaExampleSentences(
        IFormFile? sourceSentencesFile,
        IFormFile? targetSentencesFile,
        IFormFile? linksFile,
        [FromForm] string? sourceLanguageCode,
        [FromForm] string? targetLanguageCode,
        [FromForm] string? sourceProviderLanguageCode,
        [FromForm] string? targetProviderLanguageCode,
        [FromForm] int? maxRows,
        [FromForm] int? sampleSize,
        [FromForm] int? maxMessages,
        [FromForm] string? license,
        CancellationToken cancellationToken)
    {
        // Bu stream'ler HTTP upload dosyalarından gelir.
        // Controller dosya parse etmez; sadece stream'i Application command modeline aktarır.
        using var sourceSentencesStream = sourceSentencesFile?.OpenReadStream();
        using var targetSentencesStream = targetSentencesFile?.OpenReadStream();
        using var linksStream = linksFile?.OpenReadStream();

        var command = new ParseTatoebaExampleSentencesCommand
        {
            SourceSentencesStream = sourceSentencesStream,
            TargetSentencesStream = targetSentencesStream,
            LinksStream = linksStream,

            SourceFileName = sourceSentencesFile?.FileName,
            TargetFileName = targetSentencesFile?.FileName,
            LinksFileName = linksFile?.FileName,

            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),

            TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : targetLanguageCode.Trim(),

            SourceProviderLanguageCode = string.IsNullOrWhiteSpace(sourceProviderLanguageCode)
                ? "eng"
                : sourceProviderLanguageCode.Trim(),

            TargetProviderLanguageCode = string.IsNullOrWhiteSpace(targetProviderLanguageCode)
                ? "tur"
                : targetProviderLanguageCode.Trim(),

            MaxRows = maxRows ?? 100,
            SampleSize = sampleSize ?? 20,
            MaxMessages = maxMessages ?? 100,

            License = string.IsNullOrWhiteSpace(license)
                ? null
                : license.Trim()
        };

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(ApiResponse<ParseTatoebaExampleSentencesResponse>.Ok(
            data: response,
            message: "Tatoeba example sentence parser test completed successfully."));
    }


    /// <summary>
    /// Tatoeba example sentence dosyalarını parse edip mevcut Word/Phrase LearningItem'larla eşleştirir.
    /// 
    /// Endpoint:
    /// POST /api/imports/examples/tatoeba/enrich
    /// 
    /// Bu endpoint iki modda çalışır:
    /// - dryRun = true  => DB'ye kayıt atmaz, sadece kaç kayıt oluşturulabileceğini raporlar.
    /// - dryRun = false => Sentence, SentenceTranslation ve LearningItemExampleSentence kayıtları oluşturabilir.
    /// 
    /// Beklenen dosyalar:
    /// - sourceSentencesFile: eng_sentences.tsv
    /// - targetSentencesFile: tur_sentences.tsv
    /// - linksFile: links.csv veya links.tsv
    /// 
    /// Production notu:
    /// Bu endpoint normal kullanıcı endpointi değildir.
    /// Admin/import operasyonları içindir.
    /// 24K sonrası bu akış ImportJob/batch processing yapısına taşınacaktır.
    /// </summary>
    [HttpPost("examples/tatoeba/enrich")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<EnrichTatoebaExampleSentencesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrichTatoebaExampleSentencesResponse>>> EnrichTatoebaExampleSentences(
        IFormFile? sourceSentencesFile,
        IFormFile? targetSentencesFile,
        IFormFile? linksFile,
        [FromForm] string? sourceLanguageCode,
        [FromForm] string? targetLanguageCode,
        [FromForm] string? sourceProviderLanguageCode,
        [FromForm] string? targetProviderLanguageCode,
        [FromForm] int? parserMaxRows,
        [FromForm] bool? dryRun,
        [FromForm] int? maxExamplesPerLearningItem,
        [FromForm] int? maxCreatedItems,
        [FromForm] string? allowedItemTypes,
        [FromForm] string? allowedContentSources,
        [FromForm] int? sampleSize,
        [FromForm] int? maxMessages,
        [FromForm] string? license,
        CancellationToken cancellationToken)
    {
        // Controller dosya parse etmez.
        // Sadece HTTP form-data ile gelen dosyaları stream'e çevirip command'e aktarır.
        using var sourceSentencesStream = sourceSentencesFile?.OpenReadStream();
        using var targetSentencesStream = targetSentencesFile?.OpenReadStream();
        using var linksStream = linksFile?.OpenReadStream();

        var command = new EnrichTatoebaExampleSentencesCommand
        {
            SourceSentencesStream = sourceSentencesStream,
            TargetSentencesStream = targetSentencesStream,
            LinksStream = linksStream,

            SourceFileName = sourceSentencesFile?.FileName,
            TargetFileName = targetSentencesFile?.FileName,
            LinksFileName = linksFile?.FileName,

            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),

            TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : targetLanguageCode.Trim(),

            SourceProviderLanguageCode = string.IsNullOrWhiteSpace(sourceProviderLanguageCode)
                ? "eng"
                : sourceProviderLanguageCode.Trim(),

            TargetProviderLanguageCode = string.IsNullOrWhiteSpace(targetProviderLanguageCode)
                ? "tur"
                : targetProviderLanguageCode.Trim(),

            ParserMaxRows = parserMaxRows ?? 1000,
            DryRun = dryRun ?? true,

            MaxExamplesPerLearningItem = maxExamplesPerLearningItem ?? 3,
            MaxCreatedItems = maxCreatedItems,

            AllowedItemTypes = ParseAllowedItemTypes(allowedItemTypes),
            AllowedContentSources = ParseAllowedContentSources(allowedContentSources),

            SampleSize = sampleSize ?? 20,
            MaxMessages = maxMessages ?? 100,

            License = string.IsNullOrWhiteSpace(license)
                ? null
                : license.Trim()
        };

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(ApiResponse<EnrichTatoebaExampleSentencesResponse>.Ok(
            data: response,
            message: "Tatoeba example sentence enrichment completed successfully."));
    }


    /// <summary>
    /// FreeDict English-Turkish TEI XML datasından Türkçe meaningleri parse edip
    /// mevcut imported Word/LearningItem kayıtlarına bağlar.
    /// 
    /// DryRun true ise database'e kayıt atmaz, sadece kaç meaning ekleneceğini hesaplar.
    /// DryRun false ise Meaning kayıtları database'e eklenir.
    /// </summary>
    [HttpPost("meanings/freedict/enrich")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
    [ProducesResponseType(typeof(ApiResponse<EnrichFreeDictMeaningsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EnrichFreeDictMeaningsResponse>>> EnrichFreeDictMeanings(
        IFormFile? file,
        [FromForm] string? sourceLanguageCode,
        [FromForm] string? targetLanguageCode,
        [FromForm] int? maxRows,
        [FromForm] bool? includePhraseCandidates,
        [FromForm] bool? dryRun,
        [FromForm] int? batchSize,
        [FromForm] int? maxMessages,
        CancellationToken cancellationToken)
    {
        using var sourceStream = file?.OpenReadStream();

        var command = new EnrichFreeDictMeaningsCommand
        {
            SourceStream = sourceStream,
            FileName = file?.FileName,

            SourceLanguageCode = string.IsNullOrWhiteSpace(sourceLanguageCode)
                ? ImportConstants.LanguageCodes.English
                : sourceLanguageCode.Trim(),

            TargetLanguageCode = string.IsNullOrWhiteSpace(targetLanguageCode)
                ? ImportConstants.LanguageCodes.Turkish
                : targetLanguageCode.Trim(),

            MaxRows = maxRows ?? 10000,
            IncludePhraseCandidates = includePhraseCandidates ?? false,
            DryRun = dryRun ?? true,
            BatchSize = batchSize ?? ImportConstants.DefaultBatchSize,
            MaxMessages = maxMessages ?? 100,

            // FreeDict meaning'lerini verified/imported sistem kelimelerine bağlamak istiyoruz.
            // Azure/UserLookup içerikleri burada hedef havuz değildir.
            AllowedLearningItemSources = new[]
            {
            ContentSource.CefrJ,
            ContentSource.Octanove,
            ContentSource.Manual
        }
        };

        var response = await _sender.Send(command, cancellationToken);

        return Ok(ApiResponse<EnrichFreeDictMeaningsResponse>.Ok(
            data: response,
            message: "FreeDict meaning enrichment completed successfully."));
    }


    /// <summary>
    /// Türkçe meaning'i olmayan aktif Word kayıtlarını Azure Translator ile doldurur.
    /// 
    /// DryRun true ise Azure'a istek atmaz, DB'ye kayıt atmaz.
    /// DryRun false ise Azure Translator ile çeviri yapıp Meaning oluşturur.
    /// </summary>
    [HttpPost("meanings/missing/azure-backfill")]
    [ProducesResponseType(typeof(ApiResponse<AzureMissingMeaningBackfillResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AzureMissingMeaningBackfillResponse>>> BackfillMissingMeaningsWithAzure(
        [FromBody] AzureMissingMeaningBackfillRequest? request,
        CancellationToken cancellationToken)
    {
        var command = AzureMissingMeaningBackfillMapper.ToCommand(request);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(ApiResponse<AzureMissingMeaningBackfillResponse>.Ok(
            data: response,
            message: "Azure missing meaning backfill completed successfully."));
    }

    /// <summary>
    /// Form-data ile gelen allowedItemTypes değerini domain enum listesine çevirir.
    /// 
    /// Örnek input:
    /// Word,Phrase
    /// 
    /// Boş gelirse güvenli default kullanılır:
    /// Word + Phrase
    /// </summary>
    private static IReadOnlyCollection<LearningItemType> ParseAllowedItemTypes(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new[]
            {
            LearningItemType.Word,
            LearningItemType.Phrase
        };
        }

        var parsedValues = value
            .Split(
                new[] { ',', ';', '|' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => Enum.TryParse<LearningItemType>(
                item,
                ignoreCase: true,
                out var parsed)
                    ? parsed
                    : LearningItemType.Unknown)
            .Where(itemType => itemType is LearningItemType.Word or LearningItemType.Phrase)
            .Distinct()
            .ToArray();

        return parsedValues.Length == 0
            ? new[]
            {
            LearningItemType.Word,
            LearningItemType.Phrase
            }
            : parsedValues;
    }

    /// <summary>
    /// Form-data ile gelen allowedContentSources değerini domain enum listesine çevirir.
    /// 
    /// Örnek input:
    /// CefrJ,Octanove,WiktionaryKaikki,Manual
    /// 
    /// Boş gelirse command/service güvenli default kullanır.
    /// Bu yüzden boş durumda empty array dönüyoruz.
    /// </summary>
    private static IReadOnlyCollection<ContentSource> ParseAllowedContentSources(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<ContentSource>();
        }

        return value
            .Split(
                new[] { ',', ';', '|' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => Enum.TryParse<ContentSource>(
                item,
                ignoreCase: true,
                out var parsed)
                    ? parsed
                    : ContentSource.Unknown)
            .Where(contentSource => contentSource != ContentSource.Unknown)
            .Distinct()
            .ToArray();
    }

}