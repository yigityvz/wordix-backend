using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;

/// <summary>
/// Current user'ın kendi dictionary listesini getiren query modelidir.
/// 
/// Neden query?
/// - Veri okuma işlemidir.
/// - Yeni kayıt oluşturmaz.
/// - Sistem durumunu değiştirmez.
/// 
/// Kullanıcı bilgisi request body/query string ile alınmaz.
/// CurrentUserBehavior pipeline içinde token üzerinden bulunur.
/// </summary>
public sealed record GetMyDictionaryQuery
    : IRequest<GetMyDictionaryResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// 
    /// Client bu değeri göndermez.
    /// Query string veya body üzerinden alınmaz.
    /// Handler bu değerle current user'ın dictionary kayıtlarını filtreler.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}