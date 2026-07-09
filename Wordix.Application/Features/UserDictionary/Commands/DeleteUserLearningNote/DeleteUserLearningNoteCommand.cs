using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.DeleteUserLearningNote;

/// <summary>
/// Kullanıcının kendi dictionary notunu silme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan NoteId alır.
/// - Mapper bu route id bilgisini command'e dönüştürür.
/// - Handler note ownership kontrolünü yapar ve silme işlemini yürütür.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed record DeleteUserLearningNoteCommand(
    Guid UserLearningNoteId)
    : IRequest<UserLearningNoteResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;
}