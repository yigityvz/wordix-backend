using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Decks.Commands.CreateDeck;

/// <summary>
/// CreateDeckCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Deck adını normalize eder.
/// - Aynı kullanıcının aynı aktif deck adına sahip başka deck'i var mı kontrol eder.
/// - Deck entity oluşturur.
/// - Repository üzerinden kaydeder.
/// - UnitOfWork ile SaveChanges çalıştırır.
/// - CreateDeckResponse döner.
/// 
/// Bu handler ne yapmaz?
/// - HTTP response oluşturmaz.
/// - Controller işi yapmaz.
/// - DbContext kullanmaz.
/// - Keycloak claim detayını bilmez.
/// 
/// Ownership:
/// Deck, KeycloakUserId ile kullanıcıya bağlanır.
/// UserProfile/UserProfileId kullanılmaz.
/// </summary>
public sealed class CreateDeckCommandHandler
    : IRequestHandler<CreateDeckCommand, CreateDeckResponse>
{
    private readonly ITextNormalizer _textNormalizer;
    private readonly IRepository<Deck> _deckRepository;

    public CreateDeckCommandHandler(
        ITextNormalizer textNormalizer,
        IRepository<Deck> deckRepository
        )
    {
        _textNormalizer = textNormalizer;
        _deckRepository = deckRepository;
    }

    /// <summary>
    /// CreateDeckCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<CreateDeckResponse> Handle(
        CreateDeckCommand request,
        CancellationToken cancellationToken)
    {
        // Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        // Handler artık ICurrentUserService'e doğrudan bağımlı değildir.
        var keycloakUserId = request.KeycloakUserId;

        // 2. Deck adını normalize ediyoruz.
        //
        // Örnek:
        // " Software English " -> "software english"
        //
        // Böylece aynı kullanıcı aynı deck adını farklı boşluk/case ile tekrar oluşturamaz.
        var normalizedName = _textNormalizer.Normalize(request.Name);

        // 3. Aynı kullanıcıda aynı aktif deck adı var mı kontrol ediyoruz.
        //
        // Database tarafında unique filtered index var.
        // Ancak kullanıcıya daha anlamlı hata dönmek için business rule kontrolünü burada yapıyoruz.
        var duplicateDeckExists = await _deckRepository.FirstOrDefaultAsync(
            deck =>
                deck.KeycloakUserId == keycloakUserId &&
                deck.NormalizedName == normalizedName &&
                deck.IsActive,
            cancellationToken);

        if (duplicateDeckExists is not null)
        {
            throw new BusinessRuleException(
                "You already have an active deck with this name.",
                "DECK_NAME_ALREADY_EXISTS");
        }

        // 4. Domain entity oluşturuyoruz.
        //
        // Deck constructor kendi temel validation'ını da yapar.
        // Handler ise use-case/business rule kontrollerinden sorumludur.
        var deck = new Deck(
            keycloakUserId: keycloakUserId,
            name: request.Name,
            normalizedName: normalizedName,
            description: request.Description);

        // 5. Repository üzerinden ekliyoruz.
        await _deckRepository.AddAsync(
            deck,
            cancellationToken);

        // 7. Response mapping işini DeckMapper'a bırakıyoruz.
        return DeckMapper.ToCreateDeckResponse(deck);
    }
}