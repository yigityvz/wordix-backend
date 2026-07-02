using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.DeleteUserLearningNote;

/// <summary>
/// Kullanıcının kendi dictionary notunu silme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan NoteId alır.
/// - Mapper bu route id bilgisini command'e dönüştürür.
/// - Handler note ownership kontrolünü yapar ve silme işlemini yürütür.
/// </summary>
public sealed record DeleteUserLearningNoteCommand(
    Guid UserLearningNoteId)
    : IRequest<UserLearningNoteResponse>;