using Wordix.Application.Features.Decks.Commands.AddItemToDeck;
using Wordix.Application.Features.Decks.Commands.CreateDeck;
using Wordix.Application.Features.Decks.Commands.RemoveItemFromDeck;
using Wordix.Application.Features.Decks.Dtos.Requests;
using Wordix.Application.Features.Decks.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Decks.Mappers;

/// <summary>
/// Decks feature'ına ait explicit mapping işlemlerini yapan mapper sınıfıdır.
/// 
/// Neden var?
/// - Controller içinde Request DTO -> Command dönüşümü yapmak istemiyoruz.
/// - Handler içinde Response DTO propertylerini tek tek dizmek istemiyoruz.
/// - AutoMapper/Mapster kullanmıyoruz.
/// - Feature-based explicit mapper standardını koruyoruz.
/// </summary>
public static class DeckMapper
{
    /// <summary>
    /// API request DTO'sunu CreateDeckCommand modeline dönüştürür.
    /// 
    /// Controller null body kontrolü yapmaz.
    /// Request null gelirse boş command üretir.
    /// Validator bu hatayı yakalar.
    /// </summary>
    public static CreateDeckCommand ToCreateDeckCommand(
        CreateDeckRequest? request)
    {
        return new CreateDeckCommand
        {
            Name = request?.Name ?? string.Empty,
            Description = request?.Description
        };
    }

    /// <summary>
    /// Route deckId ve request body değerlerinden AddItemToDeckCommand üretir.
    /// </summary>
    public static AddItemToDeckCommand ToAddItemToDeckCommand(
        Guid deckId,
        AddItemToDeckRequest? request)
    {
        return new AddItemToDeckCommand
        {
            DeckId = deckId,
            UserLearningItemId = request?.UserLearningItemId ?? Guid.Empty
        };
    }

    /// <summary>
    /// Route değerlerinden RemoveItemFromDeckCommand üretir.
    /// </summary>
    public static RemoveItemFromDeckCommand ToRemoveItemFromDeckCommand(
        Guid deckId,
        Guid userLearningItemId)
    {
        return new RemoveItemFromDeckCommand
        {
            DeckId = deckId,
            UserLearningItemId = userLearningItemId
        };
    }

    /// <summary>
    /// Deck entity'sinden CreateDeckResponse üretir.
    /// </summary>
    public static CreateDeckResponse ToCreateDeckResponse(
        Deck deck)
    {
        ArgumentNullException.ThrowIfNull(deck);

        return new CreateDeckResponse
        {
            DeckId = deck.Id,
            Name = deck.Name,
            NormalizedName = deck.NormalizedName,
            Description = deck.Description,
            CreatedAt = deck.CreatedAt,
            IsActive = deck.IsActive
        };
    }

    /// <summary>
    /// DeckItem entity'sinden AddItemToDeckResponse üretir.
    /// </summary>
    public static AddItemToDeckResponse ToAddItemToDeckResponse(
        DeckItem deckItem)
    {
        ArgumentNullException.ThrowIfNull(deckItem);

        return new AddItemToDeckResponse
        {
            DeckItemId = deckItem.Id,
            DeckId = deckItem.DeckId,
            UserLearningItemId = deckItem.UserLearningItemId,
            AddedAt = deckItem.AddedAt
        };
    }

    /// <summary>
    /// Remove işleminden sonra response üretir.
    /// </summary>
    public static RemoveItemFromDeckResponse ToRemoveItemFromDeckResponse(
        DeckItem removedDeckItem)
    {
        ArgumentNullException.ThrowIfNull(removedDeckItem);

        return new RemoveItemFromDeckResponse
        {
            DeckId = removedDeckItem.DeckId,
            UserLearningItemId = removedDeckItem.UserLearningItemId,
            RemovedDeckItemId = removedDeckItem.Id,
            Removed = true
        };
    }

    /// <summary>
    /// Deck listesini GetMyDecksResponse içine sarar.
    /// </summary>
    public static GetMyDecksResponse ToGetMyDecksResponse(
        IReadOnlyCollection<DeckSummaryResponse> decks)
    {
        ArgumentNullException.ThrowIfNull(decks);

        return new GetMyDecksResponse
        {
            TotalCount = decks.Count,
            Decks = decks
        };
    }

    /// <summary>
    /// Boş deck listesi response'u üretir.
    /// </summary>
    public static GetMyDecksResponse ToEmptyGetMyDecksResponse()
    {
        return new GetMyDecksResponse
        {
            TotalCount = 0,
            Decks = Array.Empty<DeckSummaryResponse>()
        };
    }

    /// <summary>
    /// Deck entity'sinden liste ekranı için summary response üretir.
    /// </summary>
    public static DeckSummaryResponse ToDeckSummaryResponse(
        Deck deck,
        int itemCount)
    {
        ArgumentNullException.ThrowIfNull(deck);

        return new DeckSummaryResponse
        {
            DeckId = deck.Id,
            Name = deck.Name,
            NormalizedName = deck.NormalizedName,
            Description = deck.Description,
            ItemCount = itemCount,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
            IsActive = deck.IsActive
        };
    }

    /// <summary>
    /// Deck entity'sinden detay response üretir.
    /// </summary>
    public static DeckDetailResponse ToDeckDetailResponse(
        Deck deck,
        IReadOnlyCollection<DeckItemResponse> items)
    {
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(items);

        return new DeckDetailResponse
        {
            DeckId = deck.Id,
            Name = deck.Name,
            NormalizedName = deck.NormalizedName,
            Description = deck.Description,
            ItemCount = items.Count,
            Items = items,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
            IsActive = deck.IsActive
        };
    }

    /// <summary>
    /// Deck item detay response üretir.
    /// 
    /// Bu method UserDictionaryMapper'ın response tiplerini kullanır.
    /// Çünkü Word/Phrase için meaning, Sentence için sentence translation response formatı
    /// dictionary ile aynı mantıktadır.
    /// </summary>
    public static DeckItemResponse ToDeckItemResponse(
        DeckItem deckItem,
        UserLearningItem userLearningItem,
        LearningItem learningItem,
        Word? word,
        Phrase? phrase,
        Sentence? sentence,
        Language? sourceLanguage,
        IReadOnlyCollection<Meaning> meanings,
        SentenceTranslation? sentenceTranslation,
        Language? targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(deckItem);
        ArgumentNullException.ThrowIfNull(userLearningItem);
        ArgumentNullException.ThrowIfNull(learningItem);
        ArgumentNullException.ThrowIfNull(meanings);

        var selectedMeaning = ResolveSelectedMeaning(
            selectedMeaningId: userLearningItem.SelectedMeaningId,
            meanings: meanings);

        return new DeckItemResponse
        {
            DeckItemId = deckItem.Id,
            UserLearningItemId = userLearningItem.Id,
            LearningItemId = learningItem.Id,
            WordId = word?.Id,
            PhraseId = phrase?.Id,
            SentenceId = sentence?.Id,
            ItemType = learningItem.ItemType.ToString(),
            DisplayText = ResolveDisplayText(learningItem, word, phrase, sentence),
            NormalizedText = ResolveNormalizedText(learningItem, word, phrase, sentence),
            SourceLanguageCode = sourceLanguage?.Code ?? string.Empty,
            SelectedMeaning = selectedMeaning is null
                ? null
                : UserDictionaryMapper.ToUserDictionaryMeaningResponse(selectedMeaning),
            SentenceTranslation = sentenceTranslation is null
                ? null
                : UserDictionaryMapper.ToUserDictionarySentenceTranslationResponse(
                    sentenceTranslation,
                    targetLanguage),
            AddedAt = deckItem.AddedAt
        };
    }

    /// <summary>
    /// Kullanıcının seçili/ana meaning bilgisini çözer.
    /// 
    /// Bu method UserDictionaryMapper içindeki private helper'a benzer.
    /// Private olduğu için burada tekrar yazıyoruz.
    /// Mapping feature bazlı kalmaya devam eder.
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

        var primaryMeaning = meanings.FirstOrDefault(
            meaning => meaning.IsPrimary);

        if (primaryMeaning is not null)
        {
            return primaryMeaning;
        }

        return meanings
            .OrderBy(meaning => meaning.DisplayOrder)
            .FirstOrDefault();
    }

    /// <summary>
    /// LearningItem tipine göre gösterilecek ana metni çözer.
    /// </summary>
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
    /// LearningItem tipine göre normalize edilmiş ana metni çözer.
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