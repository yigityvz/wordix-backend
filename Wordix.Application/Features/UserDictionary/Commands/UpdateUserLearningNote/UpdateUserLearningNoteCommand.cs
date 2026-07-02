using MediatR;
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
/// </summary>
public sealed class UpdateUserLearningNoteCommand
    : IRequest<UserLearningNoteResponse>
{
    /// <summary>
    /// Güncellenecek UserLearningNote id değeridir.
    /// </summary>
    public Guid UserLearningNoteId { get; init; }

    /// <summary>
    /// Notun yeni metnidir.
    /// </summary>
    public string NoteText { get; init; } = string.Empty;
}