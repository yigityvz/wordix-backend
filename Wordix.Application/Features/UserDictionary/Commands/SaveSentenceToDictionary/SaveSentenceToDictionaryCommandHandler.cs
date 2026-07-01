using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Localization;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveSentenceToDictionary;

/// <summary>
/// SaveSentenceToDictionaryCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Source/translated text değerlerini normalize eder.
/// - Source/target language kayıtlarını çözer.
/// - SourceLookupHistoryId gönderildiyse ownership ve Sentence uyumunu kontrol eder.
/// - Global sentence kaydı var mı bakar.
/// - Yoksa LearningItem + Sentence oluşturur.
/// - SentenceTranslation var mı bakar.
/// - Yoksa SentenceTranslation oluşturur.
/// - Kullanıcının bu LearningItem'ı daha önce kaydedip kaydetmediğini kontrol eder.
/// - UserLearningItem + UserLearningProgress + LearningProgressHistory oluşturur.
/// - SaveSentenceToDictionaryResponse döner.
/// 
/// Faz 19 kararı:
/// Sentence lookup anında kalıcı kayıt oluşturulmaz.
/// Kalıcı kayıt sadece kullanıcı dictionary'ye kaydetmek isterse burada oluşturulur.
/// </summary>
public sealed class SaveSentenceToDictionaryCommandHandler
    : IRequestHandler<SaveSentenceToDictionaryCommand, SaveSentenceToDictionaryResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITextNormalizer _textNormalizer;
    private readonly ILanguageResolver _languageResolver;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Sentence> _sentenceRepository;
    private readonly IRepository<SentenceTranslation> _sentenceTranslationRepository;
    private readonly IRepository<LookupHistory> _lookupHistoryRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemGenericRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;
    private readonly IRepository<LearningProgressHistory> _learningProgressHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveSentenceToDictionaryCommandHandler(
        ICurrentUserService currentUserService,
        ITextNormalizer textNormalizer,
        ILanguageResolver languageResolver,
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Sentence> sentenceRepository,
        IRepository<SentenceTranslation> sentenceTranslationRepository,
        IRepository<LookupHistory> lookupHistoryRepository,
        IRepository<UserLearningItem> userLearningItemGenericRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository,
        IRepository<LearningProgressHistory> learningProgressHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _textNormalizer = textNormalizer;
        _languageResolver = languageResolver;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _sentenceRepository = sentenceRepository;
        _sentenceTranslationRepository = sentenceTranslationRepository;
        _lookupHistoryRepository = lookupHistoryRepository;
        _userLearningItemGenericRepository = userLearningItemGenericRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
        _learningProgressHistoryRepository = learningProgressHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SaveSentenceToDictionaryResponse> Handle(
        SaveSentenceToDictionaryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Kaynak cümle ve çeviri metnini normalize ediyoruz.
        var normalizedSourceText = _textNormalizer.Normalize(request.SourceText);
        var normalizedTranslatedText = _textNormalizer.Normalize(request.TranslatedText);

        // 3. Source/target language kayıtlarını merkezi resolver üzerinden çözüyoruz.
        var sourceLanguage = await _languageResolver.GetRequiredActiveLanguageByCodeAsync(
            request.SourceLanguageCode,
            cancellationToken);

        var targetLanguage = await _languageResolver.GetRequiredActiveLanguageByCodeAsync(
            request.TargetLanguageCode,
            cancellationToken);

        // 4. SourceLookupHistoryId gönderildiyse:
        // - current user'a ait mi?
        // - InputType Sentence mı?
        // - source/target language uyumlu mu?
        // - normalized query, source text ile aynı mı?
        await EnsureSourceLookupHistoryIsValidForSentenceSaveAsync(
            sourceLookupHistoryId: request.SourceLookupHistoryId,
            keycloakUserId: keycloakUserId,
            normalizedSourceText: normalizedSourceText,
            sourceLanguageId: sourceLanguage.Id,
            targetLanguageId: targetLanguage.Id,
            cancellationToken: cancellationToken);

        // 5. Aynı source language + normalized source text için global sentence var mı bakıyoruz.
        var sentence = await GetSentenceOrNullAsync(
            sourceLanguageId: sourceLanguage.Id,
            normalizedSourceText: normalizedSourceText,
            cancellationToken: cancellationToken);

        LearningItem learningItem;

        if (sentence is null)
        {
            // Sentence daha önce global havuzda yoksa yeni öğrenilebilir sentence oluşturuyoruz.
            learningItem = new LearningItem(
                LearningItemType.Sentence,
                sourceLanguage.Id,
                CefrLevel.A1,
                DifficultyGroup.Beginner,
                LearningItemSourceType.UserLookup);

            sentence = new Sentence(
                learningItemId: learningItem.Id,
                languageId: sourceLanguage.Id,
                text: request.SourceText,
                normalizedText: normalizedSourceText,
                externalSentenceId: null,
                sourceProvider: "UserLookup",
                license: null);

            await _learningItemRepository.AddAsync(learningItem, cancellationToken);
            await _sentenceRepository.AddAsync(sentence, cancellationToken);
        }
        else
        {
            // Sentence varsa ama LearningItemId yoksa,
            // bu sentence daha önce örnek/import verisi gibi saklanmış olabilir.
            // Kullanıcı bunu öğrenilecek içerik olarak kaydettiği için LearningItem'a bağlıyoruz.
            if (sentence.LearningItemId is null)
            {
                learningItem = new LearningItem(
                    LearningItemType.Sentence,
                    sourceLanguage.Id,
                    CefrLevel.A1,
                    DifficultyGroup.Beginner,
                    LearningItemSourceType.UserLookup);

                sentence.AttachToLearningItem(learningItem.Id);

                await _learningItemRepository.AddAsync(learningItem, cancellationToken);
            }
            else
            {
                learningItem = await GetRequiredActiveLearningItemAsync(
                    sentence.LearningItemId.Value,
                    cancellationToken);
            }
        }

        // 6. Kullanıcı bu sentence LearningItem'ını daha önce kaydetmiş mi kontrol ediyoruz.
        var alreadySaved = await _userLearningItemRepository.ExistsByUserAndLearningItemAsync(
            keycloakUserId,
            learningItem.Id,
            cancellationToken);

        if (alreadySaved)
        {
            throw new BusinessRuleException(
                "This sentence is already saved in your dictionary.",
                "SENTENCE_ALREADY_SAVED");
        }

        // 7. Aynı sentence için aynı hedef dilde aynı çeviri var mı bakıyoruz.
        var sentenceTranslation = await GetSentenceTranslationOrNullAsync(
            sentenceId: sentence.Id,
            targetLanguageId: targetLanguage.Id,
            normalizedTranslatedText: normalizedTranslatedText,
            cancellationToken: cancellationToken);

        if (sentenceTranslation is null)
        {
            sentenceTranslation = new SentenceTranslation(
                sourceSentenceId: sentence.Id,
                targetLanguageId: targetLanguage.Id,
                translatedText: request.TranslatedText,
                normalizedTranslatedText: normalizedTranslatedText,
                sourceProvider: "UserLookup",
                license: null,
                isPrimary: true,
                displayOrder: 1);

            await _sentenceTranslationRepository.AddAsync(
                sentenceTranslation,
                cancellationToken);
        }

        // 8. UserLearningItem oluşturuyoruz.
        // Sentence için SelectedMeaningId yoktur; null gönderiyoruz.
        var userLearningItem = new UserLearningItem(
            keycloakUserId,
            learningItem.Id,
            selectedMeaningId: null,
            sourceLookupHistoryId: request.SourceLookupHistoryId);

        // 9. Progress ve progress history başlangıç kaydı oluşturuyoruz.
        var userLearningProgress = new UserLearningProgress(userLearningItem.Id);

        var progressHistory = new LearningProgressHistory(
            userLearningProgress.Id,
            userLearningProgress.LearningStatus,
            userLearningProgress.LearningStatus,
            userLearningProgress.LearningConfidenceScore,
            userLearningProgress.LearningConfidenceScore,
            "Sentence saved to dictionary.");

        await _userLearningItemGenericRepository.AddAsync(
            userLearningItem,
            cancellationToken);

        await _userLearningProgressRepository.AddAsync(
            userLearningProgress,
            cancellationToken);

        await _learningProgressHistoryRepository.AddAsync(
            progressHistory,
            cancellationToken);

        // 10. Tek SaveChanges ile tüm kayıtları birlikte kaydediyoruz.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDictionaryMapper.ToSaveSentenceToDictionaryResponse(
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            sentence: sentence,
            sentenceTranslation: sentenceTranslation,
            userLearningProgress: userLearningProgress);
    }

    /// <summary>
    /// Source language + normalized source text ile sentence arar.
    /// </summary>
    private async Task<Sentence?> GetSentenceOrNullAsync(
        Guid sourceLanguageId,
        string normalizedSourceText,
        CancellationToken cancellationToken)
    {
        return await _sentenceRepository.FirstOrDefaultAsync(
            sentence =>
                sentence.LanguageId == sourceLanguageId &&
                sentence.NormalizedText == normalizedSourceText,
            cancellationToken);
    }

    /// <summary>
    /// LearningItem var mı, aktif mi ve gerçekten Sentence tipinde mi kontrol eder.
    /// </summary>
    private async Task<LearningItem> GetRequiredActiveLearningItemAsync(
        Guid learningItemId,
        CancellationToken cancellationToken)
    {
        var learningItem = await _learningItemRepository.FirstOrDefaultAsync(
            item => item.Id == learningItemId && item.IsActive,
            cancellationToken);

        if (learningItem is null)
        {
            throw new NotFoundException("Learning item", learningItemId);
        }

        if (learningItem.ItemType != LearningItemType.Sentence)
        {
            throw new BusinessRuleException(
                "The learning item attached to this sentence is not a sentence item.",
                "LEARNING_ITEM_TYPE_MISMATCH");
        }

        return learningItem;
    }

    /// <summary>
    /// Aynı sentence + hedef dil + normalize çeviri var mı kontrol eder.
    /// </summary>
    private async Task<SentenceTranslation?> GetSentenceTranslationOrNullAsync(
        Guid sentenceId,
        Guid targetLanguageId,
        string normalizedTranslatedText,
        CancellationToken cancellationToken)
    {
        return await _sentenceTranslationRepository.FirstOrDefaultAsync(
            translation =>
                translation.SourceSentenceId == sentenceId &&
                translation.TargetLanguageId == targetLanguageId &&
                translation.NormalizedTranslatedText == normalizedTranslatedText,
            cancellationToken);
    }

    /// <summary>
    /// SourceLookupHistoryId verildiyse sentence save akışı için uygun mu kontrol eder.
    /// 
    /// Kontroller:
    /// - LookupHistory var mı?
    /// - Current user'a ait mi?
    /// - InputType Sentence mı?
    /// - Source/target language uyumlu mu?
    /// - Normalized query text, kaydedilmek istenen source sentence ile aynı mı?
    /// </summary>
    private async Task EnsureSourceLookupHistoryIsValidForSentenceSaveAsync(
        Guid? sourceLookupHistoryId,
        string keycloakUserId,
        string normalizedSourceText,
        Guid sourceLanguageId,
        Guid targetLanguageId,
        CancellationToken cancellationToken)
    {
        if (sourceLookupHistoryId is null)
        {
            return;
        }

        var lookupHistory = await _lookupHistoryRepository.FirstOrDefaultAsync(
            history => history.Id == sourceLookupHistoryId.Value,
            cancellationToken);

        if (lookupHistory is null)
        {
            throw new NotFoundException("Lookup history", sourceLookupHistoryId.Value);
        }

        if (!string.Equals(
                lookupHistory.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot use another user's lookup history as save source.");
        }

        if (lookupHistory.InputType != InputType.Sentence)
        {
            throw new BusinessRuleException(
                "Source lookup history must belong to a sentence lookup.",
                "SOURCE_LOOKUP_HISTORY_MUST_BE_SENTENCE");
        }

        if (lookupHistory.SourceLanguageId != sourceLanguageId ||
            lookupHistory.TargetLanguageId != targetLanguageId)
        {
            throw new BusinessRuleException(
                "Source lookup history language pair does not match the sentence save request.",
                "SOURCE_LOOKUP_HISTORY_LANGUAGE_PAIR_MISMATCH");
        }

        if (!string.Equals(
                lookupHistory.NormalizedQueryText,
                normalizedSourceText,
                StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "Source lookup history text does not match the sentence being saved.",
                "SOURCE_LOOKUP_HISTORY_TEXT_MISMATCH");
        }
    }
}