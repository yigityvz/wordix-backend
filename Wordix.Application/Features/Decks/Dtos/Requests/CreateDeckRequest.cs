namespace Wordix.Application.Features.Decks.Dtos.Requests;

/// <summary>
/// Kullanıcının yeni bir deck oluşturmak için API'ye göndereceği request modelidir.
/// 
/// Deck nedir?
/// Kullanıcının kendi dictionary itemlarını grupladığı kişisel çalışma koleksiyonudur.
/// </summary>
public sealed class CreateDeckRequest
{
    /// <summary>
    /// Deck adıdır.
    /// 
    /// Örnek:
    /// Software English
    /// Daily Phrases
    /// Internship Words
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Deck açıklamasıdır.
    /// Opsiyoneldir.
    /// </summary>
    public string? Description { get; init; }
}