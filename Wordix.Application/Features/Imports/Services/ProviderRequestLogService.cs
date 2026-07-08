using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// ProviderRequestLog kayıtlarını oluşturan ve güncelleyen Application service'tir.
/// 
/// Bu service ne yapar?
/// - Provider çağrılarını standart şekilde loglar.
/// - Log kaydını ImportJob ile ilişkilendirebilir.
/// - Log kaydını LearningItem ile ilişkilendirebilir.
/// 
/// Bu service ne yapmaz?
/// - Azure'a istek atmaz.
/// - Cache lookup yapmaz.
/// - Provider response parse etmez.
/// - HTTP request veya controller bilmez.
/// 
/// Yani bu service sadece provider request tracking sorumluluğuna sahiptir.
/// </summary>
public sealed class ProviderRequestLogService : IProviderRequestLogService
{
    private readonly IRepository<ProviderRequestLog> _providerRequestLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProviderRequestLogService(
        IRepository<ProviderRequestLog> providerRequestLogRepository,
        IUnitOfWork unitOfWork)
    {
        _providerRequestLogRepository = providerRequestLogRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Yeni ProviderRequestLog oluşturur ve database'e kaydeder.
    /// </summary>
    public async Task<ProviderRequestLog> CreateAsync(
        ProviderRequestLogCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedStatus = NormalizeStatus(request.Status);
        var wasServedFromCache = normalizedStatus == ProviderRequestStatus.ServedFromCache ||
                                 request.WasServedFromCache;

        var log = new ProviderRequestLog(
            providerType: request.ProviderType,
            providerName: request.ProviderName,
            operationName: request.OperationName,
            status: normalizedStatus,
            requestKey: request.RequestKey,
            normalizedInput: request.NormalizedInput,
            sourceLanguageCode: request.SourceLanguageCode,
            targetLanguageCode: request.TargetLanguageCode,
            importJobId: request.ImportJobId,
            keycloakUserId: request.KeycloakUserId,
            learningItemId: request.LearningItemId,
            wasServedFromCache: wasServedFromCache,
            durationMs: request.DurationMs,
            httpStatusCode: request.HttpStatusCode,
            errorCode: request.ErrorCode,
            errorMessage: request.ErrorMessage);

        await _providerRequestLogRepository.AddAsync(
            log,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return log;
    }

    /// <summary>
    /// Provider log kaydını ImportJob ile ilişkilendirir.
    /// </summary>
    public async Task AttachImportJobAsync(
        Guid providerRequestLogId,
        Guid importJobId,
        CancellationToken cancellationToken = default)
    {
        if (providerRequestLogId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProviderRequestLogId boş Guid olamaz.",
                nameof(providerRequestLogId));
        }

        if (importJobId == Guid.Empty)
        {
            throw new ArgumentException(
                "ImportJobId boş Guid olamaz.",
                nameof(importJobId));
        }

        var log = await _providerRequestLogRepository.GetByIdAsync(
            providerRequestLogId,
            cancellationToken);

        if (log is null)
        {
            throw new InvalidOperationException(
                $"ProviderRequestLog bulunamadı. Id: {providerRequestLogId}");
        }

        log.AttachImportJob(importJobId);

        _providerRequestLogRepository.Update(log);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Provider log kaydını LearningItem ile ilişkilendirir.
    /// </summary>
    public async Task AttachLearningItemAsync(
        Guid providerRequestLogId,
        Guid learningItemId,
        CancellationToken cancellationToken = default)
    {
        if (providerRequestLogId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProviderRequestLogId boş Guid olamaz.",
                nameof(providerRequestLogId));
        }

        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "LearningItemId boş Guid olamaz.",
                nameof(learningItemId));
        }

        var log = await _providerRequestLogRepository.GetByIdAsync(
            providerRequestLogId,
            cancellationToken);

        if (log is null)
        {
            throw new InvalidOperationException(
                $"ProviderRequestLog bulunamadı. Id: {providerRequestLogId}");
        }

        log.AttachLearningItem(learningItemId);

        _providerRequestLogRepository.Update(log);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Status değerini güvenli hale getirir.
    /// 
    /// ProviderRequestLog entity'si Unknown status kabul etmez.
    /// Bu yüzden service seviyesinde daha anlaşılır hata üretiriz.
    /// </summary>
    private static ProviderRequestStatus NormalizeStatus(
        ProviderRequestStatus status)
    {
        if (!Enum.IsDefined(status) || status == ProviderRequestStatus.Unknown)
        {
            throw new ArgumentException(
                "Provider request status geçerli bir değer olmalıdır.",
                nameof(status));
        }

        return status;
    }
}