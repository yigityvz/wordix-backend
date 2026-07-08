using System.Text;
using System.Xml;
using System.Xml.Linq;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Enums;

namespace Wordix.Infrastructure.Providers.Import;

/// <summary>
/// FreeDict English-Turkish TEI XML dosyasını okuyup Wordix meaning import satırlarına çeviren provider'dır.
/// 
/// Bu provider ne yapar?
/// - eng-tur.tei XML dosyasını okur.
/// - Her TEI entry içinden İngilizce source word değerini alır.
/// - Entry/sense içindeki Türkçe translation quote değerlerini toplar.
/// - Her çeviriyi MeaningImportRow modeline dönüştürür.
/// 
/// Bu provider ne yapmaz?
/// - Database'e kayıt atmaz.
/// - LearningItem eşleştirme yapmaz.
/// - Duplicate DB kontrolü yapmaz.
/// 
/// DB eşleştirme ve Meaning insert işlemi MeaningEnrichmentService tarafından yapılır.
/// </summary>
public sealed class FreeDictMeaningImportProvider : IFreeDictMeaningImportProvider
{
    /// <summary>
    /// Çok fazla hata mesajı response'u şişirmesin diye sınır koyuyoruz.
    /// </summary>
    private const int MaxErrorMessages = 100;

    /// <summary>
    /// Meaning.MeaningText kolon limitine uygun maksimum anlam uzunluğu.
    /// Çok uzun FreeDict quote değerleri genelde yanlış bölünmüş/sözlük maddesi taşmış veridir.
    /// </summary>
    private const int MaxMeaningTextLength = 200;

    /// <summary>
    /// FreeDict eng-tur kaynağı için lisans/attribution bilgisidir.
    /// 
    /// Bunu Meaning.License alanına taşıyacağız.
    /// Production'da README veya attribution dokümanında ayrıca belirtmek gerekir.
    /// </summary>
    private const string LicenseText = "FreeDict eng-tur dictionary, GPL; based on gtksozluk2.";

    /// <summary>
    /// FreeDict TEI XML stream'ini okuyup MeaningImportRow listesi üretir.
    /// </summary>
    public async Task<MeaningImportProviderResult> LoadAsync(
        MeaningImportProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceStream is null)
        {
            return MeaningImportProviderResult.Failure(
                new[] { "FreeDict source stream zorunludur." });
        }

        if (!request.SourceStream.CanRead)
        {
            return MeaningImportProviderResult.Failure(
                new[] { "FreeDict source stream okunabilir değil." });
        }

        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);
        var targetLanguageCode = NormalizeLanguageCode(request.TargetLanguageCode);

        if (string.IsNullOrWhiteSpace(sourceLanguageCode))
        {
            return MeaningImportProviderResult.Failure(
                new[] { "Kaynak dil kodu boş olamaz." });
        }

        if (string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            return MeaningImportProviderResult.Failure(
                new[] { "Hedef dil kodu boş olamaz." });
        }

        if (request.SourceStream.CanSeek)
        {
            request.SourceStream.Position = 0;
        }

        var rows = new List<MeaningImportRow>();
        var errors = new List<string>();
        var seenMeaningKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var settings = new XmlReaderSettings
        {
            Async = true,

            // DTD/schema çözmeye çalışmasını istemiyoruz.
            // Çünkü GitHub'dan indirilen TEI dosyası local DTD referansı içerebilir.
            DtdProcessing = DtdProcessing.Ignore,
            XmlResolver = null,

            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        try
        {
            using var reader = XmlReader.Create(
                request.SourceStream,
                settings);

            var entryIndex = 0;

            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType != XmlNodeType.Element ||
                    !IsElement(reader, "entry"))
                {
                    continue;
                }

                entryIndex++;

                XElement entryElement;

                try
                {
                    // Her entry'yi ayrı XElement olarak alıyoruz.
                    // 24 MB dosyada bu yaklaşım yeterince güvenli ve parser kodunu sade tutar.
                    using var subtreeReader = reader.ReadSubtree();
                    entryElement = await XElement.LoadAsync(
                        subtreeReader,
                        LoadOptions.None,
                        cancellationToken);
                }
                catch (Exception exception) when (exception is XmlException or InvalidOperationException)
                {
                    AddError(
                        errors,
                        $"Entry {entryIndex}: XML entry parse edilemedi. Error: {exception.Message}");

                    continue;
                }

                ParseEntry(
                    entryElement,
                    entryIndex,
                    request,
                    sourceLanguageCode,
                    targetLanguageCode,
                    rows,
                    errors,
                    seenMeaningKeys);

                if (request.MaxRows.HasValue &&
                    rows.Count >= request.MaxRows.Value)
                {
                    return MeaningImportProviderResult.Success(
                        rows.Take(request.MaxRows.Value).ToArray(),
                        errors);
                }
            }
        }
        catch (XmlException exception)
        {
            AddError(
                errors,
                $"FreeDict XML parse edilemedi. Error: {exception.Message}");

            return MeaningImportProviderResult.Failure(errors);
        }
        catch (IOException exception)
        {
            AddError(
                errors,
                $"FreeDict dosyası okunamadı. Error: {exception.Message}");

            return MeaningImportProviderResult.Failure(errors);
        }

        if (rows.Count == 0)
        {
            AddError(
                errors,
                "FreeDict TEI dosyası okundu fakat import edilebilir Türkçe meaning satırı bulunamadı.");

            return MeaningImportProviderResult.Failure(errors);
        }

        return MeaningImportProviderResult.Success(rows, errors);
    }

    /// <summary>
    /// Tek bir TEI entry elementini MeaningImportRow kayıtlarına dönüştürür.
    /// </summary>
    private static void ParseEntry(
        XElement entryElement,
        int entryIndex,
        MeaningImportProviderRequest request,
        string sourceLanguageCode,
        string targetLanguageCode,
        List<MeaningImportRow> rows,
        List<string> errors,
        HashSet<string> seenMeaningKeys)
    {
        var sourceText = NormalizeDisplayText(
            FindFirstDescendantValue(entryElement, "orth"));

        if (string.IsNullOrWhiteSpace(sourceText))
        {
            AddError(
                errors,
                $"Entry {entryIndex}: orth/source word bulunamadı.");

            return;
        }

        var normalizedSourceText = NormalizeText(sourceText);

        if (string.IsNullOrWhiteSpace(normalizedSourceText))
        {
            AddError(
                errors,
                $"Entry {entryIndex}: normalize source word boş.");

            return;
        }

        var isPhraseCandidate = request.IncludePhraseCandidates &&
                                IsPhraseCandidate(sourceText);

        var partOfSpeech = NormalizeOptionalText(
            FindPartOfSpeech(entryElement));

        var senseElements = entryElement
            .Descendants()
            .Where(element => IsElement(element, "sense"))
            .ToArray();

        if (senseElements.Length == 0)
        {
            // Bazı TEI entry'lerde sense olmayabilir.
            // Bu durumda entry seviyesindeki trans cit'leri deniyoruz.
            ParseTranslationsFromScope(
                entryElement,
                entryIndex,
                senseIndex: null,
                sourceText,
                normalizedSourceText,
                partOfSpeech,
                isPhraseCandidate,
                sourceLanguageCode,
                targetLanguageCode,
                rows,
                errors,
                seenMeaningKeys,
                request.MaxRows);

            return;
        }

        for (var senseIndex = 0; senseIndex < senseElements.Length; senseIndex++)
        {
            ParseTranslationsFromScope(
                senseElements[senseIndex],
                entryIndex,
                senseIndex + 1,
                sourceText,
                normalizedSourceText,
                partOfSpeech,
                isPhraseCandidate,
                sourceLanguageCode,
                targetLanguageCode,
                rows,
                errors,
                seenMeaningKeys,
                request.MaxRows);

            if (request.MaxRows.HasValue &&
                rows.Count >= request.MaxRows.Value)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Verilen scope içindeki translation quote değerlerini MeaningImportRow'a çevirir.
    /// </summary>
    private static void ParseTranslationsFromScope(
        XElement scopeElement,
        int entryIndex,
        int? senseIndex,
        string sourceText,
        string normalizedSourceText,
        string? partOfSpeech,
        bool isPhraseCandidate,
        string sourceLanguageCode,
        string targetLanguageCode,
        List<MeaningImportRow> rows,
        List<string> errors,
        HashSet<string> seenMeaningKeys,
        int? maxRows)
    {
        var translationTexts = ExtractTranslationTexts(scopeElement);

        if (translationTexts.Count == 0)
        {
            return;
        }

        var translationIndex = 0;

        foreach (var translationText in translationTexts)
        {
            if (maxRows.HasValue &&
                rows.Count >= maxRows.Value)
            {
                return;
            }

            translationIndex++;

            var meaningText = NormalizeDisplayText(translationText);
            var normalizedMeaningText = NormalizeText(meaningText);

            if (string.IsNullOrWhiteSpace(meaningText) ||
                string.IsNullOrWhiteSpace(normalizedMeaningText))
            {
                continue;
            }

            if (meaningText.Length > MaxMeaningTextLength)
            {
                AddError(
                    errors,
                    $"Entry {entryIndex}: MeaningText {MaxMeaningTextLength} karakter sınırını aştığı için atlandı. SourceText: {sourceText}");

                continue;
            }

            var duplicateKey = BuildDuplicateKey(
                normalizedSourceText,
                partOfSpeech,
                normalizedMeaningText,
                targetLanguageCode);

            if (!seenMeaningKeys.Add(duplicateKey))
            {
                continue;
            }

            rows.Add(new MeaningImportRow
            {
                SourceText = sourceText,
                NormalizedSourceText = normalizedSourceText,
                SourceLanguageCode = sourceLanguageCode,
                TargetLanguageCode = targetLanguageCode,

                MeaningText = meaningText,
                NormalizedMeaningText = normalizedMeaningText,

                ShortDefinition = null,
                PartOfSpeech = partOfSpeech,
                Category = null,
                IsPhraseCandidate = isPhraseCandidate,

                ContentSource = ContentSource.FreeDict,
                QualityStatus = ContentQualityStatus.Imported,
                SourceProvider = ImportConstants.ProviderNames.FreeDict,
                License = LicenseText,

                ExternalSourceKey = BuildExternalSourceKey(
                    entryIndex,
                    senseIndex,
                    translationIndex,
                    normalizedSourceText),

                SourceRowNumber = entryIndex,
                SenseIndex = senseIndex,
                TranslationIndex = translationIndex
            });
        }
    }

    /// <summary>
    /// TEI içindeki translation quote değerlerini çıkarır.
    /// 
    /// Öncelik:
    /// - cit type="trans" altındaki quote değerleri.
    /// 
    /// Böylece örnek cümle veya başka quote değerlerini yanlışlıkla meaning sanma riskini azaltırız.
    /// </summary>
    private static IReadOnlyCollection<string> ExtractTranslationTexts(
        XElement scopeElement)
    {
        var translationQuotes = scopeElement
            .Descendants()
            .Where(element =>
                IsElement(element, "cit") &&
                string.Equals(
                    GetAttributeValue(element, "type"),
                    "trans",
                    StringComparison.OrdinalIgnoreCase))
            .SelectMany(cit => cit
                .Descendants()
                .Where(descendant => IsElement(descendant, "quote")))
            .Select(quote => NormalizeDisplayText(quote.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (translationQuotes.Length > 0)
        {
            return translationQuotes;
        }

        // Bazı entry'lerde cit type bilgisi eksik olabilir.
        // Fallback olarak sadece doğrudan quote değerlerini deniyoruz.
        return scopeElement
            .Descendants()
            .Where(element => IsElement(element, "quote"))
            .Select(quote => NormalizeDisplayText(quote.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Entry içinden part-of-speech bilgisini bulmaya çalışır.
    /// </summary>
    private static string? FindPartOfSpeech(
        XElement entryElement)
    {
        var typedGram = entryElement
            .Descendants()
            .FirstOrDefault(element =>
                IsElement(element, "gram") &&
                string.Equals(
                    GetAttributeValue(element, "type"),
                    "pos",
                    StringComparison.OrdinalIgnoreCase));

        if (typedGram is not null)
        {
            return typedGram.Value;
        }

        var posElement = entryElement
            .Descendants()
            .FirstOrDefault(element => IsElement(element, "pos"));

        return posElement?.Value;
    }

    /// <summary>
    /// İlk descendant text değerini localName'e göre bulur.
    /// Namespace farklarını yok sayar.
    /// </summary>
    private static string? FindFirstDescendantValue(
        XElement element,
        string localName)
    {
        return element
            .Descendants()
            .FirstOrDefault(descendant => IsElement(descendant, localName))
            ?.Value;
    }

    /// <summary>
    /// Element local name karşılaştırması yapar.
    /// TEI namespace olsa bile local name üzerinden güvenli çalışır.
    /// </summary>
    private static bool IsElement(
        XElement element,
        string localName)
    {
        return string.Equals(
            element.Name.LocalName,
            localName,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// XmlReader local name karşılaştırması yapar.
    /// </summary>
    private static bool IsElement(
        XmlReader reader,
        string localName)
    {
        return string.Equals(
            reader.LocalName,
            localName,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attribute değerini namespace bağımsız okur.
    /// </summary>
    private static string? GetAttributeValue(
        XElement element,
        string attributeName)
    {
        return element
            .Attributes()
            .FirstOrDefault(attribute =>
                string.Equals(
                    attribute.Name.LocalName,
                    attributeName,
                    StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    /// <summary>
    /// Bir source text'in phrase olup olmadığını belirler.
    /// </summary>
    private static bool IsPhraseCandidate(
        string sourceText)
    {
        return sourceText.Contains(' ', StringComparison.Ordinal);
    }

    /// <summary>
    /// Aynı source + POS + meaning + target language kombinasyonunu tekrar üretmemek için key oluşturur.
    /// </summary>
    private static string BuildDuplicateKey(
        string normalizedSourceText,
        string? partOfSpeech,
        string normalizedMeaningText,
        string targetLanguageCode)
    {
        return string.Join(
            "|",
            normalizedSourceText,
            partOfSpeech ?? string.Empty,
            normalizedMeaningText,
            targetLanguageCode);
    }

    /// <summary>
    /// Dış kaynak takibi için source key üretir.
    /// 
    /// Örnek:
    /// freedict:entry-123:sense-1:translation-2:abandon
    /// </summary>
    private static string BuildExternalSourceKey(
        int entryIndex,
        int? senseIndex,
        int translationIndex,
        string normalizedSourceText)
    {
        var sourceKey =
            $"freedict:entry-{entryIndex}:sense-{senseIndex?.ToString() ?? "none"}:translation-{translationIndex}:{normalizedSourceText}";

        if (sourceKey.Length <= ImportConstants.MaxExternalSourceKeyLength)
        {
            return sourceKey;
        }

        return sourceKey[..ImportConstants.MaxExternalSourceKeyLength];
    }

    /// <summary>
    /// Hata mesajını listeye ekler.
    /// </summary>
    private static void AddError(
        List<string> errors,
        string message)
    {
        if (errors.Count >= MaxErrorMessages)
        {
            return;
        }

        errors.Add(message);
    }

    private static string NormalizeLanguageCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string NormalizeDisplayText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}