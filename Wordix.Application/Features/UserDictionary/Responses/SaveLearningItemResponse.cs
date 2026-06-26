namespace Wordix.Application.Features.UserDictionary.Responses;

/// <summary>
/// Kullanıcı bir LearningItem'ı dictionary'sine başarıyla kaydettiğinde dönecek response modelidir.
/// 
/// Bu response kullanıcının dictionary kaydı ve ilk progress kaydı hakkında
/// temel bilgileri frontend'e döndürür.
/// </summary>
public sealed class SaveLearningItemResponse
{
    /// <summary>
    /// Oluşturulan UserLearningItem kaydının id değeridir.
    /// 
    /// Bu id, kullanıcının kişisel dictionary item detaylarına gitmek için kullanılabilir.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Kaydedilen global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği Meaning id değeridir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SelectedMeaningId { get; init; }

    /// <summary>
    /// Oluşturulan UserLearningProgress kaydının id değeridir.
    /// 
    /// Kullanıcı dictionary'ye bir item eklediğinde öğrenme progress'i de başlatılır.
    /// </summary>
    public Guid UserLearningProgressId { get; init; }

    /// <summary>
    /// Kaydın hangi lookup history üzerinden geldiğini gösterir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }

    /// <summary>
    /// Kullanıcının dictionary'sine kayıt tarihi.
    /// </summary>
    public DateTimeOffset SavedAt { get; init; }

    /// <summary>
    /// İlk öğrenme durumudur.
    /// Örnek:
    /// New
    /// Learning
    /// NotStarted
    /// 
    /// Domain enum değerini string olarak döndürerek frontend tarafını daha okunabilir tutuyoruz.
    /// </summary>
    public string LearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// İlk confidence score değeridir.
    /// Genelde başlangıçta 0 olur.
    /// </summary>
    public int LearningConfidenceScore { get; init; }

    /// <summary>
    /// Kullanıcının dictionary kaydı aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
}