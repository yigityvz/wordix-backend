using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Commands.UpdateUserLearningNote;

/// <summary>
/// UpdateUserLearningNoteCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - NoteId ile UserLearningNote kaydını bulur.
/// - Notun bağlı olduğu UserLearningItem current user'a ait mi kontrol eder.
/// - Not metnini entity methodu üzerinden günceller.
/// - UnitOfWork ile değişikliği kaydeder.
/// - Response DTO'yu mapper üzerinden döner.
/// 
/// Ownership zinciri:
/// UserLearningNote -> UserLearningItem -> KeycloakUserId
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - HttpContext kullanmaz.
/// - Controller logic'i içermez.
/// - Response mapping'i elle handler içine dağıtmaz.
/// </summary>
public sealed class UpdateUserLearningNoteCommandHandler
    : IRequestHandler<UpdateUserLearningNoteCommand, UserLearningNoteResponse>
{
    private readonly IRepository<UserLearningNote> _userLearningNoteRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    

    public UpdateUserLearningNoteCommandHandler(
        IRepository<UserLearningNote> userLearningNoteRepository,
        IRepository<UserLearningItem> userLearningItemRepository
        )
    {
        _userLearningNoteRepository = userLearningNoteRepository;
        _userLearningItemRepository = userLearningItemRepository;
        
    }

    public async Task<UserLearningNoteResponse> Handle(
        UpdateUserLearningNoteCommand request,
        CancellationToken cancellationToken)
    {
        // Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        var keycloakUserId = request.KeycloakUserId;

        var note = await _userLearningNoteRepository.FirstOrDefaultAsync(
            note => note.Id == request.UserLearningNoteId,
            cancellationToken);

        if (note is null)
        {
            throw new NotFoundException(
                "User learning note",
                request.UserLearningNoteId);
        }

        // Note doğrudan KeycloakUserId tutmaz.
        // Bu yüzden bağlı olduğu UserLearningItem üzerinden ownership kontrolü yapıyoruz.
        //
        // Başka kullanıcıya ait note id gönderilirse burada null döner.
        // NotFound dönmek bilinçli tercih: başka kullanıcıya ait kayıt var mı bilgisini sızdırmayız.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item =>
                item.Id == note.UserLearningItemId &&
                item.KeycloakUserId == keycloakUserId &&
                item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new NotFoundException(
                "User learning note",
                request.UserLearningNoteId);
        }

        note.UpdateText(request.NoteText);


        return UserDictionaryMapper.ToUserLearningNoteResponse(note);
    }
}