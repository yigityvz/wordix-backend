using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Sentence entity'sinin hedef dildeki çeviri kaydını temsil eder.
/// 
/// Neden Meaning kullanmıyoruz?
/// - Meaning, Word/Phrase gibi kısa öğrenme içeriklerinin anlamlarını tutar.
/// - SentenceTranslation ise tam cümle çevirisidir.
/// - Cümle çevirisi, kelime anlamı ile aynı domain kavramı değildir.
/// 
/// Bu yüzden sentence çevirileri ayrı entity olarak modellenmiştir.
/// </summary>
public class SentenceTranslation : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected SentenceTranslation()
    {
    }

    /// <summary>
    /// Yeni sentence translation kaydı oluşturur.
    /// </summary>
    public SentenceTranslation(
        Guid sourceSentenceId,
        Guid targetLanguageId,
        string translatedText,
        string normalizedTranslatedText,
        string? sourceProvider = null,
        string? license = null,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        if (sourceSentenceId == Guid.Empty)
        {
            throw new ArgumentException("SourceSentenceId boş Guid olamaz.", nameof(sourceSentenceId));
        }

        if (targetLanguageId == Guid.Empty)
        {
            throw new ArgumentException("TargetLanguageId boş Guid olamaz.", nameof(targetLanguageId));
        }

        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new ArgumentException("Çeviri metni boş olamaz.", nameof(translatedText));
        }

        if (string.IsNullOrWhiteSpace(normalizedTranslatedText))
        {
            throw new ArgumentException("Normalize edilmiş çeviri metni boş olamaz.", nameof(normalizedTranslatedText));
        }

        SourceSentenceId = sourceSentenceId;
        TargetLanguageId = targetLanguageId;

        // Kullanıcıya gösterilecek çeviri metni.
        TranslatedText = translatedText.Trim();

        // Duplicate kontrol ve karşılaştırma için normalize edilmiş çeviri.
        NormalizedTranslatedText = normalizedTranslatedText.Trim().ToLowerInvariant();

        SourceProvider = string.IsNullOrWhiteSpace(sourceProvider)
            ? null
            : sourceProvider.Trim();

        License = string.IsNullOrWhiteSpace(license)
            ? null
            : license.Trim();

        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Çevirinin ait olduğu kaynak sentence id değeridir.
    /// </summary>
    public Guid SourceSentenceId { get; private set; }

    /// <summary>
    /// Çevirinin hangi hedef dile ait olduğunu gösterir.
    /// 
    /// Örnek:
    /// İngilizce kaynak sentence için Türkçe çeviri varsa TargetLanguageId = Turkish.
    /// </summary>
    public Guid TargetLanguageId { get; private set; }

    /// <summary>
    /// Kullanıcıya gösterilecek çeviri metnidir.
    /// 
    /// Örnek:
    /// Bu proje üzerinde iki haftadır çalışıyorum.
    /// </summary>
    public string TranslatedText { get; private set; } = string.Empty;

    /// <summary>
    /// Karşılaştırma ve duplicate kontrol için normalize edilmiş çeviri metnidir.
    /// </summary>
    public string NormalizedTranslatedText { get; private set; } = string.Empty;

    /// <summary>
    /// Çevirinin hangi provider/import kaynağından geldiğini belirtir.
    /// 
    /// Prototype aşamasında PrototypeProvider olabilir.
    /// Faz 24'te gerçek provider/import sistemiyle daha önemli hale gelecektir.
    /// </summary>
    public string? SourceProvider { get; private set; }

    /// <summary>
    /// Çeviri verisinin lisans bilgisidir.
    /// </summary>
    public string? License { get; private set; }

    /// <summary>
    /// Aynı sentence için birden fazla çeviri varsa ana/öncelikli çeviri mi?
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Response ve listeleme sırasında gösterim sırasıdır.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Çeviri metnini günceller.
    /// 
    /// Provider/import doğrulama sürecinde daha iyi bir çeviriyle güncelleme yapılabilir.
    /// </summary>
    public void UpdateTranslation(
        string translatedText,
        string normalizedTranslatedText)
    {
        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new ArgumentException("Çeviri metni boş olamaz.", nameof(translatedText));
        }

        if (string.IsNullOrWhiteSpace(normalizedTranslatedText))
        {
            throw new ArgumentException("Normalize edilmiş çeviri metni boş olamaz.", nameof(normalizedTranslatedText));
        }

        TranslatedText = translatedText.Trim();
        NormalizedTranslatedText = normalizedTranslatedText.Trim().ToLowerInvariant();

        MarkAsUpdated();
    }

    /// <summary>
    /// Bu çeviriyi primary çeviri olarak işaretler.
    /// </summary>
    public void MarkAsPrimary()
    {
        if (IsPrimary)
        {
            return;
        }

        IsPrimary = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Bu çevirinin primary işaretini kaldırır.
    /// </summary>
    public void UnmarkAsPrimary()
    {
        if (!IsPrimary)
        {
            return;
        }

        IsPrimary = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Gösterim sırasını günceller.
    /// </summary>
    public void ChangeDisplayOrder(int displayOrder)
    {
        if (DisplayOrder == displayOrder)
        {
            return;
        }

        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    /// <summary>
    /// Provider/import metadata bilgisini günceller.
    /// </summary>
    public void UpdateSourceMetadata(
        string? sourceProvider,
        string? license)
    {
        SourceProvider = string.IsNullOrWhiteSpace(sourceProvider)
            ? null
            : sourceProvider.Trim();

        License = string.IsNullOrWhiteSpace(license)
            ? null
            : license.Trim();

        MarkAsUpdated();
    }
}
