using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Imports.Dtos.Requests;

/// <summary>
/// Türkçe meaning'i eksik olan Word kayıtlarını Azure Translator ile doldurmak için kullanılan request DTO'sudur.
/// 
/// Bu DTO neden var?
/// - Controller doğrudan command oluşturmak yerine API request modelini alır.
/// - API dış dünyaya DTO ile konuşur.
/// - Command ise Application use-case modelidir.
/// </summary>
public sealed record AzureMissingMeaningBackfillRequest
{
    /// <summary>
    /// Kaynak dil kodudur.
    /// Wordix kelime havuzu İngilizce olduğu için default en.
    /// </summary>
    public string? SourceLanguageCode { get; init; }

    /// <summary>
    /// Hedef meaning dil kodudur.
    /// Wordix için default tr.
    /// </summary>
    public string? TargetLanguageCode { get; init; }

    /// <summary>
    /// Bir çalıştırmada en fazla kaç eksik word işlenecek?
    /// 
    /// Azure maliyeti/kotası için kontrollü ilerlemek gerekir.
    /// </summary>
    public int? MaxItems { get; init; }

    /// <summary>
    /// True ise Azure'a gitmez, DB'ye kayıt atmaz.
    /// Sadece kaç eksik word bulunduğunu ve ilk örnekleri gösterir.
    /// </summary>
    public bool? DryRun { get; init; }

    /// <summary>
    /// Kaç meaning biriktikten sonra SaveChanges yapılacağını belirtir.
    /// </summary>
    public int? BatchSize { get; init; }

    /// <summary>
    /// Response içinde dönecek maksimum mesaj sayısıdır.
    /// </summary>
    public int? MaxMessages { get; init; }

    /// <summary>
    /// Hangi LearningItem kaynaklarına Azure backfill yapılabileceğini belirtir.
    /// 
    /// Varsayılan:
    /// CefrJ, Octanove, Manual.
    /// </summary>
    public IReadOnlyCollection<ContentSource>? AllowedContentSources { get; init; }
}