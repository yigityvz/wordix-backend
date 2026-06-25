using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// LookupHistory için özel repository sözleşmesidir.
/// 
/// LookupHistory, kullanıcıların arama davranışlarını kayıt altına alır.
/// Bu veriler ileride:
/// - kullanıcı geçmişi,
/// - en çok aranan kelimeler,
/// - provider kullanım analizi,
/// - admin analytics
/// için kullanılacaktır.
/// </summary>
public interface ILookupRepository
{
    /// <summary>
    /// Kullanıcının son lookup kayıtlarını getirir.
    /// 
    /// Örnek:
    /// Kullanıcının son 10 araması.
    /// 
    /// count değeri çok büyük verilirse performans problemi oluşmasın diye
    /// implementation tarafında güvenli limit uygulayacağız.
    /// </summary>
    Task<IReadOnlyList<LookupHistory>> GetRecentLookupsByUserAsync(
        Guid userProfileId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının belirli normalize query için son lookup kaydını getirir.
    /// 
    /// Örnek:
    /// Kullanıcı daha önce "achieve" aramış mı?
    /// Aradıysa en son lookup kaydı hangisi?
    /// </summary>
    Task<LookupHistory?> GetLastLookupByUserAndQueryAsync(
        Guid userProfileId,
        string normalizedQueryText,
        CancellationToken cancellationToken = default);
}