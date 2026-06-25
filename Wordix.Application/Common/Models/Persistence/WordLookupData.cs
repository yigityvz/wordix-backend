using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Models.Persistence;

/// <summary>
/// Word lookup işlemi için gerekli verileri tek modelde taşır.
/// 
/// Neden böyle bir model var?
/// 
/// Lookup sırasında sadece Word kaydı yetmez.
/// Kullanıcıya sonuç dönebilmek için:
/// - LearningItem bilgisi,
/// - Word detay bilgisi,
/// - Meaning listesi
/// birlikte gerekir.
/// 
/// Bu model API response değildir.
/// Application içindeki use-case'lerin Persistence'tan aldığı okuma modelidir.
/// </summary>
/// <param name="LearningItem">
/// Word/Phrase/Sentence ortak çatısıdır.
/// İlk prototipte Word tipindeki LearningItem dönecektir.
/// </param>
/// <param name="Word">
/// Global kelime detay kaydıdır.
/// </param>
/// <param name="Meanings">
/// Kelimenin hedef dile ait anlam listesidir.
/// Örneğin İngilizce kelime için Türkçe anlamlar.
/// </param>
public sealed record WordLookupData(
    LearningItem LearningItem,
    Word Word,
    IReadOnlyList<Meaning> Meanings);