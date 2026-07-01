using MediatR;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Queries.GetMyDecks;

/// <summary>
/// Current user'ın kendi deck listesini getiren query modelidir.
/// </summary>
public sealed record GetMyDecksQuery : IRequest<GetMyDecksResponse>;