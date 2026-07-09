using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningNotes;

/// <summary>
/// Kullanıcının kendi dictionary item'ına ait notları listeleme query modelidir.
/// 
/// Bu query neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Application katmanı bu id üzerinden ownership kontrolü yapar.
/// - Sonra sadece current user'a ait item'ın notları listelenir.
/// 
/// Buradaki id global LearningItemId değildir.
/// UserLearningItem.Id değeridir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed record GetUserLearningNotesQuery(
    Guid UserLearningItemId)
    : IRequest<GetUserLearningNotesResponse>, IRequiresCurrentUser
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}