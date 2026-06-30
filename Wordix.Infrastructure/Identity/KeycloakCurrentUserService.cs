using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Models.Identity;

namespace Wordix.Infrastructure.Identity;

/// <summary>
/// JWT token içindeki Keycloak claimlerini okuyarak current user bilgisini sağlayan servistir.
/// 
/// Neden Infrastructure katmanında?
/// - HttpContext ASP.NET Core'a ait bir detaydır.
/// - JWT claim okuma teknik bir altyapı işidir.
/// - Application katmanı Keycloak veya HttpContext bilmemelidir.
/// 
/// Bu servis, ICurrentUserService interface'ini implemente eder.
/// Böylece Application katmanı sadece interface'e bağımlı kalır.
/// </summary>
public sealed class KeycloakCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// IHttpContextAccessor ile o anki HTTP request'in HttpContext bilgisine erişiriz.
    /// 
    /// HttpContext içinde:
    /// - User
    /// - Claims
    /// - Identity
    /// bilgileri bulunur.
    /// </summary>
    public KeycloakCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// O anki request'i yapan ClaimsPrincipal nesnesidir.
    /// 
    /// ClaimsPrincipal, ASP.NET Core'un authenticate edilmiş kullanıcıyı temsil ettiği nesnedir.
    /// JWT token doğrulandıktan sonra token içindeki claimler buraya aktarılır.
    /// </summary>
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// Kullanıcı authenticate olmuş mu?
    /// 
    /// Token yoksa false.
    /// Token geçerliyse true.
    /// </summary>
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Keycloak kullanıcı id'sini döndürür.
    /// 
    /// Keycloak tokenlarında kullanıcı id genellikle "sub" claimindedir.
    /// Bazı JWT mapping ayarlarında bu değer ClaimTypes.NameIdentifier olarak da gelebilir.
    /// Bu yüzden ikisini de kontrol ediyoruz.
    /// 
    /// Yeni mimaride bu değer Wordix kullanıcı sahipliği için ana referanstır.
    /// Backend ayrıca UserProfileId/UserId üretmez.
    /// </summary>
    public string? KeycloakUserId =>
        FindFirstClaimValue("sub")
        ?? FindFirstClaimValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Kullanıcının email adresini döndürür.
    /// 
    /// Keycloak tokenında genellikle "email" claimi vardır.
    /// ASP.NET Core mapping durumuna göre ClaimTypes.Email de kullanılabilir.
    /// </summary>
    public string? Email =>
        FindFirstClaimValue("email")
        ?? FindFirstClaimValue(ClaimTypes.Email);

    /// <summary>
    /// Kullanıcının username bilgisini döndürür.
    /// 
    /// Keycloak tarafında bu bilgi çoğunlukla "preferred_username" claiminden gelir.
    /// Bazı durumlarda ClaimTypes.Name veya "name" claimi de bulunabilir.
    /// </summary>
    public string? Username =>
        FindFirstClaimValue("preferred_username")
        ?? FindFirstClaimValue("name")
        ?? FindFirstClaimValue(ClaimTypes.Name)
        ?? Email;

    /// <summary>
    /// Kullanıcının rollerini döndürür.
    /// 
    /// realm_access.roles içindeki Keycloak rollerini JWT authentication aşamasında
    /// ASP.NET Core'un anlayacağı role claimlerine çevirmiştik.
    /// 
    /// Yine de daha dayanıklı olması için birkaç olası role claim tipini kontrol ediyoruz:
    /// - ClaimTypes.Role
    /// - "role"
    /// - "roles"
    /// </summary>
    public IReadOnlyCollection<string> Roles
    {
        get
        {
            if (User is null)
            {
                return Array.Empty<string>();
            }

            var roles = User.Claims
                .Where(claim =>
                    claim.Type == ClaimTypes.Role ||
                    claim.Type == "role" ||
                    claim.Type == "roles")
                .Select(claim => claim.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return roles;
        }
    }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder.
    /// 
    /// Örnek:
    /// currentUserService.HasRole("admin")
    /// currentUserService.HasRole("basic_user")
    /// </summary>
    public bool HasRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return Roles.Any(currentRole =>
            string.Equals(currentRole, role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Current user bilgisini tek model halinde döndürür.
    /// 
    /// Eğer kullanıcı authenticate değilse Anonymous user modeli döner.
    /// Böylece null dönmeyiz ve application tarafında daha güvenli kullanım sağlarız.
    /// </summary>
    public CurrentUserInfo GetCurrentUser()
    {
        if (!IsAuthenticated)
        {
            return CurrentUserInfo.Anonymous();
        }

        return new CurrentUserInfo
        {
            KeycloakUserId = KeycloakUserId,
            Email = Email,
            Username = Username,
            Roles = Roles,
            IsAuthenticated = IsAuthenticated
        };
    }

    /// <summary>
    /// Authenticated kullanıcının Keycloak user id değerini zorunlu olarak döndürür.
    /// 
    /// Bu method özellikle kullanıcıya bağlı use-case'lerde kullanılacak:
    /// - Lookup history
    /// - User dictionary
    /// - Quiz session
    /// - Quiz answer
    /// - Learning progress
    /// 
    /// Yeni mimari kararımıza göre handlerlar artık UserProfile.Id almayacak.
    /// Bunun yerine doğrudan token içindeki KeycloakUserId değerini kullanacak.
    /// 
    /// Bu method Keycloak'a istek atmaz.
    /// Sadece JWT middleware tarafından doğrulanmış token claimlerini HttpContext.User üzerinden okur.
    /// </summary>
    public string GetRequiredKeycloakUserId()
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(KeycloakUserId))
        {
            throw new UnauthorizedAccessException("Keycloak user id could not be read from token.");
        }

        return KeycloakUserId.Trim();
    }

    /// <summary>
    /// Claim listesi içinden verilen claim tipine ait ilk değeri döndürür.
    /// 
    /// Neden yardımcı method?
    /// - Aynı claim okuma kodunu tekrar tekrar yazmamak için.
    /// - Null kontrolünü merkezi yapmak için.
    /// - Kodun okunabilirliğini artırmak için.
    /// </summary>
    private string? FindFirstClaimValue(string claimType)
    {
        if (User is null)
        {
            return null;
        }

        return User.FindFirst(claimType)?.Value;
    }
}