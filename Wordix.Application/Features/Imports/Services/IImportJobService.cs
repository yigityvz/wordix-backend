using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Services;

/// <summary>
/// Import/enrichment job kayıtlarını standart şekilde yöneten service sözleşmesidir.
/// 
/// Bu interface neden var?
/// - ImportJob oluşturma/güncelleme işlemini controller veya handler içine dağıtmak istemiyoruz.
/// - Her import akışı aynı standartla job başlatmalı, tamamlamalı veya failed yapmalı.
/// - İleride ImportJob admin ekranı, retry veya background worker geldiğinde aynı service tekrar kullanılabilir.
/// </summary>
public interface IImportJobService
{
    /// <summary>
    /// Yeni import job oluşturur ve Running durumuna geçirir.
    /// 
    /// Bu method SaveChanges çağırır.
    /// Çünkü job id değerinin gerçek import akışı başlamadan önce oluşmasını isteriz.
    /// </summary>
    Task<ImportJob> StartAsync(
        ImportJobType jobType,
        string sourceName,
        string? sourceVersion = null,
        string? sourceFileName = null,
        bool dryRun = true,
        string? triggeredByKeycloakUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Var olan import job'ı Completed durumuna geçirir.
    /// </summary>
    Task CompleteAsync(
        Guid importJobId,
        ImportJobCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Var olan import job'ı Failed durumuna geçirir.
    /// </summary>
    Task FailAsync(
        Guid importJobId,
        ImportJobFailureRequest request,
        CancellationToken cancellationToken = default);
}