using Wordix.Domain.Entities;

namespace Wordix.Application.Common.Models.Persistence;

/// <summary>
/// Phrase lookup işlemi için gerekli verileri tek modelde taşır.
/// 
/// Neden böyle bir model var?
/// 
/// Lookup sırasında sadece Phrase kaydı yetmez.
/// Kullanıcıya sonuç dönebilmek için:
/// - LearningItem bilgisi,
/// - Phrase detay bilgisi,
/// - Meaning listesi
/// birlikte gerekir.
/// 
/// Bu model API response değildir.
/// Application içindeki use-case'lerin Persistence'tan aldığı okuma modelidir.
/// </summary>
/// <param name="LearningItem">
/// Word/Phrase/Sentence ortak çatısıdır.
/// Bu modelde Phrase tipindeki LearningItem dönecektir.
/// </param>
/// <param name="Phrase">
/// Global phrase / kalıp ifade detay kaydıdır.
/// </param>
/// <param name="Meanings">
/// Phrase'in hedef dile ait anlam listesidir.
/// Örneğin İngilizce phrase için Türkçe anlamlar.
/// </param>
public sealed record PhraseLookupData(
    LearningItem LearningItem,
    Phrase Phrase,
    IReadOnlyList<Meaning> Meanings);