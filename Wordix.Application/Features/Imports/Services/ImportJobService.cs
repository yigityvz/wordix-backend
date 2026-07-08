using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// ImportJob kayıtlarını oluşturan ve durumlarını güncelleyen Application service'tir.
/// 
/// Bu service ne yapar?
/// - ImportJob oluşturur.
/// - Job'ı Running durumuna geçirir.
/// - Job başarılıysa Completed yapar.
/// - Job hata alırsa Failed yapar.
/// 
/// Bu service ne yapmaz?
/// - Dosya parse etmez.
/// - Provider çağırmaz.
/// - Import algoritmasını çalıştırmaz.
/// - HTTP request bilmez.
/// 
/// Yani bu service sadece operasyonel job tracking sorumluluğuna sahiptir.
/// </summary>
public sealed class ImportJobService : IImportJobService
{
    private readonly IRepository<ImportJob> _importJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportJobService(
        IRepository<ImportJob> importJobRepository,
        IUnitOfWork unitOfWork)
    {
        _importJobRepository = importJobRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Yeni ImportJob oluşturur, Running durumuna geçirir ve database'e kaydeder.
    /// </summary>
    public async Task<ImportJob> StartAsync(
        ImportJobType jobType,
        string sourceName,
        string? sourceVersion = null,
        string? sourceFileName = null,
        bool dryRun = true,
        string? triggeredByKeycloakUserId = null,
        CancellationToken cancellationToken = default)
    {
        var importJob = new ImportJob(
            jobType: jobType,
            sourceName: sourceName,
            sourceVersion: sourceVersion,
            sourceFileName: sourceFileName,
            dryRun: dryRun,
            triggeredByKeycloakUserId: triggeredByKeycloakUserId);

        // Job oluşturulduktan hemen sonra Running yapıyoruz.
        // Çünkü bu service genelde import akışı başlamadan çağrılacak.
        importJob.MarkAsRunning();

        await _importJobRepository.AddAsync(
            importJob,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return importJob;
    }

    /// <summary>
    /// ImportJob kaydını başarıyla tamamlandı olarak işaretler.
    /// </summary>
    public async Task CompleteAsync(
        Guid importJobId,
        ImportJobCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (importJobId == Guid.Empty)
        {
            throw new ArgumentException("ImportJobId boş Guid olamaz.", nameof(importJobId));
        }

        ArgumentNullException.ThrowIfNull(request);

        var importJob = await _importJobRepository.GetByIdAsync(
            importJobId,
            cancellationToken);

        if (importJob is null)
        {
            throw new InvalidOperationException($"ImportJob bulunamadı. Id: {importJobId}");
        }

        importJob.MarkAsCompleted(
            totalRows: request.TotalRows,
            processedRows: request.ProcessedRows,
            createdCount: request.CreatedCount,
            updatedCount: request.UpdatedCount,
            skippedCount: request.SkippedCount,
            failedCount: request.FailedCount,
            summaryMessage: request.SummaryMessage);

        _importJobRepository.Update(importJob);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// ImportJob kaydını hata aldı olarak işaretler.
    /// </summary>
    public async Task FailAsync(
        Guid importJobId,
        ImportJobFailureRequest request,
        CancellationToken cancellationToken = default)
    {
        if (importJobId == Guid.Empty)
        {
            throw new ArgumentException("ImportJobId boş Guid olamaz.", nameof(importJobId));
        }

        ArgumentNullException.ThrowIfNull(request);

        var importJob = await _importJobRepository.GetByIdAsync(
            importJobId,
            cancellationToken);

        if (importJob is null)
        {
            throw new InvalidOperationException($"ImportJob bulunamadı. Id: {importJobId}");
        }

        importJob.MarkAsFailed(
            errorMessage: request.ErrorMessage,
            totalRows: request.TotalRows,
            processedRows: request.ProcessedRows,
            createdCount: request.CreatedCount,
            updatedCount: request.UpdatedCount,
            skippedCount: request.SkippedCount,
            failedCount: request.FailedCount);

        _importJobRepository.Update(importJob);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}