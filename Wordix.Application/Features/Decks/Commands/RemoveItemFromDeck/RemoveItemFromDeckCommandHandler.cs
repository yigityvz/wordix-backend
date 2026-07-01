using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Decks.Commands.RemoveItemFromDeck;

/// <summary>
/// RemoveItemFromDeckCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Deck var mı kontrol eder.
/// - Deck current user'a ait mi kontrol eder.
/// - Deck içinde ilgili UserLearningItem bağlantısı var mı kontrol eder.
/// - DeckItem kaydını siler.
/// - UnitOfWork ile SaveChanges çalıştırır.
/// - RemoveItemFromDeckResponse döner.
/// 
/// Bu handler ne yapmaz?
/// - HTTP response oluşturmaz.
/// - Controller işi yapmaz.
/// - DbContext kullanmaz.
/// - Response mapping'i controller içinde yapmaz.
/// 
/// Faz 20 kararı:
/// DeckItem ilişki tablosu gibi davrandığı için remove işleminde fiziksel silinir.
/// İleride analytics gerekirse DeckActivityEvent ayrı fazda eklenebilir.
/// </summary>
public sealed class RemoveItemFromDeckCommandHandler
    : IRequestHandler<RemoveItemFromDeckCommand, RemoveItemFromDeckResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveItemFromDeckCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// RemoveItemFromDeckCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<RemoveItemFromDeckResponse> Handle(
        RemoveItemFromDeckCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Deck ownership bu alan üzerinden kontrol edilir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Deck var mı ve aktif mi kontrol ediyoruz.
        var deck = await _deckRepository.FirstOrDefaultAsync(
            deck => deck.Id == request.DeckId && deck.IsActive,
            cancellationToken);

        if (deck is null)
        {
            throw new NotFoundException("Deck", request.DeckId);
        }

        // 3. Deck current user'a ait mi kontrol ediyoruz.
        //
        // Kullanıcı başkasının deck'inden item silemez.
        if (!string.Equals(
                deck.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot remove items from another user's deck.");
        }

        // 4. Deck içinde bu UserLearningItem bağlantısı var mı kontrol ediyoruz.
        //
        // Route'taki userLearningItemId, DeckItem.UserLearningItemId alanıdır.
        // Yani global LearningItemId değildir.
        var deckItem = await _deckItemRepository.FirstOrDefaultAsync(
            item =>
                item.DeckId == deck.Id &&
                item.UserLearningItemId == request.UserLearningItemId,
            cancellationToken);

        if (deckItem is null)
        {
            throw new NotFoundException(
                "Deck item",
                request.UserLearningItemId);
        }

        // 5. Response için silmeden önce bilgiyi mapper'a gönderebilmek adına
        // deckItem entity'sini elimizde tutuyoruz.
        var response = DeckMapper.ToRemoveItemFromDeckResponse(deckItem);

        // 6. DeckItem ilişki kaydını fiziksel siliyoruz.
        //
        // DeckItem sadece deck ile user dictionary item arasındaki bağlantıdır.
        // Bu bağlantı kaldırıldığında ilişki kaydı silinebilir.
        _deckItemRepository.Remove(deckItem);

        // 7. Değişiklikleri database'e kaydediyoruz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}