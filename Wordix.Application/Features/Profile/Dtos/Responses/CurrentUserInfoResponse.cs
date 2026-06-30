namespace Wordix.Application.Features.Profile.Dtos.Responses;

/// <summary>
/// /api/profile/me endpointinden dönecek current user token bilgi response modelidir.
/// 
/// Bu DTO artık Wordix database'indeki UserProfile kaydını temsil etmez.
/// Çünkü yeni mimaride backend UserProfile oluşturmaz ve UserProfileId üretmez.
/// 
/// Bu DTO neyi temsil eder?
/// - Backend tarafından doğrulanmış JWT token içinden okunabilen kullanıcı bilgisini.
/// - Keycloak kullanıcısının teknik id değerini.
/// - Email, username ve role bilgilerini.
/// 
/// Neden DTO kullanıyoruz?
/// - Token claim detaylarını doğrudan controller'dan dışarı açmamak için.
/// - Frontend'e sadece ihtiyaç duyduğu alanları vermek için.
/// - Response formatını ileride token/claim yapısından bağımsız değiştirebilmek için.
/// </summary>
public sealed class CurrentUserInfoResponse
{
    /// <summary>
    /// Kullanıcının authenticated olup olmadığını gösterir.
    /// 
    /// Normalde bu endpoint [Authorize] ile korunduğu için true dönmesi beklenir.
    /// Yine de response içinde göstermek Swagger/Postman testlerinde faydalıdır.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// Keycloak kullanıcısının teknik id değeridir.
    /// 
    /// Bu değer JWT token içindeki "sub" claiminden gelir.
    /// Wordix backend tarafından üretilmez.
    /// 
    /// Kullanıcıya ait LookupHistory, UserLearningItem, QuizSession ve QuizAnswer
    /// gibi kayıtlar artık bu değer üzerinden kullanıcıya bağlanır.
    /// </summary>
    public string KeycloakUserId { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının email adresidir.
    /// 
    /// Genellikle Keycloak token içindeki "email" claiminden gelir.
    /// Token'da email claim'i yoksa boş string dönebilir.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının username değeridir.
    /// 
    /// Genellikle Keycloak token içindeki "preferred_username" claiminden gelir.
    /// Token'da username claim'i yoksa boş string dönebilir.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının Keycloak realm rolleridir.
    /// 
    /// Örnek:
    /// - basic_user
    /// - admin
    /// 
    /// Backend authorization policy'leri de bu roller üzerinden çalışır.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}
