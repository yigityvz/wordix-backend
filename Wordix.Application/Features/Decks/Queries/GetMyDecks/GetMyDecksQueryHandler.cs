using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Decks.Queries.GetMyDecks;

/// <summary>
/// GetMyDecksQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Kullanıcının aktif decklerini getirir.
/// - Her deck için item count hesaplar.
/// - GetMyDecksResponse döner.
/// 
/// Bu handler ne yapmaz?
/// - HTTP response oluşturmaz.
/// - Controller işi yapmaz.
/// - DbContext kullanmaz.
/// - Keycloak claim detayını bilmez.
/// - Response DTO propertylerini controller içinde dizmez.
/// 
/// Ownership:
/// Deck kayıtları KeycloakUserId ile kullanıcıya bağlanır.
/// Bu yüzden kullanıcı sadece kendi decklerini görebilir.
/// </summary>
public sealed class GetMyDecksQueryHandler
    : IRequestHandler<GetMyDecksQuery, GetMyDecksResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;

    /// <summary>
    /// Handler ihtiyacı olan servisleri DI üzerinden alır.
    /// 
    /// Application katmanı DbContext bilmez.
    /// Persistence işlemleri repository abstraction'ları üzerinden yapılır.
    /// </summary>
    public GetMyDecksQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository)
    {
        _currentUserService = currentUserService;
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
    }

    /// <summary>
    /// Current user'ın kendi aktif deck listesini döner.
    /// </summary>
    public async Task<GetMyDecksResponse> Handle(
        GetMyDecksQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Kullanıcı deckleri bu alan üzerinden filtrelenir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Kullanıcının aktif decklerini getiriyoruz.
        //
        // Başka kullanıcıların deckleri bu sorguya dahil edilmez.
        var decks = await _deckRepository.ListAsync(
            deck => deck.KeycloakUserId == keycloakUserId && deck.IsActive,
            cancellationToken);

        // Kullanıcının hiç deck'i yoksa gereksiz DeckItem sorgusu atmadan boş response döneriz.
        if (decks.Count == 0)
        {
            return DeckMapper.ToEmptyGetMyDecksResponse();
        }

        // 3. Deck id listesini çıkarıyoruz.
        //
        // Bu liste üzerinden DeckItem kayıtlarını tek sorguda alacağız.
        // Böylece her deck için ayrı ayrı count sorgusu atmayız.
        var deckIds = decks
            .Select(deck => deck.Id)
            .Distinct()
            .ToArray();

        // 4. Kullanıcının decklerine ait DeckItem kayıtlarını toplu alıyoruz.
        //
        // DeckItem zaten DeckId üzerinden deck'e bağlıdır.
        // Deckler current user'a göre filtrelendiği için burada deckIds filtresi güvenlidir.
        var deckItems = await _deckItemRepository.ListAsync(
            deckItem => deckIds.Contains(deckItem.DeckId),
            cancellationToken);

        // 5. DeckId -> item count lookup oluşturuyoruz.
        //
        // Örnek:
        // Software English deck -> 5 item
        // Daily Phrases deck -> 3 item
        var itemCountByDeckId = deckItems
            .GroupBy(deckItem => deckItem.DeckId)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        // 6. Deck entitylerini response DTO'larına mapliyoruz.
        //
        // Handler sadece veriyi toplar.
        // Response şekillendirme DeckMapper sorumluluğundadır.
        var deckResponses = decks
            .OrderByDescending(deck => deck.CreatedAt)
            .Select(deck =>
            {
                itemCountByDeckId.TryGetValue(deck.Id, out var itemCount);

                return DeckMapper.ToDeckSummaryResponse(
                    deck: deck,
                    itemCount: itemCount);
            })
            .ToArray();

        return DeckMapper.ToGetMyDecksResponse(deckResponses);
    }
}