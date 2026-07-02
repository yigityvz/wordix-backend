using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Domain.Entities;
using Wordix.Application.Features.UserDictionary.Mappers;

namespace Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;

/// <summary>
/// GetMyDictionaryQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Kullanıcının aktif dictionary kayıtlarını getirir.
/// - Her dictionary kaydı için LearningItem, Word, Meaning, Language ve Progress bilgilerini toplar.
/// - API'ye dönecek GetMyDictionaryResponse modelini üretir.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcının dictionary kayıtları KeycloakUserId ile filtrelenir.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir query/use-case akışıdır.
/// - HTTP detayı bilmez.
/// - DbContext bilmez.
/// - EF Core Include kullanmaz.
/// - Repository abstraction'ları üzerinden çalışır.
/// </summary>
public sealed class GetMyDictionaryQueryHandler
    : IRequestHandler<GetMyDictionaryQuery, GetMyDictionaryResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
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
    /// Handler ihtiyacı olan repository ve servisleri DI üzerinden alır.
    /// 
    /// Burada DbContext inject etmiyoruz.
    /// Burada HttpContext inject etmiyoruz.
    /// Bu sayede Application katmanı Persistence ve API detaylarını bilmez.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden alınır.
    /// Bu servis token claim okuma detayını Application katmanından saklar.
    /// </summary>
    public GetMyDictionaryQueryHandler(
        ICurrentUserService currentUserService,
        IUserLearningItemRepository userLearningItemRepository,
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
    /// Current user'ın kendi dictionary listesini döner.
    /// </summary>
    public async Task<GetMyDictionaryResponse> Handle(
        GetMyDictionaryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        // Dictionary kayıtları doğrudan bu KeycloakUserId üzerinden filtrelenir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Kullanıcının aktif dictionary kayıtlarını getiriyoruz.
        // Bu method artık UserProfileId değil, KeycloakUserId ile çalışır.
        var userLearningItems = await _userLearningItemRepository
            .GetActiveItemsByUserAsync(keycloakUserId, cancellationToken);

        // Dictionary boşsa gereksiz database sorguları yapmadan boş response döneriz.
        if (userLearningItems.Count == 0)
        {
            return UserDictionaryMapper.ToEmptyGetMyDictionaryResponse();
        }

        // 3. İlgili id listelerini çıkarıyoruz.
        // Böylece her item için tek tek sorgu atmak yerine toplu sorgu yapabiliriz.
        var learningItemIds = userLearningItems
            .Select(item => item.LearningItemId)
            .Distinct()
            .ToArray();

        var userLearningItemIds = userLearningItems
            .Select(item => item.Id)
            .Distinct()
            .ToArray();

        // 4. Global LearningItem kayıtlarını toplu alıyoruz.
        var learningItems = await _learningItemRepository.ListAsync(
            item => learningItemIds.Contains(item.Id) && item.IsActive,
            cancellationToken);

        var learningItemLookup = learningItems.ToDictionary(item => item.Id);

        // 5. Word detaylarını LearningItemId üzerinden toplu çekiyoruz.
        // Dictionary sistemi LearningItem merkezli olduğu için Word detayını ayrıca topluyoruz.
        var words = await _wordRepository.ListAsync(
            word => learningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

        // Phrase detaylarını LearningItemId üzerinden toplu çekiyoruz.
        // Faz 18 itibarıyla dictionary listesi sadece Word değil Phrase itemları da gösterebilir.
        var phrases = await _phraseRepository.ListAsync(
            phrase => learningItemIds.Contains(phrase.LearningItemId),
            cancellationToken);

        var phraseLookup = phrases.ToDictionary(phrase => phrase.LearningItemId);

        // Sentence detaylarını LearningItemId üzerinden toplu çekiyoruz.
        // Faz 19 itibarıyla dictionary listesi Word/Phrase yanında Sentence itemları da gösterebilir.
        var sentences = await _sentenceRepository.ListAsync(
            sentence =>
                sentence.LearningItemId.HasValue &&
                learningItemIds.Contains(sentence.LearningItemId.Value),
            cancellationToken);

        var sentenceLookup = sentences
            .Where(sentence => sentence.LearningItemId.HasValue)
            .ToDictionary(sentence => sentence.LearningItemId!.Value);


        // 6. Meaning kayıtlarını toplu alıyoruz.
        // SelectedMeaning veya primary meaning mapping için kullanılacak.
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


        // SentenceTranslation kayıtlarını toplu alıyoruz.
        // Sentence itemları Meaning kullanmaz; tam cümle çevirisi SentenceTranslation tablosundadır.
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

        // 7. Progress kayıtlarını toplu alıyoruz.
        // UserLearningProgress, UserLearningItemId üzerinden bire bir bağlıdır.
        var progresses = await _userLearningProgressRepository.ListAsync(
            progress => userLearningItemIds.Contains(progress.UserLearningItemId),
            cancellationToken);

        var progressLookup = progresses.ToDictionary(progress => progress.UserLearningItemId);

        // 8. Source language bilgilerini toplu almak için LearningItem.LanguageId değerlerini çıkarıyoruz.
        var languageIds = learningItems
            .Select(item => item.LanguageId)
            .Concat(sentenceTranslations.Select(translation => translation.TargetLanguageId))
            .Distinct()
            .ToArray();

        var languages = await _languageRepository.ListAsync(
            language => languageIds.Contains(language.Id),
            cancellationToken);

        var languageLookup = languages.ToDictionary(language => language.Id);


        // Dictionary listesinde her item için note/flag özetini göstermek istiyoruz.
        //
        // Burada her item için ayrı ayrı query atmak yerine,
        // current user'ın dönen UserLearningItem id'leri üzerinden toplu sorgu yapıyoruz

        var notes = await _userLearningNoteRepository.ListAsync(
            note => userLearningItemIds.Contains(note.UserLearningItemId),
            cancellationToken);

        var noteCountsByUserLearningItemId = notes
            .GroupBy(note => note.UserLearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        var flags = await _userLearningFlagRepository.ListAsync(
            flag => userLearningItemIds.Contains(flag.UserLearningItemId),
            cancellationToken);

        var flagsByUserLearningItemId = flags
            .GroupBy(flag => flag.UserLearningItemId)
            .ToDictionary(
                group => group.Key,
                group => group.ToArray());

        var responseItems = userLearningItems
            .Select(item => UserDictionaryMapper.ToUserDictionaryItemResponse(
                userLearningItem: item,
                learningItemLookup: learningItemLookup,
                wordLookup: wordLookup,
                phraseLookup: phraseLookup,
                sentenceLookup: sentenceLookup,
                meaningsByLearningItemId: meaningsByLearningItemId,
                sentenceTranslationsBySentenceId: sentenceTranslationsBySentenceId,
                progressLookup: progressLookup,
                languageLookup: languageLookup,
                noteCountsByUserLearningItemId: noteCountsByUserLearningItemId,
                flagsByUserLearningItemId: flagsByUserLearningItemId))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();

        return UserDictionaryMapper.ToGetMyDictionaryResponse(responseItems);
    }

    /// <summary>
    /// UserLearningItem entity'sini API response DTO'suna dönüştürür.
    /// 
    /// Bu mapping manual yapılıyor.
    /// AutoMapper/Mapster kullanmıyoruz.
    /// </summary>
   
}
