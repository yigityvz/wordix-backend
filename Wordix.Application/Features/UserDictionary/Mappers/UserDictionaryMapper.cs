using Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;
using Wordix.Application.Features.UserDictionary.Dtos.Requests;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Mappers;

/// <summary>
/// UserDictionary feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu sınıf neden var?
/// - Controller içinde Request DTO → Command dönüşümü yapmak istemiyoruz.
/// - Handler içinde Response DTO propertylerini tek tek dizmek istemiyoruz.
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi kullanmak istemiyoruz.
/// - Bu yüzden açık, okunabilir ve kontrollü bir mapper kullanıyoruz.
/// 
/// Bu yaklaşım:
/// - Magic mapping değildir.
/// - Hangi alanın nereye gittiği açıkça görülür.
/// - Controller'ı sadeleştirir.
/// - Handler'ı use-case akışına odaklı tutar.
/// - Mapping kurallarını feature seviyesinde tek yerde toplar.
/// </summary>
public static class UserDictionaryMapper
{
    /// <summary>
    /// API request DTO'sunu SaveLearningItemCommand modeline dönüştürür.
    /// 
    /// Controller null body kontrolü yapmaz.
    /// Request null gelirse LearningItemId Guid.Empty olur.
    /// SaveLearningItemCommandValidator bunu ValidationBehavior üzerinden yakalar.
    /// </summary>
    public static SaveLearningItemCommand ToSaveLearningItemCommand(
        SaveLearningItemRequest? request)
    {
        return new SaveLearningItemCommand
        {
            LearningItemId = request?.LearningItemId ?? Guid.Empty,
            SelectedMeaningId = request?.SelectedMeaningId,
            SourceLookupHistoryId = request?.SourceLookupHistoryId
        };
    }

    /// <summary>
    /// Yeni dictionary kaydı oluşturulduktan sonra API'ye dönecek response modelini üretir.
    /// 
    /// Bu method sadece mapping yapar.
    /// Repository kullanmaz.
    /// SaveChanges çağırmaz.
    /// </summary>
    public static SaveLearningItemResponse ToSaveLearningItemResponse(
        UserLearningItem userLearningItem,
        LearningItem learningItem,
        UserLearningProgress userLearningProgress)
    {
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(userLearningProgress);

        return new SaveLearningItemResponse
        {
            UserLearningItemId = userLearningItem.Id,
            LearningItemId = learningItem.Id,
            SelectedMeaningId = userLearningItem.SelectedMeaningId,
            UserLearningProgressId = userLearningProgress.Id,
            SourceLookupHistoryId = userLearningItem.SourceLookupHistoryId,
            SavedAt = userLearningItem.SavedAt,
            LearningStatus = userLearningProgress.LearningStatus.ToString(),
            LearningConfidenceScore = userLearningProgress.LearningConfidenceScore,
            IsActive = userLearningItem.IsActive
        };
    }

    /// <summary>
    /// Boş dictionary listesi için standart response üretir.
    /// </summary>
    public static GetMyDictionaryResponse ToEmptyGetMyDictionaryResponse()
    {
        return new GetMyDictionaryResponse
        {
            TotalCount = 0,
            Items = Array.Empty<UserDictionaryItemResponse>()
        };
    }

    /// <summary>
    /// Dictionary item response listesini GetMyDictionaryResponse modeline sarar.
    /// </summary>
    public static GetMyDictionaryResponse ToGetMyDictionaryResponse(
        IReadOnlyCollection<UserDictionaryItemResponse> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return new GetMyDictionaryResponse
        {
            TotalCount = items.Count,
            Items = items
        };
    }

    /// <summary>
    /// Liste ekranı için UserLearningItem merkezli dictionary item response üretir.
    /// 
    /// Bu overload, toplu sorgular sonucunda oluşturulan lookup dictionary'leri kullanır.
    /// Böylece handler veri toplama işini yapar, response mapping işini mapper'a bırakır.
    /// </summary>
    public static UserDictionaryItemResponse? ToUserDictionaryItemResponse(
        UserLearningItem userLearningItem,
        IReadOnlyDictionary<Guid, LearningItem> learningItemLookup,
        IReadOnlyDictionary<Guid, Word> wordLookup,
        IReadOnlyDictionary<Guid, Meaning[]> meaningsByLearningItemId,
        IReadOnlyDictionary<Guid, UserLearningProgress> progressLookup,
        IReadOnlyDictionary<Guid, Language> languageLookup)
    {
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItemLookup);
        ArgumentNullException.ThrowIfNull(wordLookup);
        ArgumentNullException.ThrowIfNull(meaningsByLearningItemId);
        ArgumentNullException.ThrowIfNull(progressLookup);
        ArgumentNullException.ThrowIfNull(languageLookup);

        // İlgili LearningItem bulunamazsa bu kayıt response'a dahil edilmez.
        // Normalde FK sayesinde böyle bir durum olmamalı.
        if (!learningItemLookup.TryGetValue(userLearningItem.LearningItemId, out var learningItem))
        {
            return null;
        }

        wordLookup.TryGetValue(learningItem.Id, out var word);
        languageLookup.TryGetValue(learningItem.LanguageId, out var sourceLanguage);
        meaningsByLearningItemId.TryGetValue(learningItem.Id, out var meanings);
        progressLookup.TryGetValue(userLearningItem.Id, out var progress);

        return ToUserDictionaryItemResponse(
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            word: word,
            sourceLanguage: sourceLanguage,
            meanings: meanings ?? Array.Empty<Meaning>(),
            progress: progress);
    }

    /// <summary>
    /// Detay ekranı için UserLearningItem, LearningItem, Word, Language, Meaning ve Progress
    /// bilgilerinden UserDictionaryItemResponse üretir.
    /// 
    /// Bu overload, GetUserDictionaryItemByIdQueryHandler tarafından kullanılır.
    /// </summary>
    public static UserDictionaryItemResponse ToUserDictionaryItemResponse(
        UserLearningItem userLearningItem,
        LearningItem learningItem,
        Word? word,
        Language? sourceLanguage,
        IReadOnlyCollection<Meaning> meanings,
        UserLearningProgress? progress)
    {
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(meanings);

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
                : ToUserDictionaryMeaningResponse(selectedMeaning),
            SavedAt = userLearningItem.SavedAt,
            SourceLookupHistoryId = userLearningItem.SourceLookupHistoryId,
            LearningStatus = progress?.LearningStatus.ToString() ?? string.Empty,
            LearningConfidenceScore = progress?.LearningConfidenceScore ?? 0,
            IsActive = userLearningItem.IsActive
        };
    }

    /// <summary>
    /// Meaning entity'sini UserDictionaryMeaningResponse DTO'suna dönüştürür.
    /// </summary>
    public static UserDictionaryMeaningResponse ToUserDictionaryMeaningResponse(
        Meaning meaning)
    {
        ArgumentNullException.ThrowIfNull(meaning);

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

    /// <summary>
    /// Kullanıcının seçtiği meaning'i çözer.
    /// 
    /// Öncelik sırası:
    /// 1. UserLearningItem.SelectedMeaningId ile eşleşen meaning
    /// 2. IsPrimary olan meaning
    /// 3. DisplayOrder'a göre ilk meaning
    /// 4. null
    /// 
    /// Bu kural response mapping kuralıdır.
    /// Bu yüzden handler içinde değil mapper içinde tutulur.
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
}