using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Features.Imports.Dtos.Responses;
using Wordix.Application.Features.Imports.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Commands.ImportCefrWords;

/// <summary>
/// CEFR/word profile kelime listesini Wordix global kelime havuzuna import eden handler'dır.
/// 
/// Bu handler ne yapar?
/// 1. CSV stream'ini ICefrWordListProvider'a verir.
/// 2. Provider'dan standart CefrWordImportRow listesi alır.
/// 3. Her kelime için duplicate kontrolü yapar.
/// 4. Yeni kelimeler için LearningItem + Word oluşturur.
/// 5. Değişiklikleri batch batch database'e kaydeder.
/// 6. ImportJob tablosu üzerinden import operasyonunu takip eder.
/// 
/// Bu handler ne yapmaz?
/// - CSV parse etmez.
/// - Dosya formatı bilmez.
/// - DbContext kullanmaz.
/// - Türkçe anlam import etmez.
/// - Örnek cümle import etmez.
/// </summary>
public sealed class ImportCefrWordsCommandHandler
    : IRequestHandler<ImportCefrWordsCommand, ImportCefrWordsResponse>
{
    /// <summary>
    /// Word.Text ve Word.NormalizedText alanları EF configuration tarafında 200 karakter ile sınırlandı.
    /// Import sırasında daha uzun değerleri database hatası almadan önce eleyebilmek için burada da kontrol ediyoruz.
    /// </summary>
    private const int MaxWordTextLength = 200;

    private readonly ICefrWordListProvider _cefrWordListProvider;
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IRepository<LearningItem> _learningItemGenericRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImportJobService _importJobService;
    private readonly ICurrentUserService _currentUserService;

    public ImportCefrWordsCommandHandler(
        ICefrWordListProvider cefrWordListProvider,
        ILearningItemRepository learningItemRepository,
        IRepository<LearningItem> learningItemGenericRepository,
        IRepository<Word> wordRepository,
        IRepository<Language> languageRepository,
        IUnitOfWork unitOfWork,
        IImportJobService importJobService,
        ICurrentUserService currentUserService)
    {
        _cefrWordListProvider = cefrWordListProvider
            ?? throw new ArgumentNullException(nameof(cefrWordListProvider));

        _learningItemRepository = learningItemRepository
            ?? throw new ArgumentNullException(nameof(learningItemRepository));

        _learningItemGenericRepository = learningItemGenericRepository
            ?? throw new ArgumentNullException(nameof(learningItemGenericRepository));

        _wordRepository = wordRepository
            ?? throw new ArgumentNullException(nameof(wordRepository));

        _languageRepository = languageRepository
            ?? throw new ArgumentNullException(nameof(languageRepository));

        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));

        _importJobService = importJobService
            ?? throw new ArgumentNullException(nameof(importJobService));

        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// CEFR/word profile import use-case'ini çalıştırır.
    /// </summary>
    public async Task<ImportCefrWordsResponse> Handle(
        ImportCefrWordsCommand request,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>();

        // Normalde validator bunu yakalar.
        // Ama handler doğrudan testten veya farklı bir yerden çağrılırsa
        // güvenli olmak için burada da guard bırakıyoruz.
        if (request.SourceStream is null)
        {
            throw new BusinessRuleException("CEFR import dosyası zorunludur.");
        }

        var normalizedImportSource = NormalizeImportSource(request.ImportSource);
        var contentSource = ResolveContentSource(normalizedImportSource);
        var sourceProviderName = ResolveSourceProviderName(normalizedImportSource);
        var sourceVersion = ResolveSourceVersion(
            normalizedImportSource,
            request.SourceVersion);

        if (contentSource == ContentSource.Unknown)
        {
            throw new BusinessRuleException(
                $"Desteklenmeyen import source değeri. ImportSource: {request.ImportSource}");
        }

        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);

        var sourceLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == sourceLanguageCode && language.IsActive,
            cancellationToken);

        if (sourceLanguage is null)
        {
            throw new BusinessRuleException(
                $"Kaynak dil bulunamadı veya aktif değil. LanguageCode: {sourceLanguageCode}");
        }

        // ImportJob'u provider/import işlemi başlamadan önce oluşturuyoruz.
        // Böylece bu import operasyonunun SQL tarafında takip edilebilir bir kaydı olur.
        var importJob = await _importJobService.StartAsync(
            jobType: ImportJobType.WordListImport,
            sourceName: sourceProviderName,
            sourceVersion: sourceVersion,
            sourceFileName: request.FileName,
            dryRun: request.DryRun,
            triggeredByKeycloakUserId: GetTriggeredByKeycloakUserId(),
            cancellationToken: cancellationToken);

        try
        {
            // Handler CSV formatını bilmez.
            // Sadece provider'ın döndürdüğü standart import row modelini kullanır.
            var providerResult = await _cefrWordListProvider.LoadAsync(
                request.SourceStream,
                cancellationToken);

            if (!providerResult.Succeeded)
            {
                await _importJobService.FailAsync(
                    importJob.Id,
                    new ImportJobFailureRequest
                    {
                        ErrorMessage = BuildProviderFailureMessage(providerResult.Errors),
                        TotalRows = providerResult.Rows.Count,
                        ProcessedRows = 0,
                        CreatedCount = 0,
                        UpdatedCount = 0,
                        SkippedCount = 0,
                        FailedCount = providerResult.Errors.Count
                    },
                    cancellationToken);

                return new ImportCefrWordsResponse
                {
                    ImportJobId = importJob.Id,
                    DryRun = request.DryRun,
                    TotalParsedRows = providerResult.Rows.Count,
                    CreatedCount = 0,
                    WouldCreateCount = 0,
                    SkippedExistingCount = 0,
                    FailedCount = providerResult.Errors.Count,
                    Messages = providerResult.Errors
                };
            }

            // Provider genel olarak başarılı olabilir ama bazı satırlar hatalı/atlanmış olabilir.
            // Bu uyarıları response içinde gösteriyoruz.
            messages.AddRange(providerResult.Errors);

            var batchSize = ResolveBatchSize(request.BatchSize);

            var createdCount = 0;
            var wouldCreateCount = 0;
            var skippedExistingCount = 0;
            var failedCount = providerResult.Errors.Count;

            // Aynı CSV içinde aynı kelime birden fazla geçerse,
            // henüz SaveChanges yapılmadan duplicate oluşmasını engellemek için HashSet kullanıyoruz.
            var seenNormalizedWordsInCurrentImport = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            // Batch ekleme için geçici listeler.
            var pendingLearningItems = new List<LearningItem>();
            var pendingWords = new List<Word>();

            foreach (var row in providerResult.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var displayText = NormalizeDisplayText(row.Text);
                var normalizedText = NormalizeWord(row.NormalizedText);

                if (string.IsNullOrWhiteSpace(displayText))
                {
                    failedCount++;
                    messages.Add(
                        $"Line {row.SourceRowNumber}: Kelime metni boş olduğu için satır atlandı.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(normalizedText))
                {
                    failedCount++;
                    messages.Add(
                        $"Line {row.SourceRowNumber}: Normalize edilmiş kelime boş olduğu için satır atlandı.");
                    continue;
                }

                if (displayText.Length > MaxWordTextLength ||
                    normalizedText.Length > MaxWordTextLength)
                {
                    failedCount++;
                    messages.Add(
                        $"Line {row.SourceRowNumber}: Kelime metni {MaxWordTextLength} karakter sınırını aştığı için satır atlandı. Word: {displayText}");
                    continue;
                }

                // CSV içinde duplicate varsa aynı import içinde ikinci kez işlemiyoruz.
                if (!seenNormalizedWordsInCurrentImport.Add(normalizedText))
                {
                    skippedExistingCount++;
                    messages.Add(
                        $"Line {row.SourceRowNumber}: Aynı dosya içinde tekrar eden kelime atlandı. Word: {normalizedText}");
                    continue;
                }

                // Database'de aynı dilde aynı kelime zaten varsa tekrar eklemiyoruz.
                var alreadyExists = await _learningItemRepository.WordExistsAsync(
                    normalizedText,
                    sourceLanguage.Id,
                    cancellationToken);

                if (alreadyExists)
                {
                    skippedExistingCount++;
                    continue;
                }

                if (request.DryRun)
                {
                    wouldCreateCount++;
                    continue;
                }

                var externalSourceKey = BuildExternalSourceKey(
                    row.SourceRowNumber,
                    normalizedText,
                    sourceProviderName,
                    sourceVersion);

                var learningItem = new LearningItem(
                    LearningItemType.Word,
                    sourceLanguage.Id,
                    row.CefrLevel,
                    row.DifficultyGroup,
                    LearningItemSourceType.Import,
                    contentSource,
                    ContentQualityStatus.Verified,
                    externalSourceKey,
                    DateTime.UtcNow);

                var word = new Word(
                    learningItem.Id,
                    displayText,
                    normalizedText,
                    row.PartOfSpeech);

                pendingLearningItems.Add(learningItem);
                pendingWords.Add(word);

                createdCount++;

                // Batch dolduysa database'e yazıyoruz.
                if (pendingLearningItems.Count >= batchSize)
                {
                    await SaveBatchAsync(
                        pendingLearningItems,
                        pendingWords,
                        cancellationToken);
                }
            }

            // Döngü bittikten sonra elde kalan son batch'i de yazıyoruz.
            if (!request.DryRun && pendingLearningItems.Count > 0)
            {
                await SaveBatchAsync(
                    pendingLearningItems,
                    pendingWords,
                    cancellationToken);
            }

            if (request.DryRun)
            {
                messages.Add(
                    $"DryRun aktif. Database'e kayıt atılmadı. Eklenebilir kelime sayısı: {wouldCreateCount}.");
            }

            messages.Add(
                $"Import source: {sourceProviderName}, version: {sourceVersion}, source language: {sourceLanguageCode}.");

            // Import başarıyla bittiği için ImportJob kaydını Completed yapıyoruz.
            await _importJobService.CompleteAsync(
                importJob.Id,
                new ImportJobCompletionRequest
                {
                    TotalRows = providerResult.Rows.Count,
                    ProcessedRows = providerResult.Rows.Count,
                    CreatedCount = request.DryRun ? 0 : createdCount,
                    UpdatedCount = 0,
                    SkippedCount = skippedExistingCount,
                    FailedCount = failedCount,
                    SummaryMessage = BuildCompletionSummaryMessage(
                        sourceProviderName,
                        sourceVersion,
                        request.DryRun,
                        createdCount,
                        wouldCreateCount,
                        skippedExistingCount,
                        failedCount)
                },
                cancellationToken);

            return new ImportCefrWordsResponse
            {
                ImportJobId = importJob.Id,
                DryRun = request.DryRun,
                TotalParsedRows = providerResult.Rows.Count,
                CreatedCount = request.DryRun ? 0 : createdCount,
                WouldCreateCount = request.DryRun ? wouldCreateCount : 0,
                SkippedExistingCount = skippedExistingCount,
                FailedCount = failedCount,
                Messages = messages
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Beklenmeyen hata olursa job kaydını Failed yapmaya çalışıyoruz.
            // Sonra orijinal exception'ı tekrar fırlatıyoruz.
            await TryFailImportJobAsync(
                importJob.Id,
                exception,
                cancellationToken);

            throw;
        }
    }

    /// <summary>
    /// Biriken LearningItem ve Word kayıtlarını database'e yazar.
    /// 
    /// Repository AddRangeAsync tek başına database'e kayıt atmaz.
    /// Asıl INSERT işlemi UnitOfWork.SaveChangesAsync ile gerçekleşir.
    /// </summary>
    private async Task SaveBatchAsync(
        List<LearningItem> pendingLearningItems,
        List<Word> pendingWords,
        CancellationToken cancellationToken)
    {
        if (pendingLearningItems.Count == 0)
        {
            return;
        }

        await _learningItemGenericRepository.AddRangeAsync(
            pendingLearningItems,
            cancellationToken);

        await _wordRepository.AddRangeAsync(
            pendingWords,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Aynı listeleri tekrar save etmeyelim diye temizliyoruz.
        pendingLearningItems.Clear();
        pendingWords.Clear();
    }

    /// <summary>
    /// Import job'a yazılacak kullanıcı id değerini güvenli şekilde döndürür.
    /// 
    /// Admin endpointleri normalde authenticated çalışır.
    /// Ama handler testlerinde current user olmayabilir.
    /// Bu yüzden burada exception fırlatmak yerine null dönebilen güvenli property kullanıyoruz.
    /// </summary>
    private string? GetTriggeredByKeycloakUserId()
    {
        return _currentUserService.IsAuthenticated
            ? _currentUserService.KeycloakUserId
            : null;
    }

    /// <summary>
    /// Provider tamamen başarısız olduğunda ImportJob.ErrorMessage için güvenli mesaj üretir.
    /// </summary>
    private static string BuildProviderFailureMessage(
        IReadOnlyCollection<string> errors)
    {
        if (errors.Count == 0)
        {
            return "CEFR word import provider failed.";
        }

        var message = string.Join(" | ", errors.Take(5));

        return message.Length <= 1000
            ? message
            : message[..1000];
    }

    /// <summary>
    /// Completed job için okunabilir summary mesajı üretir.
    /// </summary>
    private static string BuildCompletionSummaryMessage(
        string sourceProviderName,
        string sourceVersion,
        bool dryRun,
        int createdCount,
        int wouldCreateCount,
        int skippedExistingCount,
        int failedCount)
    {
        var message =
            $"CEFR word import completed. Source: {sourceProviderName}, Version: {sourceVersion}, " +
            $"DryRun: {dryRun}, Created: {createdCount}, WouldCreate: {wouldCreateCount}, " +
            $"Skipped: {skippedExistingCount}, Failed: {failedCount}.";

        return message.Length <= 1000
            ? message
            : message[..1000];
    }

    /// <summary>
    /// Beklenmeyen exception durumunda ImportJob kaydını Failed yapmaya çalışır.
    /// 
    /// Bu method best-effort çalışır:
    /// - Asıl exception'ı ezmez.
    /// - FailAsync hata verirse onu yutar.
    /// - Ana handler orijinal exception'ı tekrar fırlatır.
    /// </summary>
    private async Task TryFailImportJobAsync(
        Guid importJobId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var errorMessage = exception.Message;

            if (errorMessage.Length > 1000)
            {
                errorMessage = errorMessage[..1000];
            }

            await _importJobService.FailAsync(
                importJobId,
                new ImportJobFailureRequest
                {
                    ErrorMessage = errorMessage,
                    TotalRows = 0,
                    ProcessedRows = 0,
                    CreatedCount = 0,
                    UpdatedCount = 0,
                    SkippedCount = 0,
                    FailedCount = 1
                },
                cancellationToken);
        }
        catch
        {
            // Job fail update hatası, asıl import exception'ını ezmemeli.
        }
    }

    /// <summary>
    /// Batch size değerini güvenli hale getirir.
    /// 
    /// Validator normalde hatalı değerleri yakalar.
    /// Ancak handler doğrudan çağrılırsa da güvenli default davranış isteriz.
    /// </summary>
    private static int ResolveBatchSize(int? requestedBatchSize)
    {
        if (!requestedBatchSize.HasValue)
        {
            return ImportConstants.DefaultBatchSize;
        }

        if (requestedBatchSize.Value <= 0)
        {
            return ImportConstants.DefaultBatchSize;
        }

        return Math.Min(
            requestedBatchSize.Value,
            ImportConstants.MaxImportBatchSize);
    }

    /// <summary>
    /// Dil kodunu normalize eder.
    /// 
    /// Örnek:
    /// " EN " => "en"
    /// </summary>
    private static string NormalizeLanguageCode(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Kullanıcıya gösterilecek kelime metnini normalize eder.
    /// 
    /// Burada küçük harfe çevirmiyoruz.
    /// Çünkü display text orijinal gösterim değeridir.
    /// </summary>
    private static string NormalizeDisplayText(string value)
    {
        return value.Trim();
    }

    /// <summary>
    /// Kelimeyi duplicate kontrol için normalize eder.
    /// </summary>
    private static string NormalizeWord(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Import source değerini normalize eder.
    /// 
    /// Örnek:
    /// "CEFR-J" => "cefrj"
    /// "cefr_j" => "cefrj"
    /// "octanove" => "octanove"
    /// </summary>
    private static string NormalizeImportSource(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }

    /// <summary>
    /// Normalize edilmiş import source değerini ContentSource enumuna çevirir.
    /// </summary>
    private static ContentSource ResolveContentSource(string normalizedImportSource)
    {
        return normalizedImportSource switch
        {
            ImportConstants.ImportSources.CefrJ => ContentSource.CefrJ,
            ImportConstants.ImportSources.Octanove => ContentSource.Octanove,
            _ => ContentSource.Unknown
        };
    }

    /// <summary>
    /// Normalize edilmiş import source değerinden okunabilir provider adını üretir.
    /// </summary>
    private static string ResolveSourceProviderName(string normalizedImportSource)
    {
        return normalizedImportSource switch
        {
            ImportConstants.ImportSources.CefrJ => ImportConstants.ProviderNames.CefrJ,
            ImportConstants.ImportSources.Octanove => ImportConstants.ProviderNames.Octanove,
            _ => ImportConstants.ProviderNames.Fallback
        };
    }

    /// <summary>
    /// Import source için version bilgisini belirler.
    /// 
    /// Request'ten özel version geldiyse onu kullanır.
    /// Gelmediyse source'a göre default version döner.
    /// </summary>
    private static string ResolveSourceVersion(
        string normalizedImportSource,
        string? requestedSourceVersion)
    {
        if (!string.IsNullOrWhiteSpace(requestedSourceVersion))
        {
            return requestedSourceVersion.Trim();
        }

        return normalizedImportSource switch
        {
            ImportConstants.ImportSources.CefrJ => ImportConstants.SourceVersions.CefrJ15,
            ImportConstants.ImportSources.Octanove => ImportConstants.SourceVersions.Octanove10,
            _ => "unknown"
        };
    }

    /// <summary>
    /// LearningItem.ExternalSourceKey için takip edilebilir source key üretir.
    /// 
    /// Örnek:
    /// cefr-j:1.5:line-25:abandon
    /// octanove:1.0:line-30:timid
    /// </summary>
    private static string BuildExternalSourceKey(
        int sourceRowNumber,
        string normalizedText,
        string sourceProviderName,
        string sourceVersion)
    {
        var normalizedProviderName = sourceProviderName
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");

        var normalizedVersion = sourceVersion
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");

        var sourceKey =
            $"{normalizedProviderName}:{normalizedVersion}:line-{sourceRowNumber}:{normalizedText}";

        if (sourceKey.Length <= ImportConstants.MaxExternalSourceKeyLength)
        {
            return sourceKey;
        }

        return sourceKey[..ImportConstants.MaxExternalSourceKeyLength];
    }
}