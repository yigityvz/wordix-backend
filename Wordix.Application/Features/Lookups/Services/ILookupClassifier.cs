using Wordix.Application.Features.Lookups.Models;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Normalize edilmiş lookup text değerinin Word/Phrase/Sentence olarak sınıflandırılmasını sağlayan servis sözleşmesidir.
/// </summary>
public interface ILookupClassifier
{
    /// <summary>
    /// Normalize edilmiş text değerini sınıflandırır.
    /// 
    /// Örnek:
    /// "achieve" → Word
    /// "give up" → Phrase
    /// "i want to improve my english." → Sentence
    /// </summary>
    /// <param name="normalizedText">Normalize edilmiş lookup text.</param>
    /// <returns>Lookup input tipi.</returns>
    LookupInputType Classify(string normalizedText);
}