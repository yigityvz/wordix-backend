using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Responses;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;

/// <summary>
/// GetMyDictionaryQuery isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın UserProfile kaydını alır.
/// - Kullanıcının aktif dictionary kayıtlarını getirir.
/// - Her dictionary kaydı için LearningItem, Word, Meaning, Language ve Progress bilgilerini toplar.
/// - API'ye dönecek GetMyDictionaryResponse modelini üretir.
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
    private readonly IUserProfileSyncService _userProfileSyncService;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Word> _wordRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;

    /// <summary>
    /// Handler ihtiyacı olan repository ve servisleri DI üzerinden alır.
    /// 
    /// Burada DbContext inject etmiyoruz.
    /// Bu sayede Application katmanı Persistence detaylarını bilmez.
    /// </summary>
    public GetMyDictionaryQueryHandler(
        IUserProfileSyncService userProfileSyncService,
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Word> wordRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<Language> languageRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository)
    {
        _userProfileSyncService = userProfileSyncService;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _wordRepository = wordRepository;
        _meaningRepository = meaningRepository;
        _languageRepository = languageRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
    }

    /// <summary>
    /// Current user'ın kendi dictionary listesini döner.
    /// </summary>
    public async Task<GetMyDictionaryResponse> Handle(
        GetMyDictionaryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın Wordix UserProfile kaydını alıyoruz.
        // Kullanıcı kimliği request'ten değil, token üzerinden bulunur.
        var userProfile = await _userProfileSyncService
            .GetOrCreateCurrentUserProfileAsync(cancellationToken);

        // 2. Kullanıcının aktif dictionary kayıtlarını getiriyoruz.
        // Bu method Faz 9'da özel repository olarak eklenmişti.
        var userLearningItems = await _userLearningItemRepository
            .GetActiveItemsByUserAsync(userProfile.Id, cancellationToken);

        // Dictionary boşsa gereksiz database sorguları yapmadan boş response döneriz.
        if (userLearningItems.Count == 0)
        {
            return new GetMyDictionaryResponse
            {
                TotalCount = 0,
                Items = Array.Empty<UserDictionaryItemResponse>()
            };
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

        // 5. İlk prototipte aktif olarak Word destekliyoruz.
        // Bu yüzden Word detaylarını LearningItemId üzerinden toplu çekiyoruz.
        var words = await _wordRepository.ListAsync(
            word => learningItemIds.Contains(word.LearningItemId),
            cancellationToken);

        var wordLookup = words.ToDictionary(word => word.LearningItemId);

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

        // 7. Progress kayıtlarını toplu alıyoruz.
        // UserLearningProgress, UserLearningItemId üzerinden bire bir bağlıdır.
        var progresses = await _userLearningProgressRepository.ListAsync(
            progress => userLearningItemIds.Contains(progress.UserLearningItemId),
            cancellationToken);

        var progressLookup = progresses.ToDictionary(progress => progress.UserLearningItemId);

        // 8. Source language bilgilerini toplu almak için LearningItem.LanguageId değerlerini çıkarıyoruz.
        var languageIds = learningItems
            .Select(item => item.LanguageId)
            .Distinct()
            .ToArray();

        var languages = await _languageRepository.ListAsync(
            language => languageIds.Contains(language.Id),
            cancellationToken);

        var languageLookup = languages.ToDictionary(language => language.Id);

        // 9. UserLearningItem merkezli response mapping yapıyoruz.
        var responseItems = userLearningItems
            .OrderByDescending(item => item.SavedAt)
            .Select(item => MapToResponse(
                userLearningItem: item,
                learningItemLookup: learningItemLookup,
                wordLookup: wordLookup,
                meaningsByLearningItemId: meaningsByLearningItemId,
                progressLookup: progressLookup,
                languageLookup: languageLookup))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();

        return new GetMyDictionaryResponse
        {
            TotalCount = responseItems.Length,
            Items = responseItems
        };
    }

    /// <summary>
    /// UserLearningItem entity'sini API response DTO'suna dönüştürür.
    /// 
    /// Bu mapping manual yapılıyor.
    /// AutoMapper/Mapster kullanmıyoruz.
    /// </summary>
    private static UserDictionaryItemResponse? MapToResponse(
        UserLearningItem userLearningItem,
        IReadOnlyDictionary<Guid, LearningItem> learningItemLookup,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Meaning[]> meaningsByLearningItemId,
        IReadOnlyDictionary<Guid, UserLearningProgress> progressLookup,
        IReadOnlyDictionary<Guid, Language> languageLookup)
    {
        // İlgili LearningItem bulunamazsa bu kayıt response'a dahil edilmez.
        // Normalde FK sayesinde böyle bir durum olmamalı.
        if (!learningItemLookup.TryGetValue(userLearningItem.LearningItemId, out var learningItem))
        {
            return null;
        }

        // İlk prototipte Word aktif.
        // Phrase/Sentence geldiğinde burası genişletilebilir.
        wordLookup.TryGetValue(learningItem.Id, out var word);

        // LearningItem'ın source language kodunu buluyoruz.
        languageLookup.TryGetValue(learningItem.LanguageId, out var sourceLanguage);

        // Bu LearningItem'a ait meaning listesini alıyoruz.
        meaningsByLearningItemId.TryGetValue(learningItem.Id, out var meanings);

        // Kullanıcının seçtiği meaning varsa onu, yoksa primary meaning'i seçiyoruz.
        var selectedMeaning = ResolveSelectedMeaning(
            selectedMeaningId: userLearningItem.SelectedMeaningId,
            meanings: meanings);

        // Progress kaydı normalde SaveLearningItemCommandHandler tarafından oluşturulur.
        // Yine de null güvenliği için TryGetValue kullanıyoruz.
        progressLookup.TryGetValue(userLearningItem.Id, out var progress);

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
        IReadOnlyCollection<Meaning>? meanings)
    {
        if (meanings is null || meanings.Count == 0)
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
    /// İlk prototipte sadece Word aktif olduğu için Word.Text kullanıyoruz.
    /// Phrase/Sentence desteği geldiğinde burası yeni içerik tiplerine göre genişletilebilir.
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