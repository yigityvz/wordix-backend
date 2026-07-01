using MediatR;
using Wordix.Application.Features.Decks.Dtos.Responses;

namespace Wordix.Application.Features.Decks.Commands.CreateDeck;

/// <summary>
/// Kullanıcının yeni deck oluşturma isteğini temsil eden command modelidir.
/// </summary>
public sealed record CreateDeckCommand : IRequest<CreateDeckResponse>
{
    /// <summary>
    /// Deck adıdır.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Deck açıklamasıdır.
    /// </summary>
    public string? Description { get; init; }
}