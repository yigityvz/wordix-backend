using Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;
using Wordix.Application.Features.UserDictionary.Dtos.Requests;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;
using Wordix.Application.Features.UserDictionary.Commands.SaveSentenceToDictionary;

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
    /// API request DTO'sunu SaveSentenceToDictionaryCommand modeline dönüştürür.
    /// 
    /// Controller null body kontrolü yapmaz.
    /// Request null gelirse string alanlar boş gelir.
    /// SaveSentenceToDictionaryCommandValidator bunu ValidationBehavior üzerinden yakalar.
    /// </summary>
    public static SaveSentenceToDictionaryCommand ToSaveSentenceToDictionaryCommand(
        SaveSentenceToDictionaryRequest? request)
    {
        return new SaveSentenceToDictionaryCommand
        {
            SourceText = request?.SourceText ?? string.Empty,
            TranslatedText = request?.TranslatedText ?? string.Empty,
            SourceLanguageCode = request?.SourceLanguageCode ?? string.Empty,
            TargetLanguageCode = request?.TargetLanguageCode ?? string.Empty,
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
    /// Sentence dictionary kaydı oluşturulduktan sonra API'ye dönecek response modelini üretir.
    /// 
    /// Bu method sadece mapping yapar.
    /// Repository kullanmaz.
    /// SaveChanges çağırmaz.
    /// </summary>
    public static SaveSentenceToDictionaryResponse ToSaveSentenceToDictionaryResponse(
        UserLearningItem userLearningItem,
        LearningItem learningItem,
        Sentence sentence,
        SentenceTranslation sentenceTranslation,
        UserLearningProgress userLearningProgress)
    {
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(sentence);
        ArgumentNullException.ThrowIfNull(sentenceTranslation);
        ArgumentNullException.ThrowIfNull(userLearningProgress);

        return new SaveSentenceToDictionaryResponse
        {
            UserLearningItemId = userLearningItem.Id,
            LearningItemId = learningItem.Id,
            SentenceId = sentence.Id,
            SentenceTranslationId = sentenceTranslation.Id,
            SourceText = sentence.Text,
            NormalizedSourceText = sentence.NormalizedText,
            TranslatedText = sentenceTranslation.TranslatedText,
            NormalizedTranslatedText = sentenceTranslation.NormalizedTranslatedText,
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
        IReadOnlyDictionary<Guid, Phrase> phraseLookup,
        IReadOnlyDictionary<Guid, Sentence> sentenceLookup,
        IReadOnlyDictionary<Guid, Meaning[]> meaningsByLearningItemId,
        IReadOnlyDictionary<Guid, SentenceTranslation[]> sentenceTranslationsBySentenceId,
        IReadOnlyDictionary<Guid, UserLearningProgress> progressLookup,
        IReadOnlyDictionary<Guid, Language> languageLookup)
    {
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItemLookup);
        ArgumentNullException.ThrowIfNull(wordLookup);
        ArgumentNullException.ThrowIfNull(phraseLookup);
        ArgumentNullException.ThrowIfNull(sentenceLookup);
        ArgumentNullException.ThrowIfNull(sentenceTranslationsBySentenceId);
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
        phraseLookup.TryGetValue(learningItem.Id, out var phrase);
        sentenceLookup.TryGetValue(learningItem.Id, out var sentence);
        languageLookup.TryGetValue(learningItem.LanguageId, out var sourceLanguage);
        meaningsByLearningItemId.TryGetValue(learningItem.Id, out var meanings);
        progressLookup.TryGetValue(userLearningItem.Id, out var progress);

        SentenceTranslation? sentenceTranslation = null;
        Language? targetLanguage = null;

        if (sentence is not null &&
            sentenceTranslationsBySentenceId.TryGetValue(sentence.Id, out var sentenceTranslations))
        {
            sentenceTranslation = ResolveSentenceTranslation(sentenceTranslations);

            if (sentenceTranslation is not null)
            {
                languageLookup.TryGetValue(sentenceTranslation.TargetLanguageId, out targetLanguage);
            }
        }

        return ToUserDictionaryItemResponse(
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            word: word,
            phrase: phrase,
            sentence: sentence,
            sourceLanguage: sourceLanguage,
            meanings: meanings ?? Array.Empty<Meaning>(),
            sentenceTranslation: sentenceTranslation,
            targetLanguage: targetLanguage,
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
        Phrase? phrase,
        Sentence? sentence,
        Language? sourceLanguage,
        IReadOnlyCollection<Meaning> meanings,
        SentenceTranslation? sentenceTranslation,
        Language? targetLanguage,
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
            PhraseId = phrase?.Id,
            SentenceId = sentence?.Id,
            ItemType = learningItem.ItemType.ToString(),
            DisplayText = ResolveDisplayText(learningItem, word, phrase, sentence),
            NormalizedText = ResolveNormalizedText(learningItem, word, phrase, sentence),
            SourceLanguageCode = sourceLanguage?.Code ?? string.Empty,
            SelectedMeaningId = selectedMeaning?.Id,
            SelectedMeaning = selectedMeaning is null
        ? null
        : ToUserDictionaryMeaningResponse(selectedMeaning),
            SentenceTranslation = sentenceTranslation is null
        ? null
        : ToUserDictionarySentenceTranslationResponse(sentenceTranslation, targetLanguage),
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
    /// SentenceTranslation entity'sini UserDictionarySentenceTranslationResponse DTO'suna dönüştürür.
    /// </summary>
    public static UserDictionarySentenceTranslationResponse ToUserDictionarySentenceTranslationResponse(
        SentenceTranslation sentenceTranslation,
        Language? targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(sentenceTranslation);

        return new UserDictionarySentenceTranslationResponse
        {
            SentenceTranslationId = sentenceTranslation.Id,
            TranslatedText = sentenceTranslation.TranslatedText,
            TargetLanguageCode = targetLanguage?.Code ?? string.Empty,
            IsPrimary = sentenceTranslation.IsPrimary,
            DisplayOrder = sentenceTranslation.DisplayOrder
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
    /// Sentence için gösterilecek ana çeviriyi çözer.
    /// 
    /// Öncelik sırası:
    /// 1. IsPrimary olan translation
    /// 2. DisplayOrder'a göre ilk translation
    /// 3. null
    /// </summary>
    private static SentenceTranslation? ResolveSentenceTranslation(
        IReadOnlyCollection<SentenceTranslation> sentenceTranslations)
    {
        if (sentenceTranslations.Count == 0)
        {
            return null;
        }

        var primaryTranslation = sentenceTranslations.FirstOrDefault(
            translation => translation.IsPrimary);

        if (primaryTranslation is not null)
        {
            return primaryTranslation;
        }

        return sentenceTranslations
            .OrderBy(translation => translation.DisplayOrder)
            .FirstOrDefault();
    }


    private static string ResolveDisplayText(
        LearningItem learningItem,
        Word? word,
        Phrase? phrase,
        Sentence? sentence)
    {
        return learningItem.ItemType switch
        {
            LearningItemType.Word => word?.Text ?? string.Empty,
            LearningItemType.Phrase => phrase?.Text ?? string.Empty,
            LearningItemType.Sentence => sentence?.Text ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// LearningItem tipine göre normalize edilmiş metni çözer.
    /// 
    /// Word için Word.NormalizedText,
    /// Phrase için Phrase.NormalizedText kullanılır.
    /// </summary>
    private static string ResolveNormalizedText(
        LearningItem learningItem,
        Word? word,
        Phrase? phrase,
        Sentence? sentence)
    {
        return learningItem.ItemType switch
        {
            LearningItemType.Word => word?.NormalizedText ?? string.Empty,
            LearningItemType.Phrase => phrase?.NormalizedText ?? string.Empty,
            LearningItemType.Sentence => sentence?.NormalizedText ?? string.Empty,
            _ => string.Empty
        };
    }
}