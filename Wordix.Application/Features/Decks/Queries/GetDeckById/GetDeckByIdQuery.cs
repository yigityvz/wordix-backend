using MediatR;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Queries.GetDeckById;

/// <summary>
/// Current user'ın tek bir deck detayını getiren query modelidir.
/// </summary>
public sealed record GetDeckByIdQuery(Guid DeckId) : IRequest<DeckDetailResponse>;