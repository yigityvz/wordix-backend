using Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;
using Wordix.Application.Features.UserDictionary.Requests;

namespace Wordix.Application.Features.UserDictionary.Mappers;

/// <summary>
/// UserDictionary feature'ına ait DTO → Command dönüşümlerini merkezi olarak yapan mapper sınıfıdır.
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
}