using System.Text;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Enums;

namespace Wordix.Infrastructure.Providers.Import;

/// <summary>
/// Tatoeba example sentence dosyalarını okuyarak
/// Wordix'in anlayacağı standart example sentence import satırları üretir.
/// 
/// Bu provider ne okur?
/// - SourceSentencesStream: örn. eng_sentences.tsv
/// - TargetSentencesStream: örn. tur_sentences.tsv
/// - LinksStream: örn. links.csv / links.tsv
/// 
/// Tatoeba dosya mantığı:
/// - sentences dosyalarında cümleler id + language code + text olarak tutulur.
/// - links dosyasında iki sentence id arasında çeviri bağlantısı tutulur.
/// 
/// Bu provider ne yapmaz?
/// - Database'e kayıt atmaz.
/// - Sentence entity oluşturmaz.
/// - SentenceTranslation entity oluşturmaz.
/// - LearningItemExampleSentence ilişkisi kurmaz.
/// 
/// Bunlar 24J Example Sentence Enrichment Service aşamasında yapılacaktır.
/// </summary>
public sealed class TatoebaExampleSentenceImportProvider
    : IExampleSentenceImportProvider
{
    /// <summary>
    /// Tatoeba sentence dosyalarında alanlar genelde TAB ile ayrılır.
    /// </summary>
    private const char TabDelimiter = '\t';

    /// <summary>
    /// Dosya adı csv olsa bile Tatoeba export'larında içerik çoğu zaman TAB ayrımlı olabilir.
    /// Yine de bazı dosyalar comma-separated gelebileceği için link parser'da comma fallback tutuyoruz.
    /// </summary>
    private const char CommaDelimiter = ',';

    /// <summary>
    /// Provider ana giriş noktasıdır.
    /// 
    /// Application katmanı bu methodu çağırır.
    /// Infrastructure katmanı gerçek dosya okuma/parsing detayını burada çözer.
    /// </summary>
    public async Task<ExampleSentenceImportProviderResult> LoadAsync(
        ExampleSentenceImportProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var maxMessages = request.MaxMessages <= 0
            ? 100
            : request.MaxMessages;

        var messages = new List<string>();

        // Önce stream doğrulaması yapıyoruz.
        // Bu provider 3 dosyaya ihtiyaç duyar:
        // source sentences, target sentences ve links.
        if (!IsReadable(request.SourceSentencesStream))
        {
            messages.Add("Source sentences file is required and must be readable.");
        }

        if (!IsReadable(request.TargetSentencesStream))
        {
            messages.Add("Target sentences file is required and must be readable.");
        }

        if (!IsReadable(request.LinksStream))
        {
            messages.Add("Links file is required and must be readable.");
        }

        if (messages.Count > 0)
        {
            return ExampleSentenceImportProviderResult.Failure(messages);
        }

        var sourceProviderLanguageCode = NormalizeLanguageCode(
            request.SourceProviderLanguageCode,
            fallback: "eng");

        var targetProviderLanguageCode = NormalizeLanguageCode(
            request.TargetProviderLanguageCode,
            fallback: "tur");

        var sourceLanguageCode = NormalizeLanguageCode(
            request.SourceLanguageCode,
            fallback: ImportConstants.LanguageCodes.English);

        var targetLanguageCode = NormalizeLanguageCode(
            request.TargetLanguageCode,
            fallback: ImportConstants.LanguageCodes.Turkish);

        // Stream başka bir noktadan okunmuş olabilir.
        // Seek destekliyorsa başa alıyoruz.
        ResetStreamIfPossible(request.SourceSentencesStream!);
        ResetStreamIfPossible(request.TargetSentencesStream!);
        ResetStreamIfPossible(request.LinksStream!);

        // 1. Kaynak dil sentence dosyasını dictionary'e alıyoruz.
        //
        // Key: Tatoeba sentence id
        // Value: Sentence text + row number
        var sourceSentences = await LoadSentenceFileAsync(
            stream: request.SourceSentencesStream!,
            expectedProviderLanguageCode: sourceProviderLanguageCode,
            fileLabel: "source sentences",
            maxMessages: maxMessages,
            messages: messages,
            cancellationToken: cancellationToken);

        // 2. Hedef dil sentence dosyasını dictionary'e alıyoruz.
        var targetSentences = await LoadSentenceFileAsync(
            stream: request.TargetSentencesStream!,
            expectedProviderLanguageCode: targetProviderLanguageCode,
            fileLabel: "target sentences",
            maxMessages: maxMessages,
            messages: messages,
            cancellationToken: cancellationToken);

        if (sourceSentences.Count == 0)
        {
            AddMessage(
                messages,
                maxMessages,
                $"No source sentences found for provider language code '{sourceProviderLanguageCode}'.");
        }

        if (targetSentences.Count == 0)
        {
            AddMessage(
                messages,
                maxMessages,
                $"No target sentences found for provider language code '{targetProviderLanguageCode}'.");
        }

        if (sourceSentences.Count == 0 || targetSentences.Count == 0)
        {
            return ExampleSentenceImportProviderResult.Success(
                rows: Array.Empty<ExampleSentenceImportRow>(),
                errors: messages);
        }

        // 3. Link dosyasını okuyup source-target eşleşmelerini çıkarıyoruz.
        var rows = await LoadLinkedSentenceRowsAsync(
            linksStream: request.LinksStream!,
            sourceSentences: sourceSentences,
            targetSentences: targetSentences,
            sourceLanguageCode: sourceLanguageCode,
            targetLanguageCode: targetLanguageCode,
            sourceProviderLanguageCode: sourceProviderLanguageCode,
            targetProviderLanguageCode: targetProviderLanguageCode,
            maxRows: request.MaxRows,
            maxMessages: maxMessages,
            license: request.License,
            messages: messages,
            cancellationToken: cancellationToken);

        if (rows.Count == 0)
        {
            AddMessage(
                messages,
                maxMessages,
                "No linked source-target example sentence pairs were found.");
        }

        return ExampleSentenceImportProviderResult.Success(
            rows: rows,
            errors: messages);
    }

    /// <summary>
    /// Sentence dosyasını okur ve sadece beklenen provider language code'a ait satırları dictionary'e alır.
    /// 
    /// Beklenen Tatoeba sentence formatı:
    /// sentenceId<TAB>languageCode<TAB>sentenceText
    /// 
    /// Örnek:
    /// 1277    eng    I have to go to sleep.
    /// </summary>
    private static async Task<IReadOnlyDictionary<string, TatoebaSentenceRecord>> LoadSentenceFileAsync(
        Stream stream,
        string expectedProviderLanguageCode,
        string fileLabel,
        int maxMessages,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        var sentences = new Dictionary<string, TatoebaSentenceRecord>(
            StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024 * 16,
            leaveOpen: true);

        var rowNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // CA2024:
            // Async method içinde StreamReader.EndOfStream kullanmıyoruz.
            // EndOfStream bazı durumlarda senkron bloklama yapabilir.
            //
            // Doğru async okuma modeli:
            // ReadLineAsync çağrılır; dosya/stream sonuna gelindiyse null döner.
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
            {
                break;
            }

            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = SplitSentenceLine(line);

            if (parts.Length < 3)
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"{fileLabel} line {rowNumber}: invalid sentence row format.");

                continue;
            }

            var externalId = parts[0].Trim();
            var providerLanguageCode = NormalizeLanguageCode(
                parts[1],
                fallback: string.Empty);

            var sentenceText = NormalizeDisplayText(parts[2]);

            // Bu dosyada beklediğimiz dil dışında satırlar varsa sessizce atlıyoruz.
            // Örneğin source dosyasında sadece eng bekliyorsak tur/deu/fra satırlarını almayız.
            if (!string.Equals(
                    providerLanguageCode,
                    expectedProviderLanguageCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(externalId))
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"{fileLabel} line {rowNumber}: sentence id is empty.");

                continue;
            }

            if (string.IsNullOrWhiteSpace(sentenceText))
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"{fileLabel} line {rowNumber}: sentence text is empty.");

                continue;
            }

            if (sentences.ContainsKey(externalId))
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"{fileLabel} line {rowNumber}: duplicate sentence id '{externalId}' skipped.");

                continue;
            }

            sentences[externalId] = new TatoebaSentenceRecord(
                ExternalId: externalId,
                Text: sentenceText,
                RowNumber: rowNumber);
        }

        return sentences;
    }

    /// <summary>
    /// Links dosyasını okuyarak source-target sentence çiftlerini üretir.
    /// 
    /// Link dosyasında yön garanti olmayabilir:
    /// - Bazen sourceId -> targetId gelir.
    /// - Bazen targetId -> sourceId gelir.
    /// 
    /// Bu yüzden iki yönü de kontrol ediyoruz.
    /// </summary>
    private static async Task<IReadOnlyCollection<ExampleSentenceImportRow>> LoadLinkedSentenceRowsAsync(
        Stream linksStream,
        IReadOnlyDictionary<string, TatoebaSentenceRecord> sourceSentences,
        IReadOnlyDictionary<string, TatoebaSentenceRecord> targetSentences,
        string sourceLanguageCode,
        string targetLanguageCode,
        string sourceProviderLanguageCode,
        string targetProviderLanguageCode,
        int? maxRows,
        int maxMessages,
        string? license,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        var rows = new List<ExampleSentenceImportRow>();

        // Aynı source-target bağlantısı link dosyasında tekrar ederse duplicate üretmeyelim.
        var seenPairs = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(
            linksStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024 * 16,
            leaveOpen: true);

        var linkRowNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // CA2024:
            // Async method içinde StreamReader.EndOfStream kullanmak yerine
            // ReadLineAsync sonucunun null olup olmadığını kontrol ediyoruz.
            // Bu, stream sonunu async akışa uygun şekilde yakalar.
            var line = await reader.ReadLineAsync(cancellationToken);

            if (line is null)
            {
                break;
            }

            linkRowNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = SplitLinkLine(line);

            if (parts.Length < 2)
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"links line {linkRowNumber}: invalid link row format.");

                continue;
            }

            var leftId = parts[0].Trim();
            var rightId = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(leftId) || string.IsNullOrWhiteSpace(rightId))
            {
                AddMessage(
                    messages,
                    maxMessages,
                    $"links line {linkRowNumber}: sentence id is empty.");

                continue;
            }

            if (!TryResolveLinkedPair(
                    leftId,
                    rightId,
                    sourceSentences,
                    targetSentences,
                    sourceLanguageCode,
                    targetLanguageCode,
                    sourceProviderLanguageCode,
                    targetProviderLanguageCode,
                    license,
                    linkRowNumber,
                    out var row))
            {
                // Bu link bizim source-target dil çiftimize ait olmayabilir.
                // Tatoeba links dosyası tüm dillerin bağlantılarını içerebildiği için
                // bunu hata saymıyoruz.
                continue;
            }

            var pairKey = $"{row.SourceSentenceExternalId}:{row.TargetSentenceExternalId}";

            if (!seenPairs.Add(pairKey))
            {
                continue;
            }

            rows.Add(row);

            if (maxRows.HasValue && rows.Count >= maxRows.Value)
            {
                break;
            }
        }

        return rows;
    }

    /// <summary>
    /// Linkteki iki id'den source-target eşleşmesi üretmeye çalışır.
    /// 
    /// 1. left source, right target mı?
    /// 2. right source, left target mı?
    /// 
    /// İki ihtimalden biri doğruysa ExampleSentenceImportRow üretir.
    /// </summary>
    private static bool TryResolveLinkedPair(
        string leftId,
        string rightId,
        IReadOnlyDictionary<string, TatoebaSentenceRecord> sourceSentences,
        IReadOnlyDictionary<string, TatoebaSentenceRecord> targetSentences,
        string sourceLanguageCode,
        string targetLanguageCode,
        string sourceProviderLanguageCode,
        string targetProviderLanguageCode,
        string? license,
        int linkRowNumber,
        out ExampleSentenceImportRow row)
    {
        row = default!;

        if (sourceSentences.TryGetValue(leftId, out var sourceSentence) &&
            targetSentences.TryGetValue(rightId, out var targetSentence))
        {
            row = CreateRow(
                sourceSentence,
                targetSentence,
                sourceLanguageCode,
                targetLanguageCode,
                sourceProviderLanguageCode,
                targetProviderLanguageCode,
                license,
                linkRowNumber);

            return true;
        }

        if (sourceSentences.TryGetValue(rightId, out sourceSentence) &&
            targetSentences.TryGetValue(leftId, out targetSentence))
        {
            row = CreateRow(
                sourceSentence,
                targetSentence,
                sourceLanguageCode,
                targetLanguageCode,
                sourceProviderLanguageCode,
                targetProviderLanguageCode,
                license,
                linkRowNumber);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Source ve target sentence record'larından standart import row oluşturur.
    /// </summary>
    private static ExampleSentenceImportRow CreateRow(
        TatoebaSentenceRecord sourceSentence,
        TatoebaSentenceRecord targetSentence,
        string sourceLanguageCode,
        string targetLanguageCode,
        string sourceProviderLanguageCode,
        string targetProviderLanguageCode,
        string? license,
        int linkRowNumber)
    {
        return new ExampleSentenceImportRow
        {
            SourceSentenceExternalId = sourceSentence.ExternalId,
            TargetSentenceExternalId = targetSentence.ExternalId,

            SourceText = sourceSentence.Text,
            NormalizedSourceText = NormalizeSentenceText(sourceSentence.Text),

            TranslatedText = targetSentence.Text,
            NormalizedTranslatedText = NormalizeSentenceText(targetSentence.Text),

            SourceLanguageCode = sourceLanguageCode,
            TargetLanguageCode = targetLanguageCode,

            SourceProviderLanguageCode = sourceProviderLanguageCode,
            TargetProviderLanguageCode = targetProviderLanguageCode,

            ContentSource = ContentSource.Tatoeba,
            QualityStatus = ContentQualityStatus.Imported,
            SourceProvider = ImportConstants.ProviderNames.Tatoeba,
            License = license,

            ExternalSourceKey = BuildExternalSourceKey(
                sourceSentence.ExternalId,
                targetSentence.ExternalId),

            SourceSentenceRowNumber = sourceSentence.RowNumber,
            TargetSentenceRowNumber = targetSentence.RowNumber,
            LinkRowNumber = linkRowNumber
        };
    }

    /// <summary>
    /// Sentence satırını ayırır.
    /// 
    /// Count = 3 kullanıyoruz.
    /// Çünkü sentenceText içinde teorik olarak delimiter benzeri karakterler olabilir.
    /// İlk iki alan id ve language code, kalan her şey sentence text kabul edilir.
    /// </summary>
    private static string[] SplitSentenceLine(string line)
    {
        return line.Split(
            TabDelimiter,
            count: 3,
            StringSplitOptions.None);
    }

    /// <summary>
    /// Link satırını ayırır.
    /// 
    /// Tatoeba link dosyası çoğunlukla TAB ayrımlıdır.
    /// Ama dosya adı csv ise comma-separated gelme ihtimaline karşı comma fallback vardır.
    /// </summary>
    private static string[] SplitLinkLine(string line)
    {
        var delimiter = line.Contains(TabDelimiter)
            ? TabDelimiter
            : CommaDelimiter;

        return line.Split(
            delimiter,
            count: 2,
            StringSplitOptions.None);
    }

    /// <summary>
    /// Response/DB eşleşmeleri için sentence text normalize eder.
    /// 
    /// Bu normalize işlemi:
    /// - baş/son boşlukları temizler
    /// - çoklu boşlukları tek boşluğa indirir
    /// - küçük harfe çevirir
    /// 
    /// Noktalama işaretlerini şu an silmiyoruz.
    /// Çünkü cümlelerde punctuation anlamlı olabilir.
    /// </summary>
    private static string NormalizeSentenceText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return NormalizeDisplayText(value)
            .ToLowerInvariant();
    }

    /// <summary>
    /// Görünen text için baş/son boşlukları temizler ve çoklu whitespace'i sadeleştirir.
    /// Büyük/küçük harfi korur.
    /// </summary>
    private static string NormalizeDisplayText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value
            .Trim()
            .Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Dil kodlarını güvenli şekilde normalize eder.
    /// </summary>
    private static string NormalizeLanguageCode(
        string? value,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Tatoeba pair için dış kaynak takip anahtarı üretir.
    /// </summary>
    private static string BuildExternalSourceKey(
        string sourceSentenceExternalId,
        string targetSentenceExternalId)
    {
        var externalSourceKey =
            $"{ImportConstants.ProviderNames.Tatoeba}:sentence:{sourceSentenceExternalId}:{targetSentenceExternalId}";

        return externalSourceKey.Length <= ImportConstants.MaxExternalSourceKeyLength
            ? externalSourceKey
            : externalSourceKey[..ImportConstants.MaxExternalSourceKeyLength];
    }

    /// <summary>
    /// Stream seek destekliyorsa başa alır.
    /// </summary>
    private static void ResetStreamIfPossible(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
    }

    /// <summary>
    /// Stream okunabilir mi kontrol eder.
    /// </summary>
    private static bool IsReadable(Stream? stream)
    {
        return stream is not null && stream.CanRead;
    }

    /// <summary>
    /// Hata/uyarı mesajlarını sınırlı sayıda tutar.
    /// Büyük dosyalarda response'un şişmesini engeller.
    /// </summary>
    private static void AddMessage(
        List<string> messages,
        int maxMessages,
        string message)
    {
        if (messages.Count < maxMessages)
        {
            messages.Add(message);
        }
    }

    /// <summary>
    /// Tatoeba sentence dosyasından okunan cümle bilgisini temsil eden küçük iç modeldir.
    /// 
    /// Entity değildir.
    /// Sadece parser içinde kullanılır.
    /// </summary>
    private sealed record TatoebaSentenceRecord(
        string ExternalId,
        string Text,
        int RowNumber);
}