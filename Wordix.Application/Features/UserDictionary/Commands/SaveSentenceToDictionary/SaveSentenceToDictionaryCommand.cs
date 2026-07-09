using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
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
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini çözer.
/// - Handler bu değeri request.KeycloakUserId üzerinden kullanır.
/// </summary>
public sealed record SaveSentenceToDictionaryCommand
    : IRequest<SaveSentenceToDictionaryResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// 
    /// Bu değer client tarafından gönderilmez.
    /// Sentence dictionary kaydı ve ownership kontrolleri için kullanılır.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

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