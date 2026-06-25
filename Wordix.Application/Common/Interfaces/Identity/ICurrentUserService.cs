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
    /// Token içindeki "sub" claiminden okunur.
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
    /// Faz 5'te Keycloak realm_access.roles değerlerini ASP.NET Core role claimlerine çevirmiştik.
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
}