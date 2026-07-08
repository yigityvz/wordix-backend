using System.Text;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Extensions;

namespace Wordix.Infrastructure.Providers.Import;

/// <summary>
/// CEFR-J vocabulary CSV dosyasını okuyup Wordix import satırlarına çeviren provider'dır.
/// 
/// Bu class neden Infrastructure katmanında?
/// - CSV okuma teknik bir detaydır.
/// - Dosya formatı, header isimleri, satır parse etme gibi işler domain iş kuralı değildir.
/// - Application katmanı sadece ICefrWordListProvider interface'ini bilir.
/// 
/// Bu provider'ın görevi:
/// - CSV header satırını okumak.
/// - Kelime, CEFR ve part of speech kolonlarını bulmak.
/// - Her geçerli satırı CefrWordImportRow modeline çevirmek.
/// - Hatalı satırları errors listesine eklemek.
/// </summary>
public sealed class CefrJCsvWordListProvider : ICefrWordListProvider
{
    /// <summary>
    /// CEFR-J CSV dosyasını stream üzerinden okur.
    /// 
    /// Stream kullanmamız sayesinde kaynak şunlardan biri olabilir:
    /// - Local dosya
    /// - API upload
    /// - GitHub'dan indirilmiş memory stream
    /// </summary>
    public async Task<CefrWordListProviderResult> LoadAsync(
        Stream sourceStream,
        CancellationToken cancellationToken = default)
    {
        if (sourceStream is null)
        {
            throw new ArgumentNullException(nameof(sourceStream));
        }

        if (!sourceStream.CanRead)
        {
            return CefrWordListProviderResult.Failure(
                new[] { "CEFR-J source stream okunabilir değil." });
        }

        // Caller stream'i daha önce okumuş olabilir.
        // Seek destekliyorsa başa alarak güvenli başlıyoruz.
        if (sourceStream.CanSeek)
        {
            sourceStream.Position = 0;
        }

        var rows = new List<CefrWordImportRow>();
        var errors = new List<string>();

        using var reader = new StreamReader(
            sourceStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return CefrWordListProviderResult.Failure(
                new[] { "CEFR-J CSV dosyası boş veya header satırı yok." });
        }

        var headers = ParseCsvLine(headerLine)
            .Select(NormalizeHeader)
            .ToList();

        var wordColumnIndex = FindHeaderIndex(
            headers,
            "headword",
            "word",
            "lemma",
            "text");

        var cefrColumnIndex = FindHeaderIndex(
            headers,
            "cefr",
            "cefrlevel",
            "level");

        var partOfSpeechColumnIndex = FindHeaderIndex(
            headers,
            "pos",
            "partofspeech",
            "wordclass");

        if (wordColumnIndex < 0)
        {
            errors.Add("CSV içinde kelime kolonu bulunamadı. Beklenen header: headword, word, lemma veya text.");
        }

        if (cefrColumnIndex < 0)
        {
            errors.Add("CSV içinde CEFR kolonu bulunamadı. Beklenen header: CEFR, cefrLevel veya level.");
        }

        if (errors.Count > 0)
        {
            return CefrWordListProviderResult.Failure(errors);
        }

        var lineNumber = 1;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Async dosya okuma için EndOfStream kontrolü yapmıyoruz.
            // ReadLineAsync null dönerse dosyanın sonuna gelmişiz demektir.
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
            {
                break;
            }

            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var columns = ParseCsvLine(line);

            var rawWord = GetColumnValue(columns, wordColumnIndex);
            var rawCefrLevel = GetColumnValue(columns, cefrColumnIndex);
            var rawPartOfSpeech = partOfSpeechColumnIndex >= 0
                ? GetColumnValue(columns, partOfSpeechColumnIndex)
                : null;

            if (string.IsNullOrWhiteSpace(rawWord))
            {
                errors.Add($"Line {lineNumber}: Kelime değeri boş olduğu için satır atlandı.");
                continue;
            }

            if (!CefrLevelExtensions.TryParseCefrLevel(rawCefrLevel, out var cefrLevel))
            {
                errors.Add($"Line {lineNumber}: CEFR değeri parse edilemedi. Value: '{rawCefrLevel}'.");
                continue;
            }

            var normalizedText = NormalizeWord(rawWord);

            if (string.IsNullOrWhiteSpace(normalizedText))
            {
                errors.Add($"Line {lineNumber}: Normalize edilmiş kelime boş olduğu için satır atlandı.");
                continue;
            }

            var externalSourceKey = BuildExternalSourceKey(lineNumber, normalizedText);

            rows.Add(new CefrWordImportRow
            {
                Text = rawWord.Trim(),
                NormalizedText = normalizedText,
                CefrLevel = cefrLevel,
                DifficultyGroup = cefrLevel.ToDifficultyGroup(),
                PartOfSpeech = NormalizeOptionalText(rawPartOfSpeech),
                ExternalSourceKey = externalSourceKey,
                SourceRowNumber = lineNumber
            });
        }

        if (rows.Count == 0)
        {
            errors.Add("CEFR-J CSV okundu fakat import edilebilir geçerli satır bulunamadı.");
            return CefrWordListProviderResult.Failure(errors);
        }

        return CefrWordListProviderResult.Success(rows, errors);
    }

    /// <summary>
    /// CSV satırını kolonlara ayırır.
    /// 
    /// Neden hazır Split(',') kullanmıyoruz?
    /// Çünkü CSV içinde tırnaklı değerlerde virgül olabilir.
    /// Örnek:
    /// "hello, world",A1
    /// 
    /// Bu küçük parser temel CSV quote kurallarını destekler.
    /// </summary>
    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var isInsideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            if (currentChar == '"')
            {
                // CSV'de çift tırnak içinde "" iki tırnak karakteri gerçek " anlamına gelir.
                if (isInsideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++;
                    continue;
                }

                isInsideQuotes = !isInsideQuotes;
                continue;
            }

            if (currentChar == ',' && !isInsideQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
                continue;
            }

            currentValue.Append(currentChar);
        }

        values.Add(currentValue.ToString().Trim());

        return values;
    }

    /// <summary>
    /// Header isimlerini karşılaştırılabilir hale getirir.
    /// 
    /// Örnek:
    /// " CEFR Level " => "cefrlevel"
    /// "part_of_speech" => "partofspeech"
    /// </summary>
    private static string NormalizeHeader(string value)
    {
        return value
            .Trim()
            .TrimStart('\uFEFF')
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Header listesinde verilen alias'lardan birini arar.
    /// 
    /// Örnek:
    /// headword, word, lemma gibi farklı header isimlerini aynı anlamda kabul ederiz.
    /// </summary>
    private static int FindHeaderIndex(
        IReadOnlyList<string> normalizedHeaders,
        params string[] acceptedHeaderNames)
    {
        for (var index = 0; index < normalizedHeaders.Count; index++)
        {
            foreach (var acceptedHeaderName in acceptedHeaderNames)
            {
                if (normalizedHeaders[index] == NormalizeHeader(acceptedHeaderName))
                {
                    return index;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Belirtilen index'teki kolon değerini güvenli şekilde döner.
    /// Index satırdaki kolon sayısını aşıyorsa null döner.
    /// </summary>
    private static string? GetColumnValue(
        IReadOnlyList<string> columns,
        int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= columns.Count)
        {
            return null;
        }

        return columns[columnIndex];
    }

    /// <summary>
    /// Kelimeyi arama ve duplicate kontrol için normalize eder.
    /// </summary>
    private static string NormalizeWord(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Opsiyonel metinleri trimler.
    /// Boşsa null döner.
    /// </summary>
    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    /// <summary>
    /// Import edilen satır için dış kaynak anahtarı üretir.
    /// 
    /// Bu değer LearningItem.ExternalSourceKey alanında kullanılabilir.
    /// Böylece ileride hangi kelimenin hangi source satırından geldiğini takip ederiz.
    /// </summary>
    private static string BuildExternalSourceKey(
        int lineNumber,
        string normalizedText)
    {
        var sourceKey = $"cefr-j:v1.5:line-{lineNumber}:{normalizedText}";

        if (sourceKey.Length <= ImportConstants.MaxExternalSourceKeyLength)
        {
            return sourceKey;
        }

        return sourceKey[..ImportConstants.MaxExternalSourceKeyLength];
    }
}