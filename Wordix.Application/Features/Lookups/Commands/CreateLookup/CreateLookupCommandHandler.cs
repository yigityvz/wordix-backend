using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Localization;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Common.Models.Localization;
using Wordix.Application.Common.Models.Persistence;
using Wordix.Application.Features.Lookups.Models;
using Wordix.Application.Features.Lookups.Responses;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Lookups.Commands.CreateLookup;

/// <summary>
/// CreateLookupCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Kullanıcının lookup isteğini işler.
/// - Current user bilgisinden KeycloakUserId değerini alır.
/// - Text'i normalize eder.
/// - Input tipini belirler.
/// - İlk prototipte sadece Word lookup destekler.
/// - Önce local database'de kelime arar.
/// - Bulamazsa dictionary provider çağırır.
/// - Provider sonucu varsa LearningItem + Word + Meaning oluşturur.
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
    private const string DatabaseLookupSource = "Database";

    private readonly ICurrentUserService _currentUserService;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ILookupClassifier _lookupClassifier;
    private readonly IDictionaryProvider _dictionaryProvider;
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly ILanguageResolver _languageResolver;
    private readonly IRepository<LearningItem> _learningItemGenericRepository;
    private readonly IRepository<Word> _wordRepository;
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

        // 5. İlk prototipte sadece Word destekliyoruz.
        // Phrase ve Sentence mimari olarak düşünülüyor ama gerçek destek Faz 18/19'a bırakıldı.
        if (inputType is not LookupInputType.Word)
        {
            await SaveUnsupportedLookupHistoryAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                inputType: inputType,
                sourceLanguageId: sourceLanguage.Id,
                targetLanguageId: targetLanguage.Id,
                cancellationToken: cancellationToken);

            throw new BusinessRuleException(
                "Only single-word lookup is supported in the first prototype.",
                "LOOKUP_INPUT_TYPE_NOT_SUPPORTED");
        }

        // 6. Önce local database'de arıyoruz.
        // Faz 9'da yazdığımız özel repository methodunu kullanıyoruz.
        var databaseLookupData = await _learningItemRepository.GetWordLookupDataAsync(
            normalizedText,
            sourceLanguage.Id,
            targetLanguage.Id,
            cancellationToken);

        if (databaseLookupData is not null)
        {
            return await HandleDatabaseLookupResultAsync(
                keycloakUserId: keycloakUserId,
                request: request,
                normalizedText: normalizedText,
                sourceLanguage: sourceLanguage,
                targetLanguage: targetLanguage,
                databaseLookupData: databaseLookupData,
                cancellationToken: cancellationToken);
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
                sourceLanguageId: sourceLanguage.Id,
                targetLanguageId: targetLanguage.Id,
                providerName: providerResult.ProviderName,
                cancellationToken: cancellationToken);

            throw new NotFoundException("Lookup result", normalizedText);
        }

        // 8. Provider sonucu varsa global içerik havuzuna LearningItem + Word + Meaning oluşturuyoruz.
        return await HandleProviderLookupResultAsync(
            keycloakUserId: keycloakUserId,
            request: request,
            normalizedText: normalizedText,
            sourceLanguage: sourceLanguage,
            targetLanguage: targetLanguage,
            providerResult: providerResult,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Database'de bulunan kelime için LookupHistory oluşturur ve LookupResponse döner.
    /// </summary>
    private async Task<LookupResponse> HandleDatabaseLookupResultAsync(
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

        return new LookupResponse
        {
            LearningItemId = databaseLookupData.LearningItem.Id,
            WordId = databaseLookupData.Word.Id,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = databaseLookupData.LearningItem.ItemType.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = DatabaseLookupSource,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = MapMeanings(databaseLookupData.Meanings)
        };
    }

    /// <summary>
    /// Provider'dan bulunan kelime için LearningItem + Word + Meaning + LookupHistory oluşturur.
    /// </summary>
    private async Task<LookupResponse> HandleProviderLookupResultAsync(
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

        return new LookupResponse
        {
            LearningItemId = learningItem.Id,
            WordId = word.Id,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = LearningItemType.Word.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = providerResult.ProviderName,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = MapMeanings(meanings)
        };
    }

    /// <summary>
    /// Phrase/Sentence gibi ilk prototipte desteklenmeyen inputlar için lookup history kaydı oluşturur.
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
    /// Database ve provider sonucunda bulunamayan kelimeler için lookup history oluşturur.
    /// </summary>
    private async Task SaveNotFoundLookupHistoryAsync(
        string keycloakUserId,
        CreateLookupCommand request,
        string normalizedText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        string providerName,
        CancellationToken cancellationToken)
    {
        var lookupHistory = CreateLookupHistory(
            keycloakUserId: keycloakUserId,
            queryText: request.Text,
            normalizedQueryText: normalizedText,
            inputType: LookupInputType.Word,
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
            MapToDomainInputType(inputType),
            sourceLanguageId,
            targetLanguageId,
            learningItemId,
            wasFoundInDatabase,
            wasCreatedFromProvider,
            providerType: null,
            providerName,
            resultCount);
    }

    /// <summary>
    /// Application lookup input type değerini Domain InputType enumuna çevirir.
    /// </summary>
    private static InputType MapToDomainInputType(LookupInputType inputType)
    {
        return inputType switch
        {
            LookupInputType.Word => InputType.Word,
            LookupInputType.Phrase => InputType.Phrase,
            LookupInputType.Sentence => InputType.Sentence,
            _ => InputType.Word
        };
    }

    /// <summary>
    /// Meaning entity listesini API response DTO listesine çevirir.
    /// </summary>
    private static IReadOnlyCollection<LookupMeaningResponse> MapMeanings(
        IReadOnlyCollection<Meaning> meanings)
    {
        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .Select(meaning => new LookupMeaningResponse
            {
                MeaningId = meaning.Id,
                Translation = meaning.MeaningText,
                Definition = meaning.ShortDefinition,
                ExampleSentence = null,
                PartOfSpeech = meaning.PartOfSpeech
            })
            .ToArray();
    }
}