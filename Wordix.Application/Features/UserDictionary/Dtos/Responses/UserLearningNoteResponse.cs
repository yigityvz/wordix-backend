namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary item notunu API response olarak temsil eder.
/// 
/// Bu DTO hem not ekleme/güncelleme response'unda,
/// hem de ileride not listeleme endpointinde kullanılabilir.
/// </summary>
public sealed class UserLearningNoteResponse
{
    /// <summary>
    /// Oluşturulan veya dönen UserLearningNote id değeridir.
    /// </summary>
    public Guid UserLearningNoteId { get; init; }

    /// <summary>
    /// Notun bağlı olduğu kullanıcı dictionary item id değeridir.
    /// 
    /// Bu id global LearningItemId değildir.
    /// UserLearningItem.Id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının yazdığı kişisel not metnidir.
    /// </summary>
    public string NoteText { get; init; } = string.Empty;

    /// <summary>
    /// Notun oluşturulduğu zamandır.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Notun en son güncellendiği zamandır.
    /// Henüz güncellenmediyse null olabilir.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}