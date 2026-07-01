using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Localization;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Localization;
using Wordix.Application.Common.Models.Persistence;
using Wordix.Application.Features.Lookups.Models;
using Wordix.Application.Features.Lookups.Dtos.Responses;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;
using Wordix.Application.Features.Lookups.Mappers;

namespace Wordix.Application.Features.Lookups.Commands.CreateLookup;

/// <summary>
/// CreateLookupCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Kullanıcının lookup isteğini işler.
/// - Current user bilgisinden KeycloakUserId değerini alır.
/// - Text'i normalize eder.
/// - Input tipini belirler.
/// - Word ve Phrase lookup sonucunu global içerik havuzuna kaydedebilir.
/// - Sentence lookup sonucunu geçici translation response olarak döner.
/// - Sentence kalıcı kayıtları sadece kullanıcı dictionary'ye kaydetmek isterse oluşturulur..
/// - Önce local database'de kelime arar.
/// - Bulamazsa dictionary provider çağırır.
/// - Word/Phrase provider sonucu varsa LearningItem + Word/Phrase + Meaning oluşturur.
/// - Sentence provider sonucu varsa sadece LookupHistory oluşturur ve translation response döner.
/// - Her durumda uygun LookupHistory kaydı oluşturur.
/// - Kullanıcıya LookupResponse döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcı sahipliği token içindeki KeycloakUserId ile yapılır.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir use-case akışıdır.
/// - HTTP detayı bilmez.
/// - DbContext bilmez.
/// - Keycloak claim detayı bilmez.
/// - Repository ve service abstraction'ları üzerinden çalışır.
/// </summary>
public sealed class CreateLookupCommandHandler
    : IRequestHandler<CreateLookupCommand, LookupResponse>
{

    private readonly ICurrentUserService _currentUserService;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ILookupClassifier _lookupClassifier;
    private readonly IDictionaryProvider _dictionaryProvider;
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly ILanguageResolver _languageResolver;
    private readonly IRepository<LearningItem> _learningItemGenericRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<LookupHistory> _lookupHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler ihtiyacı olan tüm application/persistence abstraction'larını DI üzerinden alır.
    /// 
    /// Dikkat:
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada Keycloak claim okuma yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden alınır.
    /// Bu servis Application katmanına sadece gerekli kullanıcı bilgisini sağlar.
    /// </summary>
    public CreateLookupCommandHandler(
        ICurrentUserService currentUserService,
        ITextNormalizer textNormalizer,
        ILookupClassifier lookupClassifier,
        IDictionaryProvider dictionaryProvider,
        ILearningItemRepository learningItemRepository,
        IUserLearningItemRepository userLearningItemRepository,
        ILanguageResolver languageResolver,
        IRepository<LearningItem> learningItemGenericRepository,
        IRepository<Word> wordRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<LookupHistory> lookupHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _textNormalizer = textNormalizer;
        _lookupClassifier = lookupClassifier;
        _dictionaryProvider = dictionaryProvider;
        _learningItemRepository = learningItemRepository;
        _userLearningItemRepository = userLearningItemRepository;
        _languageResolver = languageResolver;
        _learningItemGenericRepository = learningItemGenericRepository;
        _wordRepository = wordRepository;
        _phraseRepository = phraseRepository;
        _meaningRepository = meaningRepository;
        _lookupHistoryRepository = lookupHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// CreateLookupCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<LookupResponse> Handle(
        CreateLookupCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        // Kullanıcıya ait lookup/dictionary/progress gibi kayıtlar bu KeycloakUserId ile ilişkilendirilir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Kullanıcının gönderdiği ham text'i normalize ediyoruz.
        // Örnek:
        // " Achieve " → "achieve"
        var normalizedText = _textNormalizer.Normalize(request.Text);

        // 3. Source ve target language bilgilerini merkezi resolver üzerinden çözüyoruz.
        //
        // Handler artık language repository/cache detayını bilmez.
        // ILanguageResolver implementasyonu önce cache'e bakar,
        // cache'te yoksa database'den aktif Language kaydını çözer.
        var sourceLanguage = await _languageResolver.GetRequiredActiveLanguageByCodeAsync(
            request.SourceLanguageCode,
            cancellationToken);

        var targetLanguage = await _languageResolver.GetRequiredActiveLanguageByCodeAsync(
            request.TargetLanguageCode,
            cancellationToken);

        // 4. Input Word/Phrase/Sentence mı belirliyoruz.
        var inputType = _lookupClassifier.Classify(normalizedText);

        // 5. Word/Phrase için önce local database'de arama yapacağız.
        //
        // Sentence için bilinçli olarak database lookup yapmıyoruz.
        // Çünkü Faz 19 kararına göre sentence lookup translation use-case gibi çalışır.
        // Her sentence lookup sonucunu database'e yazmak istemiyoruz.
        // Kullanıcı cümleyi kaydetmek isterse kalıcı Sentence save akışında oluşturulur.

        // 6. Önce local database'de arıyoruz.
        // Word ve Phrase için ayrı repository methodları kullanıyoruz.
        // Böylece çalışan Word akışını bozmadan Phrase desteğini ekliyoruz.
        if (inputType is LookupInputType.Word)
        {
            var databaseLookupData = await _learningItemRepository.GetWordLookupDataAsync(
                normalizedText,
                sourceLanguage.Id,
                targetLanguage.Id,
                cancellationToken);

            if (databaseLookupData is not null)
            {
                return await HandleWordDatabaseLookupResultAsync(
                    keycloakUserId: keycloakUserId,
                    request: request,
                    normalizedText: normalizedText,
                    sourceLanguage: sourceLanguage,
                    targetLanguage: targetLanguage,
                    databaseLookupData: databaseLookupData,
                    cancellationToken: cancellationToken);
            }
        }

        if (inputType is LookupInputType.Phrase)
        {
            var databaseLookupData = await _learningItemRepository.GetPhraseLookupDataAsync(
                normalizedText,
                sourceLanguage.Id,
                targetLanguage.Id,
                cancellationToken);

            if (databaseLookupData is not null)
            {
                return await HandlePhraseDatabaseLookupResultAsync(
                    keycloakUserId: keycloakUserId,
                    request: request,
                    normalizedText: normalizedText,
                    sourceLanguage: sourceLanguage,
                    targetLanguage: targetLanguage,
                    databaseLookupData: databaseLookupData,
                    cancellationToken: cancellationToken);
            }
        }

        // 7. Database'de yoksa provider çağırıyoruz.
        // Faz 13C'de Infrastructure içinde PrototypeDictionaryProvider ekledik.
        var providerResult = await _dictionaryProvider.FindAsync(
            normalizedText,
            request.SourceLanguageCode,
            request.TargetLanguageCode,
            cancellationToken);

        if (!providerResult.Found)
        {
            await SaveNotFoundLookupHistoryAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                inputType: inputType,
                sourceLanguageId: sourceLanguage.Id,
                targetLanguageId: targetLanguage.Id,
                providerName: providerResult.ProviderName,
                cancellationToken: cancellationToken);

            throw new NotFoundException("Lookup result", normalizedText);
        }

        return inputType switch
        {
            LookupInputType.Word => await HandleWordProviderLookupResultAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                sourceLanguage: sourceLanguage,
                targetLanguage: targetLanguage,
                providerResult: providerResult,
                cancellationToken: cancellationToken),

            LookupInputType.Phrase => await HandlePhraseProviderLookupResultAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                sourceLanguage: sourceLanguage,
                targetLanguage: targetLanguage,
                providerResult: providerResult,
                cancellationToken: cancellationToken),

            LookupInputType.Sentence => await HandleSentenceProviderLookupResultAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                sourceLanguage: sourceLanguage,
                targetLanguage: targetLanguage,
                providerResult: providerResult,
                cancellationToken: cancellationToken),

            _ => throw new BusinessRuleException(
                "Lookup input type is not supported.",
                "LOOKUP_INPUT_TYPE_NOT_SUPPORTED")
        };
    }

    /// <summary>
    /// Database'de bulunan kelime için LookupHistory oluşturur ve LookupResponse döner.
    /// </summary>
    private async Task<LookupResponse> HandleWordDatabaseLookupResultAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        WordLookupData databaseLookupData,
        CancellationToken cancellationToken)
    {
        var resultCount = databaseLookupData.Meanings.Count;

        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Word,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            learningItemId: databaseLookupData.LearningItem.Id,
            wasFoundInDatabase: true,
            wasCreatedFromProvider: false,
            providerName: null,
            resultCount: resultCount);

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Kullanıcının dictionary'sinde bu item zaten var mı diye kontrol ediyoruz.
        // Yeni mimaride bu kontrol UserProfileId ile değil KeycloakUserId ile yapılacak.
        var isAlreadyInUserDictionary = await _userLearningItemRepository
            .ExistsByUserAndLearningItemAsync(
                keycloakUserId,
                databaseLookupData.LearningItem.Id,
                cancellationToken);

        return LookupMapper.ToDatabaseLookupResponse(
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            databaseLookupData: databaseLookupData,
            lookupHistory: lookupHistory,
            isAlreadyInUserDictionary: isAlreadyInUserDictionary);
    }


    /// <summary>
    /// Database'de bulunan phrase için LookupHistory oluşturur ve LookupResponse döner.
    /// </summary>
    private async Task<LookupResponse> HandlePhraseDatabaseLookupResultAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        PhraseLookupData databaseLookupData,
        CancellationToken cancellationToken)
    {
        var resultCount = databaseLookupData.Meanings.Count;

        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Phrase,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            learningItemId: databaseLookupData.LearningItem.Id,
            wasFoundInDatabase: true,
            wasCreatedFromProvider: false,
            providerName: null,
            resultCount: resultCount);

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Kullanıcının dictionary'sinde bu phrase zaten var mı diye kontrol ediyoruz.
        // Kontrol yine LearningItemId üzerinden yapılır.
        var isAlreadyInUserDictionary = await _userLearningItemRepository
            .ExistsByUserAndLearningItemAsync(
                keycloakUserId,
                databaseLookupData.LearningItem.Id,
                cancellationToken);

        return LookupMapper.ToDatabaseLookupResponse(
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            databaseLookupData: databaseLookupData,
            lookupHistory: lookupHistory,
            isAlreadyInUserDictionary: isAlreadyInUserDictionary);
    }


    /// <summary>
    /// Provider'dan bulunan word için LearningItem + Word + Meaning + LookupHistory oluşturur.
    /// </summary>
    private async Task<LookupResponse> HandleWordProviderLookupResultAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        CancellationToken cancellationToken)
    {
        // İlk prototype provider'da CEFR/Difficulty otomatik tespit etmiyoruz.
        // Faz 24 import/provider sisteminde bu konu detaylandırılacak.
        var learningItem = new LearningItem(
            LearningItemType.Word,
            sourceLanguage.Id,
            CefrLevel.A1,
            DifficultyGroup.Beginner,
            LearningItemSourceType.Provider);

        var primaryProviderMeaning = providerResult.Meanings.First();

        var word = new Word(
            learningItem.Id,
            normalizedText,
            normalizedText,
            primaryProviderMeaning.PartOfSpeech,
            pronunciation: null);

        var meanings = providerResult.Meanings
            .Select((providerMeaning, index) => new Meaning(
                learningItem.Id,
                targetLanguage.Id,
                providerMeaning.Translation,
                providerMeaning.Definition,
                providerMeaning.PartOfSpeech,
                category: null,
                isPrimary: index == 0,
                displayOrder: index + 1))
            .ToArray();

        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Word,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            learningItemId: learningItem.Id,
            wasFoundInDatabase: false,
            wasCreatedFromProvider: true,
            providerName: providerResult.ProviderName,
            resultCount: meanings.Length);

        await _learningItemGenericRepository.AddAsync(learningItem, cancellationToken);
        await _wordRepository.AddAsync(word, cancellationToken);

        foreach (var meaning in meanings)
        {
            await _meaningRepository.AddAsync(meaning, cancellationToken);
        }

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);

        // LearningItem + Word + Meaning + LookupHistory tek SaveChanges ile kaydedilir.
        // EF Core SaveChanges kendi içinde transaction açar.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Yeni oluşturulan bir provider sonucu kullanıcının dictionary'sine otomatik kaydedilmez.
        // Bu yüzden false.
        const bool isAlreadyInUserDictionary = false;

        return LookupMapper.ToProviderLookupResponse(
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            providerResult: providerResult,
            learningItem: learningItem,
            word: word,
            meanings: meanings,
            lookupHistory: lookupHistory,
            isAlreadyInUserDictionary: isAlreadyInUserDictionary);
    }


    /// <summary>
    /// Provider'dan bulunan phrase için LearningItem + Phrase + Meaning + LookupHistory oluşturur.
    /// </summary>
    private async Task<LookupResponse> HandlePhraseProviderLookupResultAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        CancellationToken cancellationToken)
    {
        // İlk phrase provider akışında CEFR/Difficulty otomatik tespit etmiyoruz.
        // Faz 24 import/provider sisteminde bu konu detaylandırılacak.
        var learningItem = new LearningItem(
            LearningItemType.Phrase,
            sourceLanguage.Id,
            CefrLevel.A1,
            DifficultyGroup.Beginner,
            LearningItemSourceType.Provider);

        var phrase = new Phrase(
            learningItem.Id,
            normalizedText,
            normalizedText,
            PhraseType.Unknown);

        var meanings = providerResult.Meanings
            .Select((providerMeaning, index) => new Meaning(
                learningItem.Id,
                targetLanguage.Id,
                providerMeaning.Translation,
                providerMeaning.Definition,
                providerMeaning.PartOfSpeech,
                category: null,
                isPrimary: index == 0,
                displayOrder: index + 1))
            .ToArray();

        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Phrase,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            learningItemId: learningItem.Id,
            wasFoundInDatabase: false,
            wasCreatedFromProvider: true,
            providerName: providerResult.ProviderName,
            resultCount: meanings.Length);

        await _learningItemGenericRepository.AddAsync(learningItem, cancellationToken);
        await _phraseRepository.AddAsync(phrase, cancellationToken);

        foreach (var meaning in meanings)
        {
            await _meaningRepository.AddAsync(meaning, cancellationToken);
        }

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);

        // LearningItem + Phrase + Meaning + LookupHistory tek SaveChanges ile kaydedilir.
        // EF Core SaveChanges kendi içinde transaction açar.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Yeni oluşturulan bir provider sonucu kullanıcının dictionary'sine otomatik kaydedilmez.
        // Bu yüzden false.
        const bool isAlreadyInUserDictionary = false;

        return LookupMapper.ToProviderLookupResponse(
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            providerResult: providerResult,
            learningItem: learningItem,
            phrase: phrase,
            meanings: meanings,
            lookupHistory: lookupHistory,
            isAlreadyInUserDictionary: isAlreadyInUserDictionary);
    }


    /// <summary>
    /// Provider'dan bulunan sentence translation sonucu için LookupHistory oluşturur ve LookupResponse döner.
    /// 
    /// Faz 19 kararı:
    /// - Sentence lookup translation use-case olarak çalışır.
    /// - Lookup anında LearningItem oluşturulmaz.
    /// - Lookup anında Sentence oluşturulmaz.
    /// - Lookup anında SentenceTranslation oluşturulmaz.
    /// - Kalıcı kayıt sadece kullanıcı sentence'i dictionary'ye kaydetmek isterse oluşturulur.
    /// </summary>
    private async Task<LookupResponse> HandleSentenceProviderLookupResultAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        CancellationToken cancellationToken)
    {
        if (providerResult.SentenceTranslations.Count == 0)
        {
            throw new BusinessRuleException(
                "Provider returned a sentence lookup result without any sentence translation.",
                "SENTENCE_TRANSLATION_RESULT_EMPTY");
        }

        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Sentence,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            learningItemId: null,
            wasFoundInDatabase: false,
            wasCreatedFromProvider: false,
            providerName: providerResult.ProviderName,
            resultCount: providerResult.SentenceTranslations.Count);

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);

        // Sentence lookup anında sadece LookupHistory kaydedilir.
        // LearningItem/Sentence/SentenceTranslation kaydı yapılmaz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LookupMapper.ToProviderSentenceLookupResponse(
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            providerResult: providerResult,
            lookupHistory: lookupHistory);
    }


    /// <summary>
    /// Şu an desteklenmeyen input tipleri için lookup history kaydı oluşturur.
    /// 
    /// Faz 18 itibarıyla Word ve Phrase desteklenir.
    /// Sentence ise Faz 19'a bırakıldığı için bu method şu anda özellikle Sentence için kullanılır.
    /// </summary>
    private async Task SaveUnsupportedLookupHistoryAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LookupInputType inputType,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken)
    {
        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: inputType,
            sourceLanguageId: sourceLanguageId,
            targetLanguageId: targetLanguageId,
            learningItemId: null,
            wasFoundInDatabase: false,
            wasCreatedFromProvider: false,
            providerName: null,
            resultCount: 0);

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Database ve provider sonucunda bulunamayan word/phrase lookup istekleri için lookup history oluşturur.
    /// </summary>
    private async Task SaveNotFoundLookupHistoryAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        LookupInputType inputType,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        string providerName,
        CancellationToken cancellationToken)
    {
        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: inputType,
            sourceLanguageId: sourceLanguageId,
            targetLanguageId: targetLanguageId,
            learningItemId: null,
            wasFoundInDatabase: false,
            wasCreatedFromProvider: false,
            providerName: providerName,
            resultCount: 0);

        await _lookupHistoryRepository.AddAsync(lookupHistory, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// LookupHistory entity'sini merkezi olarak oluşturur.
    /// 
    /// Neden ayrı method?
    /// - Database sonucu, provider sonucu, not found ve unsupported input senaryolarında
    ///   aynı entity oluşturma kodunu tekrar etmemek için.
    /// 
    /// Yeni mimaride LookupHistory kullanıcıyı UserProfileId ile değil,
    /// doğrudan KeycloakUserId ile sahiplenir.
    /// </summary>
    private static LookupHistory CreateLookupHistory(
        string keycloakUserId,
        string queryText,
        string normalizedQueryText,
        LookupInputType inputType,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        Guid? learningItemId,
        bool wasFoundInDatabase,
        bool wasCreatedFromProvider,
        string? providerName,
        int resultCount)
    {
        return new LookupHistory(
            keycloakUserId,
            queryText,
            normalizedQueryText,
            LookupMapper.ToDomainInputType(inputType),
            sourceLanguageId,
            targetLanguageId,
            learningItemId,
            wasFoundInDatabase,
            wasCreatedFromProvider,
            providerType: null,
            providerName,
            resultCount);
    }

}
