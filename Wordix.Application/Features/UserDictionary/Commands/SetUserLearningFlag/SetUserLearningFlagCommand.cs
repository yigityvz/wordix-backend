using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.SetUserLearningFlag;

/// <summary>
/// Kullanıcının kendi dictionary item'ına flag ekleme use-case command modelidir.
/// 
/// Bu command neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Body'den FlagType alır.
/// - Mapper bu iki bilgiyi command'e dönüştürür.
/// - Handler ownership ve duplicate/idempotent kontrolünü yapar.
/// </summary>
public sealed class SetUserLearningFlagCommand
    : IRequest<UserLearningFlagResponse>
{
    /// <summary>
    /// Flag eklenecek kullanıcı dictionary item id değeridir.
    /// 
    /// Global LearningItemId değildir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Eklenecek flag tipidir.
    /// 
    /// Örnek:
    /// Favorite
    /// Difficult
    /// </summary>
    public string FlagType { get; init; } = string.Empty;
}