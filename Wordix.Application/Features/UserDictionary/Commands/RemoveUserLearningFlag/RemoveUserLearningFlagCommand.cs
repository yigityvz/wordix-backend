using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.RemoveUserLearningFlag;

/// <summary>
/// Kullanıcının kendi dictionary item'ından belirli bir flag'i kaldırma use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Controller route'tan FlagType alır.
/// - Mapper bu iki route bilgisini command'e dönüştürür.
/// - Handler ownership ve silme işlemini yürütür.
/// 
/// Örnek:
/// DELETE /api/user-dictionary/{userLearningItemId}/flags/Difficult
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed record RemoveUserLearningFlagCommand(
    Guid UserLearningItemId,
    string FlagType)
    : IRequest<UserLearningFlagResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}