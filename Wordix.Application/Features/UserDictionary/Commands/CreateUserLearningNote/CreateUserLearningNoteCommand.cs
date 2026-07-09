using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Common.Interfaces.Persistence;

namespace Wordix.Application.Features.UserDictionary.Commands.CreateUserLearningNote;

/// <summary>
/// Kullanıcının kendi dictionary item'ına kişisel not ekleme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller HTTP request'i alır.
/// - Mapper request DTO + route id bilgisini command'e dönüştürür.
/// - Handler ownership ve business kontrollerini yapar.
/// 
/// Not:
/// UserLearningItemId route'tan gelir.
/// NoteText body'den gelir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed class CreateUserLearningNoteCommand
    : IRequest<UserLearningNoteResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Not eklenecek kullanıcı dictionary item id değeridir.
    /// 
    /// Global LearningItemId değildir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının yazdığı kişisel not metnidir.
    /// </summary>
    public string NoteText { get; init; } = string.Empty;
}