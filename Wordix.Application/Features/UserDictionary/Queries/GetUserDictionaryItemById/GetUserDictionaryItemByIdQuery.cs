using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;

/// <summary>
/// Current user'ın dictionary'sindeki tek bir item detayını getiren query modelidir.
/// 
/// Bu query neden var?
/// - GET /api/user-dictionary/{id} endpointi için kullanılır.
/// - Kullanıcının kendi dictionary item detayını döner.
/// - Başka kullanıcının item'ına erişim ownership kontrolüyle engellenir.
/// 
/// Buradaki id global LearningItemId değildir.
/// Buradaki id UserLearningItemId değeridir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed record GetUserDictionaryItemByIdQuery
    : IRequest<UserDictionaryItemResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// 
    /// Bu değer client tarafından gönderilmez.
    /// Dictionary item ownership kontrolü için kullanılır.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının kişisel dictionary item id değeridir.
    /// 
    /// Bu id UserLearningItems tablosundaki kaydın id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Query constructor.
    /// 
    /// Controller route parametresinden gelen id değerini bu query'ye aktaracak.
    /// </summary>
    public GetUserDictionaryItemByIdQuery(Guid userLearningItemId)
    {
        UserLearningItemId = userLearningItemId;
    }
}