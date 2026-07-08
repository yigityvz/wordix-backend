namespace Wordix.Application.Common.Constants;

/// <summary>
/// Import/provider sistemi için ortak sabit değerleri tutar.
/// 
/// Neden sabit dosyası kullanıyoruz?
/// - Provider adlarını string olarak her yere dağınık yazmak hataya açıktır.
/// - "Tatoeba", "tatoeba", "TatoebaProvider" gibi farklı yazımlar oluşabilir.
/// - Merkezi sabitler sayesinde source/provider isimleri tutarlı kalır.
/// 
/// Bu dosya Application katmanında durur.
/// Çünkü import akışları Application use-case'leri tarafından yönetilecek,
/// gerçek provider implementasyonları ise Infrastructure katmanında olacaktır.
/// </summary>
public static class ImportConstants
{
    /// <summary>
    /// Büyük import işlemlerinde tek seferde kaç kayıt işleneceğini belirtir.
    /// 
    /// 9500 kelimeyi tek transaction ile işlemek yerine parça parça işlemek daha güvenlidir.
    /// </summary>
    public const int DefaultBatchSize = 100;

    /// <summary>
    /// Kullanıcının veya API'nin verebileceği maksimum batch size değeridir.
    /// 
    /// Çok büyük batch size verilirse memory ve transaction yönetimi zorlaşabilir.
    /// Bu yüzden üst sınır koyuyoruz.
    /// </summary>
    public const int MaxImportBatchSize = 1000;

    /// <summary>
    /// LearningItem.ExternalSourceKey alanının maksimum uzunluğuyla uyumlu tutulur.
    /// </summary>
    public const int MaxExternalSourceKeyLength = 200;


    /// <summary>
    /// Import süreçlerinde kullanılan standart dil kodlarıdır.
    /// 
    /// CEFR-J kelime listesi İngilizce kelime listesi olduğu için
    /// default source language English kabul edilir.
    /// </summary>
    public static class LanguageCodes
    {
        public const string English = "en";

        public const string Turkish = "tr";
    }

    /// <summary>
    /// Import endpoint'inde dışarıdan alınabilecek source değerleridir.
    /// 
    /// Kullanıcı/API şu değerleri form field olarak gönderebilir:
    /// - cefrj
    /// - octanove
    /// </summary>
    public static class ImportSources
    {
        public const string CefrJ = "cefrj";

        public const string Octanove = "octanove";
    }

    /// <summary>
    /// Import kaynaklarının default version değerleridir.
    /// </summary>
    public static class SourceVersions
    {
        public const string CefrJ15 = "1.5";

        public const string Octanove10 = "1.0";
    }

    /// <summary>
    /// Provider/import kaynak adları.
    /// 
    /// Bu değerler Meaning.SourceProvider, Sentence.SourceProvider
    /// veya log kayıtlarında okunabilir provider adı olarak kullanılabilir.
    /// </summary>
    public static class ProviderNames
    {
        public const string PrototypeSeed = "PrototypeSeed";

        public const string CefrJ = "CEFR-J";

        public const string WiktionaryKaikki = "WiktionaryKaikki";

        public const string Tatoeba = "Tatoeba";

        public const string LibreTranslate = "LibreTranslate";

        public const string Fallback = "Fallback";

        public const string SystemGenerated = "SystemGenerated";

        public const string UserInput = "UserInput";

        public const string Octanove = "Octanove";

        public const string AzureTranslator = "AzureTranslator";

        public const string FreeDict = "FreeDict";
    }
}