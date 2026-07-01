using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Decks.Commands.AddItemToDeck;

/// <summary>
/// AddItemToDeckCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Deck var mı kontrol eder.
/// - Deck current user'a ait mi kontrol eder.
/// - UserLearningItem var mı kontrol eder.
/// - UserLearningItem current user'a ait mi kontrol eder.
/// - Aynı item aynı deck'e daha önce eklenmiş mi kontrol eder.
/// - DeckItem oluşturur.
/// - SaveChanges çalıştırır.
/// - AddItemToDeckResponse döner.
/// 
/// Bu handler ne yapmaz?
/// - HTTP response oluşturmaz.
/// - Controller işi yapmaz.
/// - DbContext kullanmaz.
/// - Response propertylerini tek tek controller içinde dizmez.
/// 
/// Önemli tasarım kararı:
/// DeckItem doğrudan LearningItemId tutmaz.
/// UserLearningItemId tutar.
/// Böylece deck'e sadece kullanıcının kendi dictionary itemları eklenebilir.
/// </summary>
public sealed class AddItemToDeckCommandHandler
    : IRequestHandler<AddItemToDeckCommand, AddItemToDeckResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddItemToDeckCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository,
        IRepository<UserLearningItem> userLearningItemRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// AddItemToDeckCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<AddItemToDeckResponse> Handle(
        AddItemToDeckCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Deck ve UserLearningItem ownership kontrolleri bu değerle yapılır.
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
        // Kullanıcı başkasının deck'ine item ekleyemez.
        if (!string.Equals(
                deck.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot add items to another user's deck.");
        }

        // 4. UserLearningItem var mı ve aktif mi kontrol ediyoruz.
        //
        // Buradaki id global LearningItemId değildir.
        // Kullanıcının dictionary item id değeridir.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item => item.Id == request.UserLearningItemId && item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new NotFoundException(
                "User dictionary item",
                request.UserLearningItemId);
        }

        // 5. UserLearningItem current user'a ait mi kontrol ediyoruz.
        //
        // Kullanıcı başkasının dictionary itemını kendi deck'ine ekleyemez.
        if (!string.Equals(
                userLearningItem.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot add another user's dictionary item to your deck.");
        }

        // 6. Aynı item aynı deck'e daha önce eklenmiş mi kontrol ediyoruz.
        //
        // Database tarafında DeckId + UserLearningItemId unique index var.
        // Ama kullanıcıya anlamlı business rule hatası dönmek için burada da kontrol ediyoruz.
        var alreadyAdded = await _deckItemRepository.FirstOrDefaultAsync(
            deckItem =>
                deckItem.DeckId == deck.Id &&
                deckItem.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        if (alreadyAdded is not null)
        {
            throw new BusinessRuleException(
                "This dictionary item is already added to this deck.",
                "DECK_ITEM_ALREADY_EXISTS");
        }

        // 7. DeckItem oluşturuyoruz.
        //
        // DeckItem -> UserLearningItem -> LearningItem zinciriyle çalışır.
        var deckItem = new DeckItem(
            deckId: deck.Id,
            userLearningItemId: userLearningItem.Id);

        // 8. Repository üzerinden ekliyoruz.
        await _deckItemRepository.AddAsync(
            deckItem,
            cancellationToken);

        // 9. Değişiklikleri database'e kaydediyoruz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. Response mapping işini DeckMapper'a bırakıyoruz.
        return DeckMapper.ToAddItemToDeckResponse(deckItem);
    }
}