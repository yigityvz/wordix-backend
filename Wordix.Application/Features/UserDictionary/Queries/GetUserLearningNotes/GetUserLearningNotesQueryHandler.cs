using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningNotes;

/// <summary>
/// GetUserLearningNotesQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Route'tan gelen UserLearningItemId gerçekten current user'a ait mi kontrol eder.
/// - İlgili UserLearningItem'a bağlı notları listeler.
/// - Response DTO mapping işlemini UserDictionaryMapper'a bırakır.
/// 
/// Ownership zinciri:
/// UserLearningNote -> UserLearningItem -> KeycloakUserId
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - Controller logic'i içermez.
/// - Response propertylerini controller içinde dizmez.
/// </summary>
public sealed class GetUserLearningNotesQueryHandler
    : IRequestHandler<GetUserLearningNotesQuery, GetUserLearningNotesResponse>
{
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningNote> _userLearningNoteRepository;

    public GetUserLearningNotesQueryHandler(
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<UserLearningNote> userLearningNoteRepository)
    {
        _userLearningItemRepository = userLearningItemRepository;
        _userLearningNoteRepository = userLearningNoteRepository;
    }

    public async Task<GetUserLearningNotesResponse> Handle(
        GetUserLearningNotesQuery request,
        CancellationToken cancellationToken)
    {
        // Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        var keycloakUserId = request.KeycloakUserId;

        // Önce UserLearningItem current user'a ait mi kontrol ediyoruz.
        //
        // Notlar doğrudan KeycloakUserId tutmadığı için ownership kontrolü
        // UserLearningItem üzerinden yapılır.
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

        var notes = await _userLearningNoteRepository.ListAsync(
            note => note.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        var orderedNotes = notes
            .OrderByDescending(note => note.CreatedAt)
            .ToArray();

        return UserDictionaryMapper.ToGetUserLearningNotesResponse(orderedNotes);
    }
}