using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;

/// <summary>
/// Kullanıcının bir LearningItem'ı kendi dictionary'sine kaydetme isteğini temsil eden command modelidir.
/// 
/// Neden command?
/// - Bu işlem sistem durumunu değiştirir.
/// - UserLearningItem oluşturur.
/// - UserLearningProgress oluşturur.
/// - İleride UserLearningItemEvent oluşturur.
/// 
/// Bu yüzden CQRS açısından Query değil Command olarak modellenir.
/// 
/// Current user bilgisi:
/// - Client request içinde KeycloakUserId göndermez.
/// - Controller KeycloakUserId set etmez.
/// - CurrentUserBehavior pipeline içinde token'dan KeycloakUserId değerini okur
///   ve bu command üzerindeki KeycloakUserId propertysine yazar.
/// </summary>
public sealed record SaveLearningItemCommand
    : IRequest<SaveLearningItemResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// 
    /// Bu property client tarafından set edilmez.
    /// Handler bu değeri ownership ve kullanıcıya özel kayıt oluşturma işlemlerinde kullanır.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Kaydedilecek global LearningItem id değeridir.
    /// 
    /// Kullanıcı Word/Phrase/Sentence detay id'si değil, ortak LearningItem id'si gönderir.
    /// Böylece dictionary sistemi içerik tipinden bağımsız çalışır.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği anlam id değeridir.
    /// 
    /// Nullable çünkü bazı kayıt senaryolarında sistem primary meaning'i kullanabilir.
    /// Ancak handler içinde bu id verilmişse, ilgili meaning'in gerçekten bu LearningItem'a
    /// ait olup olmadığı kontrol edilecektir.
    /// </summary>
    public Guid? SelectedMeaningId { get; init; }

    /// <summary>
    /// Bu kaydın hangi lookup history sonucundan geldiğini gösterir.
    /// 
    /// Nullable çünkü kullanıcı ileride lookup dışındaki ekranlardan da dictionary'ye kayıt yapabilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }
}