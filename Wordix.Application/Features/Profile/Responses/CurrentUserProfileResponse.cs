namespace Wordix.Application.Features.Profile.Responses;

/// <summary>
/// /api/profile/me endpointinden dönecek kullanıcı profil response modelidir.
/// 
/// Neden DTO kullanıyoruz?
/// - Domain entity'yi doğrudan dışarı açmamak için.
/// - Frontend'e sadece ihtiyaç duyduğu alanları vermek için.
/// - Response formatını ileride entity yapısından bağımsız değiştirebilmek için.
/// </summary>
public sealed class CurrentUserProfileResponse
{
    /// <summary>
    /// Wordix database'indeki UserProfile Id değeridir.
    /// Bu değer Keycloak user id değildir.
    /// Wordix içindeki dictionary, lookup, quiz ve progress kayıtları bu profile bağlanır.
    /// </summary>
    public Guid UserProfileId { get; init; }

    /// <summary>
    /// Kullanıcının email adresidir.
    /// Keycloak token'dan gelen email bilgisiyle oluşturulur.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının username değeridir.
    /// Genellikle Keycloak preferred_username claiminden gelir.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Uygulama içinde gösterilebilecek kullanıcı adıdır.
    /// İlk oluşturma sırasında username/email üzerinden doldurulur.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Kullanıcının Wordix içindeki hesap tipidir.
    /// Enum'u int olarak dönmek yerine string dönüyoruz.
    /// Böylece frontend tarafında daha okunabilir olur.
    /// Örnek:
    /// - BasicUser
    /// - Admin
    /// </summary>
    public string AccountType { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının ana dil id değeridir.
    /// İlk prototipte null olabilir.
    /// İleride kullanıcı profil ayarlarından seçilecek.
    /// </summary>
    public Guid? NativeLanguageId { get; init; }

    /// <summary>
    /// Kullanıcının öğrenmek istediği hedef dil id değeridir.
    /// İlk prototipte null olabilir.
    /// İleride kullanıcı profil ayarlarından seçilecek.
    /// </summary>
    public Guid? TargetLanguageId { get; init; }

    /// <summary>
    /// Kullanıcının Wordix tarafında aktif olup olmadığını gösterir.
    /// </summary>
    public bool IsActive { get; init; }
}