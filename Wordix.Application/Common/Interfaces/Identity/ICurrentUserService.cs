using Wordix.Application.Common.Models.Identity;

namespace Wordix.Application.Common.Interfaces.Identity;

/// <summary>
/// O anki request'i yapan kullanıcı bilgisini Application katmanına sağlayan servis interface'idir.
/// 
/// Neden interface?
/// - Application katmanı HttpContext veya JWT detaylarını bilmemelidir.
/// - Keycloak'a bağımlılık doğrudan Application içine girmemelidir.
/// - Testlerde bu interface kolayca fake/mock yapılabilir.
/// 
/// Implementation nerede olacak?
/// - Wordix.Infrastructure katmanında KeycloakCurrentUserService olarak yazılacak.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Kullanıcının authenticate olup olmadığını gösterir.
    /// Token geçerli ise true olur.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Keycloak kullanıcı id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden okunur.
    /// Yeni mimari kararımıza göre Wordix backend kullanıcıya ayrıca UserProfileId/UserId üretmez.
    /// Kullanıcıya ait lookup, dictionary, quiz ve progress kayıtları bu KeycloakUserId ile ilişkilendirilir.
    /// </summary>
    string? KeycloakUserId { get; }

    /// <summary>
    /// Kullanıcının email adresidir.
    /// Token içindeki "email" claiminden okunur.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Kullanıcının username değeridir.
    /// Token içindeki "preferred_username" claiminden okunur.
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Kullanıcının rolleridir.
    /// 
    /// Keycloak realm_access.roles değerleri JWT authentication aşamasında
    /// ASP.NET Core role claimlerine çevrilir.
    /// Bu servis o rolleri standart şekilde Application tarafına verir.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder.
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Tüm current user bilgisini tek model halinde döndürür.
    /// Handler veya service tarafında birden fazla property taşımak yerine bu model kullanılabilir.
    /// </summary>
    CurrentUserInfo GetCurrentUser();

    /// <summary>
    /// Authenticated kullanıcının Keycloak user id değerini zorunlu olarak döndürür.
    /// 
    /// Neden bu metoda ihtiyaç var?
    /// - Kullanıcıya bağlı use-case'lerde KeycloakUserId zorunludur.
    /// - Her handler içinde tekrar tekrar null/auth kontrolü yazmak istemiyoruz.
    /// - UserProfileId yerine artık doğrudan token içindeki KeycloakUserId kullanılacak.
    /// 
    /// Eğer kullanıcı authenticated değilse veya token içinde "sub" claimi yoksa exception fırlatır.
    /// </summary>
    string GetRequiredKeycloakUserId();
}