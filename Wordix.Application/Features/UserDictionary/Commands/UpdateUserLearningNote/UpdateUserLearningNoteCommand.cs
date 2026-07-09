using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.UpdateUserLearningNote;

/// <summary>
/// Kullanıcının kendi notunu güncelleme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan NoteId alır.
/// - Body'den NoteText alır.
/// - Mapper bu iki bilgiyi command'e dönüştürür.
/// - Handler ownership ve update işlemini yürütür.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// </summary>
public sealed class UpdateUserLearningNoteCommand
    : IRequest<UserLearningNoteResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Güncellenecek UserLearningNote id değeridir.
    /// </summary>
    public Guid UserLearningNoteId { get; init; }

    /// <summary>
    /// Notun yeni metnidir.
    /// </summary>
    public string NoteText { get; init; } = string.Empty;
}