using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.RemoveUserLearningFlag;

/// <summary>
/// Kullanıcının kendi dictionary item'ından belirli bir flag'i kaldırma use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Controller route'tan FlagType alır.
/// - Mapper bu iki route bilgisini command'e dönüştürür.
/// - Handler ownership ve silme işlemini yürütür.
/// 
/// Örnek:
/// DELETE /api/user-dictionary/{userLearningItemId}/flags/Difficult
/// </summary>
public sealed record RemoveUserLearningFlagCommand(
    Guid UserLearningItemId,
    string FlagType)
    : IRequest<UserLearningFlagResponse>;