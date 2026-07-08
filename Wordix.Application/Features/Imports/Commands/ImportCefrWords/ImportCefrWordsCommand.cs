using MediatR;
using Wordix.Application.Common.Constants;
using Wordix.Application.Features.Imports.Dtos.Responses;

namespace Wordix.Application.Features.Imports.Commands.ImportCefrWords;

/// <summary>
/// CEFR/word profile kelime listesini sisteme import etmek için kullanılan command'dır.
/// 
/// Bu command şu kaynakları destekleyebilir:
/// - CEFR-J vocabulary profile
/// - Octanove C1/C2 vocabulary profile
/// 
/// Dosya formatı aynı kaldığı sürece aynı provider üzerinden okunabilir.
/// </summary>
public sealed record ImportCefrWordsCommand : IRequest<ImportCefrWordsResponse>
{
    /// <summary>
    /// Import edilecek CSV dosyasının stream içeriğidir.
    /// 
    /// Nullable bırakmamızın sebebi:
    /// Controller dosya gelmezse null geçebilir.
    /// Bu durumu validator yakalar.
    /// </summary>
    public Stream? SourceStream { get; init; }

    /// <summary>
    /// Import edilen dosyanın adı.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Import edilecek kaynağın kısa adıdır.
    /// 
    /// Desteklenen değerler:
    /// - cefrj
    /// - octanove
    /// </summary>
    public string ImportSource { get; init; } = ImportConstants.ImportSources.CefrJ;

    /// <summary>
    /// Import kaynağının versiyon bilgisidir.
    /// 
    /// Örnek:
    /// - CEFR-J için 1.5
    /// - Octanove için 1.0
    /// </summary>
    public string? SourceVersion { get; init; }

    /// <summary>
    /// Import edilecek kelimelerin kaynak dil kodudur.
    /// 
    /// CEFR-J ve Octanove dosyaları İngilizce kelimelerden oluştuğu için
    /// default olarak "en" kullanıyoruz.
    /// </summary>
    public string SourceLanguageCode { get; init; } = ImportConstants.LanguageCodes.English;

    /// <summary>
    /// Tek seferde kaç kelime işleneceğini belirtir.
    /// </summary>
    public int? BatchSize { get; init; }

    /// <summary>
    /// Eğer true ise import sadece okuma/validasyon yapar, database'e kayıt atmaz.
    /// </summary>
    public bool DryRun { get; init; }
}