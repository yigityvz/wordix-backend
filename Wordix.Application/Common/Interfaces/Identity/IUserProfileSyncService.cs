using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Interfaces.Identity;

/// <summary>
/// Keycloak token'ındaki kullanıcı bilgisi ile Wordix database'indeki UserProfile kaydını
/// eşleştirmekten sorumlu servis sözleşmesidir.
/// 
/// Neden var?
/// - Keycloak authentication sağlar, ama Wordix'e özel kullanıcı profili bizim database'imizde tutulur.
/// - Application ve API katmanı "kullanıcı profili var mı, yoksa oluştur" detayını tekrar tekrar yazmamalıdır.
/// - Profile, lookup, dictionary, quiz gibi use-case'ler aktif Wordix kullanıcısına ihtiyaç duyar.
/// 
/// Bu interface sadece sözleşmedir.
/// Gerçek implementation bir sonraki adımda yazılacaktır.
/// </summary>
public interface IUserProfileSyncService
{
    /// <summary>
    /// O anki authenticated kullanıcının Wordix UserProfile kaydını getirir.
    /// Eğer UserProfile kaydı yoksa yeni bir kayıt oluşturur ve database'e kaydeder.
    /// 
    /// Temel akış:
    /// 1. Current user bilgisi ICurrentUserService üzerinden okunur.
    /// 2. Kullanıcı authenticated değilse hata fırlatılır.
    /// 3. Token içindeki KeycloakUserId alınır.
    /// 4. UserProfiles tablosunda bu KeycloakUserId ile kayıt aranır.
    /// 5. Varsa mevcut UserProfile döndürülür.
    /// 6. Yoksa token bilgileriyle yeni UserProfile oluşturulur.
    /// 7. UserPreference de oluşturulur.
    /// 8. UnitOfWork ile database'e kaydedilir.
    /// 9. Oluşan UserProfile döndürülür.
    /// </summary>
    /// <param name="cancellationToken">
    /// Request iptal edilirse async database işlemlerini durdurmak için kullanılır.
    /// </param>
    /// <returns>
    /// Mevcut veya yeni oluşturulmuş UserProfile kaydı.
    /// </returns>
    Task<UserProfile> GetOrCreateCurrentUserProfileAsync(
        CancellationToken cancellationToken = default);
}