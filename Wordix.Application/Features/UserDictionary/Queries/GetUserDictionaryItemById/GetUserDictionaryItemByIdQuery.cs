using MediatR;
using Wordix.Application.Features.UserDictionary.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserDictionaryItemById;

/// <summary>
/// Current user'ın dictionary'sindeki tek bir item detayını getiren query modelidir.
/// 
/// Bu query neden var?
/// - GET /api/user-dictionary/{id} endpointi için kullanılır.
/// - Kullanıcının kendi dictionary item detayını döner.
/// - Başka kullanıcının item'ına erişim ownership kontrolüyle engellenir.
/// 
/// Buradaki id global LearningItemId değildir.
/// Buradaki id UserLearningItemId değeridir.
/// </summary>
public sealed record GetUserDictionaryItemByIdQuery : IRequest<UserDictionaryItemResponse>
{
    /// <summary>
    /// Kullanıcının kişisel dictionary item id değeridir.
    /// 
    /// Bu id UserLearningItems tablosundaki kaydın id değeridir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Query constructor.
    /// 
    /// Controller route parametresinden gelen id değerini bu query'ye aktaracak.
    /// </summary>
    public GetUserDictionaryItemByIdQuery(Guid userLearningItemId)
    {
        UserLearningItemId = userLearningItemId;
    }
}