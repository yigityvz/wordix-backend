using Wordix.Application.Common.Models.Translation;
using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Interfaces.Translation;

/// <summary>
/// Dış çeviri provider'ları için ortak interface.
/// 
/// Bu fazda sadece AzureTranslator implementasyonu yazacağız.
/// Buna rağmen interface kullanıyoruz çünkü:
/// - Application katmanı Azure HTTP detaylarını bilmemeli.
/// - Lookup handler ileride doğrudan Azure class'ına bağımlı olmamalı.
/// - Testlerde fake translation provider kullanılabilmeli.
/// </summary>
public interface ITranslationProvider
{
    /// <summary>
    /// Provider'ın gerçek içerik kaynağını belirtir.
    /// 
    /// Bu fazda değer AzureTranslator olacak.
    /// </summary>
    ContentSource ContentSource { get; }

    /// <summary>
    /// Okunabilir provider adıdır.
    /// 
    /// Örnek:
    /// AzureTranslator
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Tek bir metni çevirir.
    /// 
    /// İlk sürümde word, phrase ve sentence lookup fallback için bu method yeterli.
    /// Batch/rate limit stratejisini ileride ihtiyaç olursa ayrıca genişletiriz.
    /// </summary>
    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken = default);
}