using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.SetUserLearningFlag;

/// <summary>
/// Kullanıcının kendi dictionary item'ına flag ekleme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Body'den FlagType alır.
/// - Mapper bu iki bilgiyi command'e dönüştürür.
/// - Handler ownership ve duplicate/idempotent kontrolünü yapar.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed class SetUserLearningFlagCommand
    : IRequest<UserLearningFlagResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Flag eklenecek kullanıcı dictionary item id değeridir.
    /// 
    /// Global LearningItemId değildir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Eklenecek flag tipidir.
    /// 
    /// Örnek:
    /// Favorite
    /// Difficult
    /// </summary>
    public string FlagType { get; init; } = string.Empty;
}