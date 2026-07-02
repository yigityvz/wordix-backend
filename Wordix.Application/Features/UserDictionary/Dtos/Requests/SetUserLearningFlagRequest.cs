namespace Wordix.Application.Features.UserDictionary.Dtos.Requests;

/// <summary>
/// Kullanıcının kendi dictionary item'ına flag eklerken API'ye göndereceği request modelidir.
/// 
/// Endpoint:
/// POST /api/user-dictionary/{userLearningItemId}/flags
/// 
/// Örnek JSON:
/// {
///   "flagType": "Difficult"
/// }
/// 
/// Desteklenen değerler:
/// - Favorite
/// - Difficult
/// - WantMorePractice
/// - Ignored
/// </summary>
public sealed class SetUserLearningFlagRequest
{
    /// <summary>
    /// Kullanıcının item'a vereceği flag tipidir.
    /// 
    /// String almamızın nedeni:
    /// - API tarafında daha okunabilir request sağlamak.
    /// - "Difficult" gibi değerleri Swagger'da rahat test edebilmek.
    /// 
    /// Handler/validator tarafında UserLearningFlagType enum'una çevrilir.
    /// </summary>
    public string? FlagType { get; init; }
}