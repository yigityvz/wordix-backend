namespace Wordix.Application.Common.Models.Identity;

/// <summary>
/// O anki HTTP request'i yapan kullanıcıya ait temel bilgileri taşıyan modeldir.
/// 
/// Bu model JWT token'dan okunmuş kullanıcı bilgisini Application katmanına temiz şekilde taşır.
/// 
/// Neden gerekli?
/// - Application katmanı HttpContext bilmemelidir.
/// - Handler'lar token claimlerini tek tek okumamalıdır.
/// - Kullanıcı bilgisi merkezi ve standart bir model üzerinden taşınmalıdır.
/// </summary>
public sealed class CurrentUserInfo
{
    /// <summary>
    /// Keycloak kullanıcısının benzersiz kullanıcı id değeridir.
    /// 
    /// Keycloak token içinde genellikle "sub" claiminden okunur.
    /// Wordix tarafındaki UserProfile.KeycloakUserId alanı ile eşleşir.
    /// </summary>
    public string? KeycloakUserId { get; init; }

    /// <summary>
    /// Kullanıcının email adresidir.
    /// 
    /// Keycloak token içinde genellikle "email" claiminden okunur.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Kullanıcının username değeridir.
    /// 
    /// Keycloak token içinde genellikle "preferred_username" claiminden okunur.
    /// Bazı durumlarda email ile aynı olabilir.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Kullanıcının sahip olduğu rollerin listesidir.
    /// 
    /// Örnek:
    /// - basic_user
    /// - admin
    /// - teacher
    /// - student
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Request yapan kullanıcının authenticate olup olmadığını gösterir.
    /// 
    /// Token geçerli ise true olur.
    /// Token yoksa veya kullanıcı authenticate değilse false olur.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder.
    /// 
    /// Neden method olarak ekledik?
    /// - Role kontrolünü her yerde Roles.Contains(...) şeklinde tekrar etmek istemiyoruz.
    /// - String karşılaştırmasını case-insensitive yaparak daha güvenli hale getiriyoruz.
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
    /// Token yoksa veya kullanıcı authenticate değilse kullanılabilecek boş kullanıcı modeli.
    /// 
    /// Bu sayede implementation tarafında sürekli null dönmek zorunda kalmayız.
    /// Null yerine anlamlı bir "anonymous user" modeli döneriz.
    /// </summary>
    public static CurrentUserInfo Anonymous()
    {
        return new CurrentUserInfo
        {
            IsAuthenticated = false,
            Roles = Array.Empty<string>()
        };
    }
}