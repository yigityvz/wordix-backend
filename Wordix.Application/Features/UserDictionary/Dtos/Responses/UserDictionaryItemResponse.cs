namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının dictionary listesindeki tek bir item'ı temsil eder.
/// 
/// Bu DTO, GET /api/user-dictionary endpointinde dönecek liste elemanıdır.
/// 
/// Dikkat:
/// Bu response UserLearningItem merkezlidir.
/// Çünkü dictionary kaydı global LearningItem değil, kullanıcının kişisel UserLearningItem kaydıdır.
/// </summary>
public sealed class UserDictionaryItemResponse
{
    /// <summary>
    /// Kullanıcının kişisel dictionary item id değeridir.
    /// 
    /// GET /api/user-dictionary/{id} endpointindeki id olarak bu değer kullanılacak.
    /// </summary>
    public Guid UserLearningItemId { get; init; }

    /// <summary>
    /// Global LearningItem id değeridir.
    /// </summary>
    public Guid LearningItemId { get; init; }

    /// <summary>
    /// Eğer item Word ise Word entity id değeridir.
    /// Phrase/Sentence geldiğinde null olabilir.
    /// </summary>
    public Guid? WordId { get; init; }


    /// <summary>
    /// Eğer item Phrase ise Phrase entity id değeridir.
    /// 
    /// Word itemlarında null olur.
    /// Phrase itemlarında dolu olur.
    /// Ana dictionary/progress/quiz akışı yine LearningItemId üzerinden ilerler.
    /// </summary>
    public Guid? PhraseId { get; init; }


    /// <summary>
    /// İçerik tipi.
    /// 
    /// Örnek:
    /// Word
    /// Phrase
    /// Sentence
    /// </summary>
    /// 
    public string ItemType { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcıya gösterilecek ana metindir.
    /// 
    /// Word için:
    /// achieve
    /// 
    /// Phrase için:
    /// give up
    /// 
    /// Sentence için:
    /// I want to improve my English.
    /// </summary>
    public string DisplayText { get; init; } = string.Empty;

    /// <summary>
    /// Normalize edilmiş metindir.
    /// 
    /// Örnek:
    /// " Achieve " → "achieve"
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// Kaynak dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// </summary>
    public string SourceLanguageCode { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının seçtiği anlam id değeridir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SelectedMeaningId { get; init; }

    /// <summary>
    /// Kullanıcının seçtiği veya sistemin primary olarak belirlediği anlam bilgisidir.
    /// </summary>
    public UserDictionaryMeaningResponse? SelectedMeaning { get; init; }

    /// <summary>
    /// Kullanıcı bu item'ı ne zaman dictionary'sine ekledi?
    /// </summary>
    public DateTimeOffset SavedAt { get; init; }

    /// <summary>
    /// Bu kayıt hangi lookup history üzerinden geldiyse onun id değeridir.
    /// Nullable olabilir.
    /// </summary>
    public Guid? SourceLookupHistoryId { get; init; }

    /// <summary>
    /// Öğrenme durumu.
    /// 
    /// Örnek:
    /// New
    /// Learning
    /// Mastered
    /// </summary>
    public string LearningStatus { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının bu item için öğrenme güven skoru.
    /// </summary>
    public int LearningConfidenceScore { get; init; }

    /// <summary>
    /// Bu item kullanıcı dictionary'sinde aktif mi?
    /// Silme/arsivleme ileride soft delete gibi davranabilir.
    /// </summary>
    public bool IsActive { get; init; }
}
