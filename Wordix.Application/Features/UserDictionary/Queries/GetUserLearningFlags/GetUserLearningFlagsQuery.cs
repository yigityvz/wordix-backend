using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningFlags;

/// <summary>
/// Kullanıcının kendi dictionary item'ına ait flagleri listeleme query modelidir.
/// 
/// Bu query neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Application katmanı bu id üzerinden ownership kontrolü yapar.
/// - Sonra sadece current user'a ait item'ın flagleri listelenir.
/// 
/// Buradaki id global LearningItemId değildir.
/// UserLearningItem.Id değeridir.
/// </summary>
public sealed record GetUserLearningFlagsQuery(
    Guid UserLearningItemId)
    : IRequest<GetUserLearningFlagsResponse>;