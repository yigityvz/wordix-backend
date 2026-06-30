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
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Repository metotları kullanıcıyı Keycloak token içindeki "sub" claiminden gelen KeycloakUserId ile filtreler.
/// </summary>
public interface ILookupRepository
{
    /// <summary>
    /// Kullanıcının son lookup kayıtlarını getirir.
    /// 
    /// Örnek:
    /// Kullanıcının son 10 araması.
    /// 
    /// keycloakUserId:
    /// - Token içindeki "sub" claiminden gelen kullanıcı id değeridir.
    /// - UserProfileId yerine kullanılır.
    /// - Backend tarafından üretilmez.
    /// 
    /// count değeri çok büyük verilirse performans problemi oluşmasın diye
    /// implementation tarafında güvenli limit uygulanır.
    /// </summary>
    Task<IReadOnlyList<LookupHistory>> GetRecentLookupsByUserAsync(
        string keycloakUserId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının belirli normalize query için son lookup kaydını getirir.
    /// 
    /// Örnek:
    /// Kullanıcı daha önce "achieve" aramış mı?
    /// Aradıysa en son lookup kaydı hangisi?
    /// 
    /// keycloakUserId:
    /// - Lookup geçmişinin hangi Keycloak kullanıcısına ait olduğunu belirler.
    /// </summary>
    Task<LookupHistory?> GetLastLookupByUserAndQueryAsync(
        string keycloakUserId,
        string normalizedQueryText,
        CancellationToken cancellationToken = default);
}