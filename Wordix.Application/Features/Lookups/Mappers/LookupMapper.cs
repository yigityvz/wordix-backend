using Wordix.Application.Common.Models.Localization;
using Wordix.Application.Common.Models.Persistence;
using Wordix.Application.Features.Lookups.Commands.CreateLookup;
using Wordix.Application.Features.Lookups.Dtos.Requests;
using Wordix.Application.Features.Lookups.Dtos.Responses;
using Wordix.Application.Features.Lookups.Models;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Lookups.Mappers;

/// <summary>
/// Lookup feature'ına ait explicit mapping işlemlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu sınıf neden var?
/// - Controller içinde property property manual mapping yapmak istemiyoruz.
/// - Handler içinde response DTO propertylerini tek tek dizmek istemiyoruz.
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi de kullanmak istemiyoruz.
/// - Bu yüzden explicit, okunabilir ve kontrollü bir mapper kullanıyoruz.
/// 
/// Bu yaklaşım:
/// - Magic mapping değildir.
/// - Hangi alanın nereye gittiği açıkça görülür.
/// - Controller'ı sadeleştirir.
/// - Handler'ı use-case akışına odaklı tutar.
/// - Mapping kurallarını feature seviyesinde tek yerde toplar.
/// </summary>
public static class LookupMapper
{
    private const string DatabaseLookupSource = "Database";

    /// <summary>
    /// API request DTO'sunu CreateLookupCommand modeline dönüştürür.
    /// 
    /// Dikkat:
    /// Controller içinde null kontrolü yapmıyoruz.
    /// Eğer request null gelirse boş string değerleriyle command oluşturuyoruz.
    /// Bu sayede validation işlemi controller'da değil,
    /// ValidationBehavior + CreateLookupCommandValidator tarafında yapılır.
    /// </summary>
    public static CreateLookupCommand ToCreateLookupCommand(
        LookupRequest? request)
    {
        return new CreateLookupCommand
        {
            Text = request?.Text ?? string.Empty,
            SourceLanguageCode = request?.SourceLanguageCode ?? string.Empty,
            TargetLanguageCode = request?.TargetLanguageCode ?? string.Empty
        };
    }

    /// <summary>
    /// Database'den bulunan word lookup sonucunu API response DTO'suna dönüştürür.
    /// </summary>
    public static LookupResponse ToDatabaseLookupResponse(
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        WordLookupData databaseLookupData,
        LookupHistory lookupHistory,
        bool isAlreadyInUserDictionary)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(databaseLookupData);
        ArgumentNullException.ThrowIfNull(lookupHistory);

        return new LookupResponse
        {
            LearningItemId = databaseLookupData.LearningItem.Id,
            WordId = databaseLookupData.Word.Id,
            PhraseId = null,
            SentenceId = null,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = databaseLookupData.LearningItem.ItemType.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = DatabaseLookupSource,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = ToLookupMeaningResponses(databaseLookupData.Meanings),
            SentenceTranslations = Array.Empty<LookupSentenceTranslationResponse>()
        };
    }

    /// <summary>
    /// Database'den bulunan phrase lookup sonucunu API response DTO'suna dönüştürür.
    /// </summary>
    public static LookupResponse ToDatabaseLookupResponse(
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        PhraseLookupData databaseLookupData,
        LookupHistory lookupHistory,
        bool isAlreadyInUserDictionary)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(databaseLookupData);
        ArgumentNullException.ThrowIfNull(lookupHistory);

        return new LookupResponse
        {
            LearningItemId = databaseLookupData.LearningItem.Id,
            WordId = null,
            PhraseId = databaseLookupData.Phrase.Id,
            SentenceId = null,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = databaseLookupData.LearningItem.ItemType.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = DatabaseLookupSource,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = ToLookupMeaningResponses(databaseLookupData.Meanings),
            SentenceTranslations = Array.Empty<LookupSentenceTranslationResponse>()
        };
    }

    /// <summary>
    /// Provider'dan gelen ve sisteme yeni eklenen word lookup sonucunu API response DTO'suna dönüştürür.
    /// 
    /// Bu method LearningItem, Word, Meaning ve LookupHistory entity'lerini oluşturmaz.
    /// Onlar handler/use-case tarafında oluşturulur.
    /// Bu method sadece oluşmuş nesneleri response modeline map eder.
    /// </summary>
    public static LookupResponse ToProviderLookupResponse(
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        LearningItem learningItem,
        Word word,
        IReadOnlyCollection<Meaning> meanings,
        LookupHistory lookupHistory,
        bool isAlreadyInUserDictionary)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(providerResult);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(word);
        ArgumentNullException.ThrowIfNull(meanings);
        ArgumentNullException.ThrowIfNull(lookupHistory);

        return new LookupResponse
        {
            LearningItemId = learningItem.Id,
            WordId = word.Id,
            PhraseId = null,
            SentenceId = null,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = learningItem.ItemType.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = providerResult.ProviderName,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = ToLookupMeaningResponses(meanings),
            SentenceTranslations = Array.Empty<LookupSentenceTranslationResponse>()
        };
    }

    /// <summary>
    /// Provider'dan gelen ve sisteme yeni eklenen phrase lookup sonucunu API response DTO'suna dönüştürür.
    /// 
    /// Bu method LearningItem, Phrase, Meaning ve LookupHistory entity'lerini oluşturmaz.
    /// Onlar handler/use-case tarafında oluşturulur.
    /// Bu method sadece oluşmuş nesneleri response modeline map eder.
    /// </summary>
    public static LookupResponse ToProviderLookupResponse(
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        LearningItem learningItem,
        Phrase phrase,
        IReadOnlyCollection<Meaning> meanings,
        LookupHistory lookupHistory,
        bool isAlreadyInUserDictionary)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(providerResult);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(phrase);
        ArgumentNullException.ThrowIfNull(meanings);
        ArgumentNullException.ThrowIfNull(lookupHistory);

        return new LookupResponse
        {
            LearningItemId = learningItem.Id,
            WordId = null,
            PhraseId = phrase.Id,
            SentenceId = null,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = learningItem.ItemType.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = providerResult.ProviderName,
            IsAlreadyInUserDictionary = isAlreadyInUserDictionary,
            Meanings = ToLookupMeaningResponses(meanings),
            SentenceTranslations = Array.Empty<LookupSentenceTranslationResponse>()
        };
    }

    /// <summary>
    /// Provider'dan gelen sentence translation sonucunu API response DTO'suna dönüştürür.
    /// 
    /// Faz 19 kararı:
    /// - Sentence lookup translation use-case gibi çalışır.
    /// - Lookup anında LearningItem/Sentence/SentenceTranslation oluşturulmaz.
    /// - Bu yüzden LearningItemId, SentenceId ve SentenceTranslationId null dönebilir.
    /// - Kullanıcı sentence'i dictionary'ye kaydederse kalıcı kayıt save akışında oluşur.
    /// </summary>
    public static LookupResponse ToProviderSentenceLookupResponse(
        CreateLookupCommand request,
        string normalizedText,
        LanguageLookupData sourceLanguage,
        LanguageLookupData targetLanguage,
        DictionaryProviderResult providerResult,
        LookupHistory lookupHistory)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        ArgumentNullException.ThrowIfNull(providerResult);
        ArgumentNullException.ThrowIfNull(lookupHistory);

        return new LookupResponse
        {
            LearningItemId = null,
            WordId = null,
            PhraseId = null,
            SentenceId = null,
            LookupHistoryId = lookupHistory.Id,
            Text = request.Text,
            NormalizedText = normalizedText,
            ItemType = LearningItemType.Sentence.ToString(),
            SourceLanguageCode = sourceLanguage.Code,
            TargetLanguageCode = targetLanguage.Code,
            LookupSource = providerResult.ProviderName,
            IsAlreadyInUserDictionary = false,
            Meanings = Array.Empty<LookupMeaningResponse>(),
            SentenceTranslations = ToLookupSentenceTranslationResponses(providerResult.SentenceTranslations)
        };
    }

    /// <summary>
    /// Meaning entity listesini API response DTO listesine çevirir.
    /// 
    /// Neden mapper içinde?
    /// - Meaning domain entity'dir.
    /// - LookupMeaningResponse API response DTO'sudur.
    /// - Domain → Response DTO dönüşümü handler içinde dağılmamalıdır.
    /// </summary>
    public static IReadOnlyCollection<LookupMeaningResponse> ToLookupMeaningResponses(
        IReadOnlyCollection<Meaning> meanings)
    {
        ArgumentNullException.ThrowIfNull(meanings);

        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .Select(ToLookupMeaningResponse)
            .ToArray();
    }

    /// <summary>
    /// Tek bir Meaning entity'sini LookupMeaningResponse DTO'suna dönüştürür.
    /// </summary>
    public static LookupMeaningResponse ToLookupMeaningResponse(
        Meaning meaning)
    {
        ArgumentNullException.ThrowIfNull(meaning);

        return new LookupMeaningResponse
        {
            MeaningId = meaning.Id,
            Translation = meaning.MeaningText,
            Definition = meaning.ShortDefinition,
            ExampleSentence = null,
            PartOfSpeech = meaning.PartOfSpeech
        };
    }

    /// <summary>
    /// Provider sentence translation listesini API response DTO listesine çevirir.
    /// 
    /// Provider sonucu entity değildir.
    /// Bu yüzden SentenceTranslationId null atanır.
    /// </summary>
    public static IReadOnlyCollection<LookupSentenceTranslationResponse> ToLookupSentenceTranslationResponses(
        IReadOnlyCollection<DictionaryProviderSentenceTranslation> sentenceTranslations)
    {
        ArgumentNullException.ThrowIfNull(sentenceTranslations);

        return sentenceTranslations
            .Select(ToLookupSentenceTranslationResponse)
            .ToArray();
    }

    /// <summary>
    /// Tek bir provider sentence translation modelini response DTO'ya çevirir.
    /// </summary>
    public static LookupSentenceTranslationResponse ToLookupSentenceTranslationResponse(
        DictionaryProviderSentenceTranslation sentenceTranslation)
    {
        ArgumentNullException.ThrowIfNull(sentenceTranslation);

        return new LookupSentenceTranslationResponse
        {
            SentenceTranslationId = null,
            TranslatedText = sentenceTranslation.TranslatedText,
            SourceProvider = sentenceTranslation.SourceProvider,
            License = sentenceTranslation.License
        };
    }

    /// <summary>
    /// Application lookup input type değerini Domain InputType enumuna çevirir.
    /// 
    /// Neden mapper içinde?
    /// - LookupInputType application feature modelidir.
    /// - InputType domain enumudur.
    /// - İki katman modelinin dönüşüm kuralı handler içinde kalmamalıdır.
    /// </summary>
    public static InputType ToDomainInputType(
        LookupInputType inputType)
    {
        return inputType switch
        {
            LookupInputType.Word => InputType.Word,
            LookupInputType.Phrase => InputType.Phrase,
            LookupInputType.Sentence => InputType.Sentence,
            _ => InputType.Word
        };
    }
}