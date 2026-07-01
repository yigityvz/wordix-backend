using MediatR;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveSentenceToDictionary;

/// <summary>
/// Kullanıcının sentence lookup sonucunu dictionary'sine kaydetme isteğini temsil eden command modelidir.
/// 
/// Neden ayrı command?
/// - SaveLearningItemCommand var olan bir LearningItem'ı kaydeder.
/// - Bu command ise henüz kalıcı olmayan bir sentence lookup sonucunu
///   LearningItem + Sentence + SentenceTranslation haline getirip dictionary'ye kaydeder.
/// 
/// Bu yüzden iki farklı use-case'i tek command içine sıkıştırmıyoruz.
/// </summary>
public sealed record SaveSentenceToDictionaryCommand : IRequest<SaveSentenceToDictionaryResponse>
{
    /// <summary>
    /// Kaynak cümle metni.
    /// </summary>
    public string SourceText { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dildeki çeviri metni.
    /// </summary>
    public string TranslatedText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodu.
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Hedef dil kodu.
    /// </summary>
    public string TargetLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Bu kaydın hangi lookup history sonucundan geldiğini gösterir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }
}