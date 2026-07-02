namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının bir dictionary item'ına ait not listesini temsil eden response DTO'sudur.
/// 
/// Endpoint:
/// GET /api/user-dictionary/{userLearningItemId}/notes
/// 
/// Neden wrapper response kullanıyoruz?
/// - Listeyle birlikte TotalCount dönebilmek için.
/// - İleride pagination eklenirse DTO yapısı bozulmadan genişletilebilir.
/// </summary>
public sealed class GetUserLearningNotesResponse
{
    /// <summary>
    /// Dönen not sayısıdır.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Kullanıcının ilgili dictionary item için yazdığı notlardır.
    /// </summary>
    public IReadOnlyCollection<UserLearningNoteResponse> Items { get; init; }
        = Array.Empty<UserLearningNoteResponse>();
}