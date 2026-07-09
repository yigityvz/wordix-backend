namespace Wordix.Application.Common.Interfaces.Identity;

/// <summary>
/// Current user bilgisine ihtiyaç duyan MediatR command/query requestleri için marker interface'tir.
/// 
/// Neden var?
/// - Her handler içinde ICurrentUserService inject edip
///   GetRequiredKeycloakUserId() çağırmak istemiyoruz.
/// - Current user çözümleme işini MediatR pipeline'a taşıyoruz.
/// - Böylece handler sadece request.KeycloakUserId değerini kullanır.
/// 
/// Nasıl çalışır?
/// - Bir command/query bu interface'i implemente ederse,
///   CurrentUserBehavior otomatik olarak token'dan KeycloakUserId değerini okur.
/// - Okunan değer bu property'ye yazılır.
/// - Handler artık current user servisini bilmek zorunda kalmaz.
/// 
/// Örnek:
/// public sealed class SaveLearningItemCommand : IRequest<SaveLearningItemResponse>, IRequiresCurrentUser
/// {
///     public string KeycloakUserId { get; set; } = string.Empty;
/// }
/// 
/// Önemli:
/// - Bu interface sadece current user zorunlu olan requestlerde kullanılmalıdır.
/// - Public/admin/global işlemlerde gereksiz yere eklenmemelidir.
/// </summary>
public interface IRequiresCurrentUser
{
    /// <summary>
    /// JWT token içindeki sub claiminden gelen Keycloak user id değeridir.
    /// 
    /// Bu değer pipeline tarafından doldurulur.
    /// Client bu değeri göndermez.
    /// Controller bu değeri set etmez.
    /// Handler bu değeri request üzerinden okur.
    /// </summary>
    string KeycloakUserId { get; set; }
}