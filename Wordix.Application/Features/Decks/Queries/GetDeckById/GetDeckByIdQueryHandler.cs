using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.Decks.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.Decks.Queries.GetDeckById;

/// <summary>
/// GetDeckByIdQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Deck var mı kontrol eder.
/// - Deck current user'a ait mi kontrol eder.
/// - Deck içindeki DeckItem kayıtlarını getirir.
/// - DeckItem -> UserLearningItem -> LearningItem zincirini kurar.
/// - Word/Phrase/Sentence detaylarını toplar.
/// - Word/Phrase için Meaning bilgilerini toplar.
/// - Sentence için SentenceTranslation bilgisini toplar.
/// - DeckDetailResponse döner.
/// 
/// Bu handler ne yapmaz?
/// - HTTP response oluşturmaz.
/// - Controller işi yapmaz.
/// - DbContext kullanmaz.
/// - Response DTO mapping'i controller içinde yapmaz.
/// 
/// Ownership:
/// Kullanıcı sadece kendi Deck detayını görebilir.
/// Başkasına ait DeckId gönderirse ForbiddenException döner.
/// </summary>
public sealed class GetDeckByIdQueryHandler
    : IRequestHandler<GetDeckByIdQuery, DeckDetailResponse>
{
    private readonly IRepository<Deck> _deckRepository;
    private readonly IRepository<DeckItem> _deckItemRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<Language> _languageRepository;

    /// <summary>
    /// Handler ihtiyacı olan tüm repository ve servisleri DI üzerinden alır.
    /// 
    /// Application katmanı DbContext bilmez.
    /// Veriye repository abstraction'ları üzerinden ulaşır.
    /// </summary>
    public GetDeckByIdQueryHandler(
        IRepository<Deck> deckRepository,
        IRepository<DeckItem> deckItemRepository,
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<Language> languageRepository)
    {
        _deckRepository = deckRepository;
        _deckItemRepository = deckItemRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _sentenceRepository = sentenceRepository;
        _meaningRepository = meaningRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _languageRepository = languageRepository;
    }

    /// <summary>
    /// Current user'ın tek bir deck detayını döner.
    /// </summary>
    public async Task<DeckDetailResponse> Handle(
        GetDeckByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Current user'ın KeycloakUserId değerini request üzerinden alıyoruz.
        //
        // Bu değer client'tan gelmez.
        // CurrentUserBehavior, MediatR pipeline içinde token'dan okuyup request'e yazar.
        // Handler artık ICurrentUserService'e doğrudan bağımlı değildir.
        var keycloakUserId = request.KeycloakUserId;

        // 2. Deck var mı ve aktif mi kontrol ediyoruz.
        var deck = await _deckRepository.FirstOrDefaultAsync(
            deck => deck.Id == request.DeckId && deck.IsActive,
            cancellationToken);

        if (deck is null)
        {
            throw new NotFoundException("Deck", request.DeckId);
        }

        // 3. Ownership kontrolü.
        //
        // Kullanıcı başkasına ait deck detayını göremez.
        if (!string.Equals(
                deck.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot access another user's deck.");
        }

        // 4. Deck içindeki itemları alıyoruz.
        var deckItems = await _deckItemRepository.ListAsync(
            deckItem => deckItem.DeckId == deck.Id,
            cancellationToken);

        // Deck boşsa detay response'u boş item listesiyle döner.
        if (deckItems.Count == 0)
        {
            return DeckMapper.ToDeckDetailResponse(
                deck: deck,
                items: Array.Empty<DeckItemResponse>());
        }

        // 5. DeckItem kayıtlarından UserLearningItem id listesi çıkarıyoruz.
        var userLearningItemIds = deckItems
            .Select(deckItem => deckItem.UserLearningItemId)
            .Distinct()
            .ToArray();

        // 6. UserLearningItem kayıtlarını toplu alıyoruz.
        //
        // Burada tekrar KeycloakUserId filtresi kullanıyoruz.
        // Normalde Deck zaten current user'a ait olduğu için DeckItem'lar da doğru olmalı.
        // Ama defensive ownership için UserLearningItem tarafını da current user ile filtreliyoruz.
        var userLearningItems = await _userLearningItemRepository.ListAsync(
            item =>
                userLearningItemIds.Contains(item.Id) &&
                item.KeycloakUserId == keycloakUserId &&
                item.IsActive,
            cancellationToken);

        var userLearningItemLookup = userLearningItems
            .ToDictionary(item => item.Id);

        if (userLearningItems.Count == 0)
        {
            return DeckMapper.ToDeckDetailResponse(
                deck: deck,
                items: Array.Empty<DeckItemResponse>());
        }

        // 7. LearningItem kayıtlarını toplu alıyoruz.
        var learningItemIds = userLearningItems
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();

        var learningItems = await _learningItemRepository.ListAsync(
            item => learningItemIds.Contains(item.Id) && item.IsActive,
            cancellationToken);

        var learningItemLookup = learningItems
            .ToDictionary(item => item.Id);

        // 8. Word/Phrase/Sentence detaylarını LearningItemId üzerinden toplu alıyoruz.
        var words = await _wordRepository.ListAsync(
            word => learningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        var phrases = await _phraseRepository.ListAsync(
            phrase => learningItemIds.Contains(phrase.LearningItemId),
            cancellationToken);

        var phraseLookup = phrases.ToDictionary(phrase => phrase.LearningItemId);

        var sentences = await _sentenceRepository.ListAsync(
            sentence =>
                sentence.LearningItemId.HasValue &&
                learningItemIds.Contains(sentence.LearningItemId.Value),
            cancellationToken);

        var sentenceLookup = sentences
            .Where(sentence => sentence.LearningItemId.HasValue)
            .ToDictionary(sentence => sentence.LearningItemId!.Value);

        // 9. Word/Phrase için Meaning kayıtlarını toplu alıyoruz.
        var meanings = await _meaningRepository.ListAsync(
            meaning => learningItemIds.Contains(meaning.LearningItemId),
            cancellationToken);

        var meaningsByLearningItemId = meanings
            .GroupBy(meaning => meaning.LearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(meaning => meaning.DisplayOrder)
                    .ToArray());

        // 10. Sentence için SentenceTranslation kayıtlarını toplu alıyoruz.
        var sentenceIds = sentences
            .Select(sentence => sentence.Id)
            .Distinct()
            .ToArray();

        var sentenceTranslations = sentenceIds.Length == 0
            ? Array.Empty<SentenceTranslation>()
            : await _sentenceTranslationRepository.ListAsync(
                translation => sentenceIds.Contains(translation.SourceSentenceId),
                cancellationToken);

        var sentenceTranslationsBySentenceId = sentenceTranslations
            .GroupBy(translation => translation.SourceSentenceId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(translation => translation.IsPrimary)
                    .ThenBy(translation => translation.DisplayOrder)
                    .ToArray());

        // 11. Source language ve sentence target language kayıtlarını toplu alıyoruz.
        var languageIds = learningItems
            .Select(item => item.LanguageId)
            .Concat(sentenceTranslations.Select(translation => translation.TargetLanguageId))
            .Distinct()
            .ToArray();

        var languages = await _languageRepository.ListAsync(
            language => languageIds.Contains(language.Id),
            cancellationToken);

        var languageLookup = languages.ToDictionary(language => language.Id);

        // 12. DeckItem response listesini oluşturuyoruz.
        //
        // DeckItem sıralaması için DisplayOrder kullanmıyoruz.
        // Kullanıcının kararına göre Faz 20'de sıralama yok.
        // Eklenme tarihine göre en yeni üstte göstermek yeterli.
        var itemResponses = deckItems
            .OrderByDescending(deckItem => deckItem.AddedAt)
            .Select(deckItem => TryMapDeckItemResponse(
                deckItem,
                userLearningItemLookup,
                learningItemLookup,
                wordLookup,
                phraseLookup,
                sentenceLookup,
                meaningsByLearningItemId,
                sentenceTranslationsBySentenceId,
                languageLookup))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();

        // 13. Deck detay response mapping işini DeckMapper'a bırakıyoruz.
        return DeckMapper.ToDeckDetailResponse(
            deck: deck,
            items: itemResponses);
    }

    /// <summary>
    /// Tek bir DeckItem için response üretmeye çalışır.
    /// 
    /// Eksik veya tutarsız veri varsa null döner.
    /// Normalde FK ve business rules sayesinde eksik veri olmamalıdır.
    /// Ancak response üretiminde defensive davranmak liste endpointlerinin patlamasını engeller.
    /// </summary>
    private static DeckItemResponse? TryMapDeckItemResponse(
        DeckItem deckItem,
        IReadOnlyDictionary<Guid, UserLearningItem> userLearningItemLookup,
        IReadOnlyDictionary<Guid, LearningItem> learningItemLookup,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Phrase> phraseLookup,
        IReadOnlyDictionary<Guid, Sentence> sentenceLookup,
        IReadOnlyDictionary<Guid, Meaning[]> meaningsByLearningItemId,
        IReadOnlyDictionary<Guid, SentenceTranslation[]> sentenceTranslationsBySentenceId,
        IReadOnlyDictionary<Guid, Language> languageLookup)
    {
        if (!userLearningItemLookup.TryGetValue(
                deckItem.UserLearningItemId,
                out var userLearningItem))
        {
            return null;
        }

        if (!learningItemLookup.TryGetValue(
                userLearningItem.LearningItemId,
                out var learningItem))
        {
            return null;
        }

        wordLookup.TryGetValue(learningItem.Id, out var word);
        phraseLookup.TryGetValue(learningItem.Id, out var phrase);
        sentenceLookup.TryGetValue(learningItem.Id, out var sentence);
        languageLookup.TryGetValue(learningItem.LanguageId, out var sourceLanguage);

        meaningsByLearningItemId.TryGetValue(
            learningItem.Id,
            out var meanings);

        SentenceTranslation? sentenceTranslation = null;
        Language? targetLanguage = null;

        if (sentence is not null &&
            sentenceTranslationsBySentenceId.TryGetValue(
                sentence.Id,
                out var sentenceTranslations))
        {
            sentenceTranslation = sentenceTranslations.FirstOrDefault();

            if (sentenceTranslation is not null)
            {
                languageLookup.TryGetValue(
                    sentenceTranslation.TargetLanguageId,
                    out targetLanguage);
            }
        }

        return DeckMapper.ToDeckItemResponse(
            deckItem: deckItem,
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            word: word,
            phrase: phrase,
            sentence: sentence,
            sourceLanguage: sourceLanguage,
            meanings: meanings ?? Array.Empty<Meaning>(),
            sentenceTranslation: sentenceTranslation,
            targetLanguage: targetLanguage);
    }
}