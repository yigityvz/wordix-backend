namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının bir dictionary item'ına ait flag listesini temsil eden response DTO'sudur.
/// 
/// Endpoint:
/// GET /api/user-dictionary/{userLearningItemId}/flags
/// 
/// Neden wrapper response kullanıyoruz?
/// - Listeyle birlikte TotalCount dönebilmek için.
/// - İleride pagination veya summary alanları eklenirse DTO yapısını bozmadan genişletebilmek için.
/// </summary>
public sealed class GetUserLearningFlagsResponse
{
    /// <summary>
    /// Dönen flag sayısıdır.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Kullanıcının ilgili dictionary item için verdiği flaglerdir.
    /// 
    /// Örnek:
    /// Favorite
    /// Difficult
    /// WantMorePractice
    /// Ignored
    /// </summary>
    public IReadOnlyCollection<UserLearningFlagResponse> Items { get; init; }
        = Array.Empty<UserLearningFlagResponse>();
}