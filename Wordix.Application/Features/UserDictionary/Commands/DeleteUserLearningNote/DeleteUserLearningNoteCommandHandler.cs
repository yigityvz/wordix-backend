using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Commands.DeleteUserLearningNote;

/// <summary>
/// DeleteUserLearningNoteCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - NoteId ile UserLearningNote kaydını bulur.
/// - Notun bağlı olduğu UserLearningItem current user'a ait mi kontrol eder.
/// - Notu siler.
/// - UnitOfWork ile değişikliği kaydeder.
/// - Silinen not bilgisini response olarak döner.
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
public sealed class DeleteUserLearningNoteCommandHandler
    : IRequestHandler<DeleteUserLearningNoteCommand, UserLearningNoteResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserLearningNote> _userLearningNoteRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserLearningNoteCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<UserLearningNote> userLearningNoteRepository,
        IRepository<UserLearningItem> userLearningItemRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userLearningNoteRepository = userLearningNoteRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserLearningNoteResponse> Handle(
        DeleteUserLearningNoteCommand request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

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

        // Silmeden önce response modelini hazırlıyoruz.
        // Çünkü SaveChanges sonrası kayıt database'den kalkacak.
        var response = UserDictionaryMapper.ToUserLearningNoteResponse(note);

        _userLearningNoteRepository.Remove(note);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}