using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Responses;
using Wordix.Domain.Entities;

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
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;

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
        IRepository<Meaning> meaningRepository,
        IRepository<Language> languageRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository)
    {
        _currentUserService = currentUserService;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _meaningRepository = meaningRepository;
        _languageRepository = languageRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
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

        // 5. İlk prototipte Word aktif olduğu için Word bilgisini LearningItemId üzerinden alıyoruz.
        // Phrase/Sentence desteği geldiğinde burası genişletilebilir.
        var word = await _wordRepository.FirstOrDefaultAsync(
            word => word.LearningItemId == learningItem.Id,
            cancellationToken);

        // 6. LearningItem'ın source language bilgisini alıyoruz.
        var sourceLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Id == learningItem.LanguageId,
            cancellationToken);

        // 7. LearningItem'a ait anlamları alıyoruz.
        var meanings = await _meaningRepository.ListAsync(
            meaning => meaning.LearningItemId == learningItem.Id,
            cancellationToken);

        // 8. Bu dictionary item'a ait progress kaydını alıyoruz.
        var progress = await _userLearningProgressRepository.FirstOrDefaultAsync(
            progress => progress.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        // 9. Seçili anlamı çözüyoruz.
        var selectedMeaning = ResolveSelectedMeaning(
            selectedMeaningId: userLearningItem.SelectedMeaningId,
            meanings: meanings);

        return new UserDictionaryItemResponse
        {
            UserLearningItemId = userLearningItem.Id,
            LearningItemId = learningItem.Id,
            WordId = word?.Id,
            ItemType = learningItem.ItemType.ToString(),
            DisplayText = ResolveDisplayText(word),
            NormalizedText = ResolveNormalizedText(word),
            SourceLanguageCode = sourceLanguage?.Code ?? string.Empty,
            SelectedMeaningId = selectedMeaning?.Id,
            SelectedMeaning = selectedMeaning is null
                ? null
                : MapMeaningToResponse(selectedMeaning),
            SavedAt = userLearningItem.SavedAt,
            SourceLookupHistoryId = userLearningItem.SourceLookupHistoryId,
            LearningStatus = progress?.LearningStatus.ToString() ?? string.Empty,
            LearningConfidenceScore = progress?.LearningConfidenceScore ?? 0,
            IsActive = userLearningItem.IsActive
        };
    }

    /// <summary>
    /// Kullanıcının seçtiği meaning'i çözer.
    /// 
    /// Öncelik sırası:
    /// 1. UserLearningItem.SelectedMeaningId ile eşleşen meaning
    /// 2. IsPrimary olan meaning
    /// 3. DisplayOrder'a göre ilk meaning
    /// 4. null
    /// </summary>
    private static Meaning? ResolveSelectedMeaning(
        Guid? selectedMeaningId,
        IReadOnlyCollection<Meaning> meanings)
    {
        if (meanings.Count == 0)
        {
            return null;
        }

        if (selectedMeaningId is not null)
        {
            var selectedMeaning = meanings.FirstOrDefault(
                meaning => meaning.Id == selectedMeaningId.Value);

            if (selectedMeaning is not null)
            {
                return selectedMeaning;
            }
        }

        var primaryMeaning = meanings.FirstOrDefault(meaning => meaning.IsPrimary);

        if (primaryMeaning is not null)
        {
            return primaryMeaning;
        }

        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .FirstOrDefault();
    }

    /// <summary>
    /// Word bilgisinden kullanıcıya gösterilecek ana metni çözer.
    /// 
    /// İlk prototipte sadece Word aktif.
    /// Phrase/Sentence geldiğinde bu method genişletilebilir.
    /// </summary>
    private static string ResolveDisplayText(Word? word)
    {
        return word?.Text ?? string.Empty;
    }

    /// <summary>
    /// Word bilgisinden normalize edilmiş metni çözer.
    /// </summary>
    private static string ResolveNormalizedText(Word? word)
    {
        return word?.NormalizedText ?? string.Empty;
    }

    /// <summary>
    /// Meaning entity'sini UserDictionaryMeaningResponse DTO'suna dönüştürür.
    /// </summary>
    private static UserDictionaryMeaningResponse MapMeaningToResponse(Meaning meaning)
    {
        return new UserDictionaryMeaningResponse
        {
            MeaningId = meaning.Id,
            Translation = meaning.MeaningText,
            Definition = meaning.ShortDefinition,
            PartOfSpeech = meaning.PartOfSpeech,
            IsPrimary = meaning.IsPrimary,
            DisplayOrder = meaning.DisplayOrder
        };
    }
}