using Wordix.Application.Features.Lookups.Models;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Provider'dan gelen phrase sonucunu global catalog'a kaydeden service sözleşmesidir.
/// 
/// Bu interface neden var?
/// - Lookup handler Azure'dan sonuç aldıktan sonra phrase oluşturma detaylarını bilmemeli.
/// - LearningItem + Phrase + Meaning oluşturma mantığı tek yerde durmalı.
/// - 24L'de lookup akışına bu service'i bağlayacağız.
/// </summary>
public interface IProviderPhraseCreationService
{
    /// <summary>
    /// Provider'dan gelen phrase sonucunu global catalog'a kaydeder.
    /// 
    /// Bu method:
    /// - Duplicate phrase kontrolü yapar.
    /// - LearningItem oluşturur.
    /// - Phrase oluşturur.
    /// - Meaning oluşturur.
    /// - UnitOfWork ile kaydeder.
    /// </summary>
    Task<ProviderPhraseCreationResult> CreateAsync(
        ProviderPhraseCreationRequest request,
        CancellationToken cancellationToken = default);
}