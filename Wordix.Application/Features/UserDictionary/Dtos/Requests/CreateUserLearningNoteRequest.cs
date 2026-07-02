namespace Wordix.Application.Features.UserDictionary.Dtos.Requests;

/// <summary>
/// Kullanıcının kendi dictionary item'ına not eklerken API'ye göndereceği request modelidir.
/// 
/// Endpoint:
/// POST /api/user-dictionary/{userLearningItemId}/notes
/// 
/// Örnek JSON:
/// {
///   "noteText": "Bunu hedefe ulaşmak gibi düşüneceğim."
/// }
/// </summary>
public sealed class CreateUserLearningNoteRequest
{
    /// <summary>
    /// Kullanıcının yazdığı kişisel not metnidir.
    /// 
    /// Bu not global LearningItem'a değil,
    /// route'tan gelen UserLearningItemId değerindeki kişisel dictionary kaydına eklenir.
    /// </summary>
    public string? NoteText { get; init; }
}