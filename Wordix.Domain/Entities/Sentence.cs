using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sistemdeki sentence / cümle kaydını temsil eder.
/// 
/// Sentence, Word ve Phrase'ten bilinçli olarak farklı ele alınır.
/// Çünkü cümleler sonsuz sayıda üretilebilir.
/// Bu yüzden her sentence lookup sonucunu otomatik olarak database'e kaydetmiyoruz.
/// 
/// Faz 19 kararı:
/// - Kullanıcı sadece cümle çevirisi yaparsa kalıcı Sentence oluşturulmaz.
/// - Kullanıcı bu cümleyi öğrenmek için dictionary'ye kaydederse Sentence oluşturulur.
/// - İleride import/provider sisteminden gelen örnek cümleler de LearningItem olmadan saklanabilir.
/// 
/// Bu yüzden LearningItemId nullable tasarlanmıştır.
/// </summary>
public class Sentence : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// Domain tarafında dışarıdan boş Sentence oluşturulmasını istemiyoruz.
    /// EF Core ise entity materialization sırasında bu constructor'ı kullanabilir.
    /// </summary>
    protected Sentence()
    {
    }

    /// <summary>
    /// Yeni sentence kaydı oluşturur.
    /// 
    /// learningItemId neden nullable?
    /// - Eğer sentence kullanıcı tarafından öğrenilecek bir içerik olarak kaydediliyorsa dolu olur.
    /// - Eğer sentence ileride sadece örnek cümle/import verisi olarak tutulacaksa null olabilir.
    /// 
    /// languageId neden zorunlu?
    /// - Aynı sentence metni farklı dillerde farklı anlamlara gelebilir.
    /// - Arama, duplicate kontrol ve provider/import süreçleri için sentence'in kaynak dili bilinmelidir.
    /// </summary>
    public Sentence(
        Guid? learningItemId,
        Guid languageId,
        string text,
        string normalizedText,
        string? externalSentenceId = null,
        string? sourceProvider = null,
        string? license = null)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz. Null veya geçerli Guid olmalıdır.", nameof(learningItemId));
        }

        if (languageId == Guid.Empty)
        {
            throw new ArgumentException("LanguageId boş Guid olamaz.", nameof(languageId));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Sentence metni boş olamaz.", nameof(text));
        }

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            throw new ArgumentException("Normalize edilmiş sentence metni boş olamaz.", nameof(normalizedText));
        }

        LearningItemId = learningItemId;
        LanguageId = languageId;

        // Kullanıcıya gösterilecek orijinal cümle metni.
        Text = text.Trim();

        // Duplicate kontrol ve arama için normalize edilmiş cümle metni.
        NormalizedText = normalizedText.Trim().ToLowerInvariant();

        ExternalSentenceId = string.IsNullOrWhiteSpace(externalSentenceId)
            ? null
            : externalSentenceId.Trim();

        SourceProvider = string.IsNullOrWhiteSpace(sourceProvider)
            ? null
            : sourceProvider.Trim();

        License = string.IsNullOrWhiteSpace(license)
            ? null
            : license.Trim();
    }

    /// <summary>
    /// Bu sentence öğrenilebilir bir içerik olarak kaydedildiyse bağlı LearningItem Id değeridir.
    /// 
    /// Null olabilir.
    /// Çünkü her sentence LearningItem olmak zorunda değildir.
    /// Örneğin ileride sadece Word/Phrase için örnek cümle olarak saklanan sentence'ler
    /// LearningItem'a bağlı olmayabilir.
    /// </summary>
    public Guid? LearningItemId { get; private set; }

    /// <summary>
    /// Sentence'in kaynak dil Id değeridir.
    /// 
    /// Örnek:
    /// İngilizce bir cümle için English language Id tutulur.
    /// </summary>
    public Guid LanguageId { get; private set; }

    /// <summary>
    /// Sentence'in kullanıcıya gösterilecek orijinal metnidir.
    /// 
    /// Örnek:
    /// I have been working on this project for two weeks.
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>
    /// Arama ve duplicate kontrol için normalize edilmiş sentence değeridir.
    /// 
    /// Örnek:
    /// " I Have Been Working " → "i have been working"
    /// </summary>
    public string NormalizedText { get; private set; } = string.Empty;

    /// <summary>
    /// Dış provider/import kaynağındaki sentence id değeridir.
    /// 
    /// Örnek:
    /// Tatoeba sentence id.
    /// 
    /// Prototype aşamasında çoğunlukla null kalabilir.
    /// </summary>
    public string? ExternalSentenceId { get; private set; }

    /// <summary>
    /// Sentence'in hangi provider/import kaynağından geldiğini belirtir.
    /// 
    /// Örnek:
    /// PrototypeProvider, Tatoeba, LibreTranslate
    /// </summary>
    public string? SourceProvider { get; private set; }

    /// <summary>
    /// Sentence verisinin lisans bilgisidir.
    /// 
    /// Faz 24 import/provider sisteminde önemli hale gelecektir.
    /// Prototype aşamasında null kalabilir.
    /// </summary>
    public string? License { get; private set; }

    /// <summary>
    /// LearningItem bağlantısı olmayan bir sentence'i öğrenilebilir içerik haline getirir.
    /// 
    /// Ne zaman kullanılır?
    /// - İleride import edilmiş ama LearningItem olmayan bir sentence'i
    ///   kullanıcı dictionary'ye kaydetmek isterse.
    /// 
    /// Faz 19 save akışında çoğunlukla Sentence zaten LearningItemId ile oluşturulacak.
    /// Bu method future-ready kalması için eklendi.
    /// </summary>
    public void AttachToLearningItem(Guid learningItemId)
    {
        if (learningItemId == Guid.Empty)
        {
            throw new ArgumentException("LearningItemId boş Guid olamaz.", nameof(learningItemId));
        }

        if (LearningItemId == learningItemId)
        {
            return;
        }

        LearningItemId = learningItemId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Provider/import metadata bilgisini günceller.
    /// 
    /// Sentence'in ana metnini değiştirmiyoruz.
    /// Çünkü sentence text değişikliği duplicate ve analytics açısından hassas bir işlemdir.
    /// </summary>
    public void UpdateSourceMetadata(
        string? externalSentenceId,
        string? sourceProvider,
        string? license)
    {
        ExternalSentenceId = string.IsNullOrWhiteSpace(externalSentenceId)
            ? null
            : externalSentenceId.Trim();

        SourceProvider = string.IsNullOrWhiteSpace(sourceProvider)
            ? null
            : sourceProvider.Trim();

        License = string.IsNullOrWhiteSpace(license)
            ? null
            : license.Trim();

        MarkAsUpdated();
    }
}