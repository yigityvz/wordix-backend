using Wordix.Domain.Enums;

namespace Wordix.Application.Common.Models.Import;

/// <summary>
/// CEFR kelime listesi provider'ından okunan tek bir import satırını temsil eder.
/// 
/// Bu model database entity değildir.
/// Yani LearningItem veya Word değildir.
/// 
/// Bu modelin görevi:
/// - Dış kaynaktan gelen kelime verisini Application katmanına standart şekilde taşımak.
/// - Import handler'ın CSV/provider detaylarını bilmesini engellemek.
/// 
/// Örnek:
/// CEFR-J dosyasında bir satırdan şuna benzer veri gelebilir:
/// Text = "abandon"
/// NormalizedText = "abandon"
/// CefrLevel = B2
/// DifficultyGroup = Intermediate
/// </summary>
public sealed record CefrWordImportRow
{
    /// <summary>
    /// Kelimenin kullanıcıya gösterilecek orijinal halidir.
    /// 
    /// Örnek:
    /// abandon
    /// Ability
    /// take
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Arama ve duplicate kontrol için normalize edilmiş kelime metnidir.
    /// 
    /// Örnek:
    /// " Abandon " => "abandon"
    /// </summary>
    public string NormalizedText { get; init; } = string.Empty;

    /// <summary>
    /// CEFR seviyesidir.
    /// 
    /// CEFR-J kaynağından A1, A2, B1, B2, C1 veya C2 gelebilir.
    /// Eğer satır parse edilemezse Unknown kalabilir.
    /// </summary>
    public CefrLevel CefrLevel { get; init; } = CefrLevel.Unknown;

    /// <summary>
    /// CEFR seviyesinden türetilen Wordix zorluk grubudur.
    /// 
    /// Mapping:
    /// A1-A2 => Beginner
    /// B1-B2 => Intermediate
    /// C1-C2 => Hard
    /// </summary>
    public DifficultyGroup DifficultyGroup { get; init; } = DifficultyGroup.Unknown;

    /// <summary>
    /// Kelime türü varsa tutulur.
    /// 
    /// Örnek:
    /// noun, verb, adjective
    /// 
    /// CEFR-J dosyasında her zaman güvenilir part of speech olmayabilir.
    /// Bu yüzden nullable bırakıyoruz.
    /// </summary>
    public string? PartOfSpeech { get; init; }

    /// <summary>
    /// Dış kaynaktaki satırı veya kelimeyi temsil eden anahtar değerdir.
    /// 
    /// Örneğin:
    /// - CSV satır numarası
    /// - source dosya adı + satır numarası
    /// - provider key
    /// 
    /// Bu değer import tekrarlarını ve hata takibini kolaylaştırır.
    /// </summary>
    public string? ExternalSourceKey { get; init; }

    /// <summary>
    /// Kaynak dosyadaki satır numarasıdır.
    /// 
    /// Hata olduğunda "şu satırda sorun var" diyebilmek için kullanılır.
    /// Database entity olmak zorunda değildir, import sürecinde yardımcı bilgidir.
    /// </summary>
    public int SourceRowNumber { get; init; }
}