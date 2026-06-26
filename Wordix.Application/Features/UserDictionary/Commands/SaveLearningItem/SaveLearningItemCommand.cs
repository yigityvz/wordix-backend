using MediatR;
using Wordix.Application.Features.UserDictionary.Responses;

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
/// </summary>
public sealed record SaveLearningItemCommand : IRequest<SaveLearningItemResponse>
{
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