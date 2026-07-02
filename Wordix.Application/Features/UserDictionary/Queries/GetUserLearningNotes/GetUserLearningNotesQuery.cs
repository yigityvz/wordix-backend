using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningNotes;

/// <summary>
/// Kullanıcının kendi dictionary item'ına ait notları listeleme query modelidir.
/// 
/// Bu query neden var?
/// - Controller route'tan UserLearningItemId alır.
/// - Application katmanı bu id üzerinden ownership kontrolü yapar.
/// - Sonra sadece current user'a ait item'ın notları listelenir.
/// 
/// Buradaki id global LearningItemId değildir.
/// UserLearningItem.Id değeridir.
/// </summary>
public sealed record GetUserLearningNotesQuery(
    Guid UserLearningItemId)
    : IRequest<GetUserLearningNotesResponse>;