namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary item flag bilgisini API response olarak temsil eder.
/// 
/// Bu DTO:
/// - Flag set response'unda,
/// - Flag listeleme response'unda,
/// - Flag remove sonrası istenirse response olarak
/// kullanılabilir.
/// </summary>
public sealed class UserLearningFlagResponse
{
    /// <summary>
    /// UserLearningFlag id değeridir.
    /// </summary>
    public Guid UserLearningFlagId { get; init; }

    /// <summary>
    /// Flag'in bağlı olduğu kullanıcı dictionary item id değeridir.
    /// 
    /// Bu id global LearningItemId değildir.
    /// UserLearningItem.Id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Flag tipidir.
    /// 
    /// Örnek:
    /// Favorite
    /// Difficult
    /// WantMorePractice
    /// Ignored
    /// </summary>
    public string FlagType { get; init; } = string.Empty;

    /// <summary>
    /// Flag'in oluşturulduğu zamandır.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}