using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Commands.CreateUserLearningNote;

/// <summary>
/// CreateUserLearningNoteCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Route'tan gelen UserLearningItemId gerçekten current user'a ait mi kontrol eder.
/// - UserLearningNote entity'si oluşturur.
/// - Repository üzerinden ekler.
/// - UnitOfWork ile kaydeder.
/// - Response DTO'yu mapper üzerinden döner.
/// 
/// Ownership kuralı:
/// UserLearningNote doğrudan KeycloakUserId tutmaz.
/// Sahiplik şu zincirle doğrulanır:
/// UserLearningNote -> UserLearningItem -> KeycloakUserId
/// </summary>
public sealed class CreateUserLearningNoteCommandHandler
    : IRequestHandler<CreateUserLearningNoteCommand, UserLearningNoteResponse>
{
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningNote> _userLearningNoteRepository;
    

    /// <summary>
    /// Handler ihtiyaç duyduğu dependency'leri DI üzerinden alır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller logic'i yok.
    /// </summary>
    public CreateUserLearningNoteCommandHandler(
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<UserLearningNote> userLearningNoteRepository
        )
    {
        _userLearningItemRepository = userLearningItemRepository;
        _userLearningNoteRepository = userLearningNoteRepository;
        
    }

    /// <summary>
    /// Kullanıcının kendi dictionary item'ına not ekleme akışını yürütür.
    /// </summary>
    public async Task<UserLearningNoteResponse> Handle(
        CreateUserLearningNoteCommand request,
        CancellationToken cancellationToken)
    {
        // Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        var keycloakUserId = request.KeycloakUserId;

        // UserLearningItem current user'a ait mi kontrol ediyoruz.
        //
        // Başka kullanıcının UserLearningItemId değeri gönderilirse null döner.
        // Böylece ownership ihlali detay sızdırmadan engellenir.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item =>
                item.Id == request.UserLearningItemId &&
                item.KeycloakUserId == keycloakUserId &&
                item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new NotFoundException(
                "User dictionary item",
                request.UserLearningItemId);
        }

        var note = new UserLearningNote(
            userLearningItem.Id,
            request.NoteText);

        await _userLearningNoteRepository.AddAsync(
            note,
            cancellationToken);

        

        return UserDictionaryMapper.ToUserLearningNoteResponse(note);
    }
}