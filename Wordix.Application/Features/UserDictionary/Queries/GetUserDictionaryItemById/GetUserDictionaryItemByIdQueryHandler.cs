using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Domain.Entities;
using Wordix.Application.Features.UserDictionary.Mappers;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;

/// <summary>
/// GetUserDictionaryItemByIdQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Route'tan gelen UserLearningItemId ile dictionary kaydını bulur.
/// - Kayıt current user'a ait mi kontrol eder.
/// - İlgili LearningItem, Word, Meaning, Language ve Progress bilgilerini toplar.
/// - UserDictionaryItemResponse döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcının dictionary kayıtları KeycloakUserId ile sahiplenilir.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir query/use-case akışıdır.
/// - HTTP route detayını bilmez.
/// - DbContext bilmez.
/// - Repository abstraction'ları üzerinden çalışır.
/// </summary>
public sealed class GetUserDictionaryItemByIdQueryHandler
    : IRequestHandler<GetUserDictionaryItemByIdQuery, UserDictionaryItemResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;
    private readonly IRepository<UserLearningNote> _userLearningNoteRepository;
    private readonly IRepository<UserLearningFlag> _userLearningFlagRepository;

    /// <summary>
    /// Handler ihtiyacı olan servis ve repository abstraction'larını DI üzerinden alır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden gelir.
    /// Bu servis token claim okuma detayını Application katmanından saklar.
    /// </summary>
    public GetUserDictionaryItemByIdQueryHandler(
        ICurrentUserService currentUserService,
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Language> languageRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository,
        IRepository<UserLearningNote> userLearningNoteRepository,
        IRepository<UserLearningFlag> userLearningFlagRepository)
    {
        _currentUserService = currentUserService;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _sentenceRepository = sentenceRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _meaningRepository = meaningRepository;
        _languageRepository = languageRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
        _userLearningNoteRepository = userLearningNoteRepository;
        _userLearningFlagRepository = userLearningFlagRepository;
    }

    /// <summary>
    /// Current user'ın dictionary item detayını döner.
    /// </summary>
    public async Task<UserDictionaryItemResponse> Handle(
        GetUserDictionaryItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        // Ownership kontrolü doğrudan KeycloakUserId üzerinden yapılır.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. UserLearningItem kaydını id ile buluyoruz.
        // Bu id global LearningItemId değil, kullanıcının kişisel dictionary kayıt id'sidir.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item => item.Id == request.UserLearningItemId && item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new NotFoundException(
                "User dictionary item",
                request.UserLearningItemId);
        }

        // 3. Ownership kontrolü.
        //
        // Kullanıcı başkasına ait UserLearningItem detayını göremez.
        // Eski mimaride bu kontrol UserProfileId ile yapılıyordu.
        // Yeni mimaride UserLearningItem.KeycloakUserId ile token'daki KeycloakUserId karşılaştırılır.
        if (!string.Equals(
                userLearningItem.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot access another user's dictionary item.");
        }

        // 4. Global LearningItem kaydını alıyoruz.
        var learningItem = await _learningItemRepository.FirstOrDefaultAsync(
            item => item.Id == userLearningItem.LearningItemId && item.IsActive,
            cancellationToken);

        if (learningItem is null)
        {
            throw new NotFoundException(
                "Learning item",
                userLearningItem.LearningItemId);
        }

        // 5. LearningItem Word ise Word detayını alıyoruz.
        // Phrase itemlarında bu sorgu null döner; mapper item type'a göre doğru alanı kullanır.
        var word = await _wordRepository.FirstOrDefaultAsync(
            word => word.LearningItemId == learningItem.Id,
            cancellationToken);

        // LearningItem Phrase ise Phrase detayını alıyoruz.
        // Word itemlarında bu sorgu null döner.
        var phrase = await _phraseRepository.FirstOrDefaultAsync(
            phrase => phrase.LearningItemId == learningItem.Id,
            cancellationToken);


        // LearningItem Sentence ise Sentence detayını alıyoruz.
        // Word/Phrase itemlarında bu sorgu null döner.
        var sentence = await _sentenceRepository.FirstOrDefaultAsync(
            sentence => sentence.LearningItemId == learningItem.Id,
            cancellationToken);

        // 6. LearningItem'ın source language bilgisini alıyoruz.
        var sourceLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Id == learningItem.LanguageId,
            cancellationToken);

        // 7. LearningItem'a ait anlamları alıyoruz.
        var meanings = await _meaningRepository.ListAsync(
            meaning => meaning.LearningItemId == learningItem.Id,
            cancellationToken);

        SentenceTranslation? sentenceTranslation = null;
        Language? targetLanguage = null;

        if (sentence is not null)
        {
            var sentenceTranslations = await _sentenceTranslationRepository.ListAsync(
                translation => translation.SourceSentenceId == sentence.Id,
                cancellationToken);

            sentenceTranslation = sentenceTranslations
                .OrderByDescending(translation => translation.IsPrimary)
                .ThenBy(translation => translation.DisplayOrder)
                .FirstOrDefault();

            if (sentenceTranslation is not null)
            {
                targetLanguage = await _languageRepository.FirstOrDefaultAsync(
                    language => language.Id == sentenceTranslation.TargetLanguageId,
                    cancellationToken);
            }
        }

        // Detay response içinde note/flag özetini göstermek için
        // ilgili UserLearningItem'a bağlı not ve flagleri çekiyoruz.
        var notes = await _userLearningNoteRepository.ListAsync(
            note => note.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        var noteCount = notes.Count;

        var flags = await _userLearningFlagRepository.ListAsync(
            flag => flag.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        // 8. Bu dictionary item'a ait progress kaydını alıyoruz.
        var progress = await _userLearningProgressRepository.FirstOrDefaultAsync(
            progress => progress.UserLearningItemId == userLearningItem.Id,
            cancellationToken);


        // 9. API response mapping işini feature mapper'a bırakıyoruz.
        // Handler veri toplama ve ownership kontrolü yapar;
        // response DTO propertylerini tek tek dizmez.
        return UserDictionaryMapper.ToUserDictionaryItemResponse(
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            word: word,
            phrase: phrase,
            sentence: sentence,
            sourceLanguage: sourceLanguage,
            meanings: meanings,
            sentenceTranslation: sentenceTranslation,
            targetLanguage: targetLanguage,
            progress: progress,
            noteCount: noteCount,
            flags: flags);
    }

}
