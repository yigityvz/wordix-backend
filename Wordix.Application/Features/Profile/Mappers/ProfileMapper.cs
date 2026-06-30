using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.Profile.Dtos.Responses;

namespace Wordix.Application.Features.Profile.Mappers;

/// <summary>
/// Profile feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Neden var?
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi kullanmak istemiyoruz.
/// - Handler içinde response DTO propertylerini tek tek dizmek istemiyoruz.
/// - Mapping kurallarını feature seviyesinde, açık ve kontrollü biçimde toplamak istiyoruz.
/// 
/// Bu sınıfın sorumluluğu:
/// - Current user bilgisini API response DTO'suna çevirmek.
/// 
/// Önemli:
/// Bu mapper static tutuldu çünkü state tutmaz.
/// DI'a register edilmesine gerek yoktur.
/// </summary>
public static class ProfileMapper
{
    /// <summary>
    /// Current user servisinden gelen authenticated kullanıcı bilgisini
    /// /api/profile/me endpoint response modeline dönüştürür.
    /// 
    /// Bu method database'e gitmez.
    /// Sadece ICurrentUserService üzerinden token claimlerinden okunmuş bilgileri kullanır.
    /// </summary>
    public static CurrentUserInfoResponse ToCurrentUserInfoResponse(
        ICurrentUserService currentUserService)
    {
        ArgumentNullException.ThrowIfNull(currentUserService);

        // KeycloakUserId zorunludur.
        // Eğer kullanıcı authenticated değilse veya token içinde "sub" claim'i okunamazsa
        // ICurrentUserService uygun UnauthorizedAccessException fırlatır.
        var keycloakUserId = currentUserService.GetRequiredKeycloakUserId();

        return new CurrentUserInfoResponse
        {
            IsAuthenticated = currentUserService.IsAuthenticated,
            KeycloakUserId = keycloakUserId,
            Email = currentUserService.Email ?? string.Empty,
            Username = currentUserService.Username ?? string.Empty,

            // Rolleri response'a temiz ve deterministik şekilde döndürüyoruz.
            // Authorization zaten policy tarafında yapılır; buradaki işlem sadece response kalitesi içindir.
            Roles = NormalizeRoles(currentUserService.Roles)
        };
    }

    /// <summary>
    /// Role listesini API response için temizler.
    /// 
    /// Neden ayrı method?
    /// - Ana mapping methodu okunabilir kalsın.
    /// - Role temizleme kuralı tek yerde dursun.
    /// - Null/boş/tekrarlı roller response'a taşınmasın.
    /// </summary>
    private static IReadOnlyCollection<string> NormalizeRoles(
        IEnumerable<string>? roles)
    {
        return (roles ?? Array.Empty<string>())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role)
            .ToArray();
    }
}