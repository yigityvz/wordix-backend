using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

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
/// </summary>
public sealed class CreateUserLearningNoteCommand
    : IRequest<UserLearningNoteResponse>
{
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