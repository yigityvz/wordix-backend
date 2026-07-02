namespace Wordix.Application.Features.UserDictionary.Dtos.Requests;

/// <summary>
/// Kullanıcının mevcut dictionary item notunu güncellerken API'ye göndereceği request modelidir.
/// 
/// Endpoint:
/// PUT /api/user-dictionary/notes/{noteId}
/// 
/// Örnek JSON:
/// {
///   "noteText": "Bu kelime hedefe ulaşmak anlamında kullanılıyor."
/// }
/// </summary>
public sealed class UpdateUserLearningNoteRequest
{
    /// <summary>
    /// Notun yeni metnidir.
    /// 
    /// Boş olamaz.
    /// Maksimum uzunluk validator tarafında kontrol edilir.
    /// </summary>
    public string? NoteText { get; init; }
}