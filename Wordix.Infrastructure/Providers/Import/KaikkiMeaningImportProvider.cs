using System.Text;
using System.Text.Json;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Common.Models.Import;
using Wordix.Domain.Enums;

namespace Wordix.Infrastructure.Providers.Import;

/// <summary>
/// Kaikki/Wiktionary JSONL datasını okuyup Wordix meaning import satırlarına çeviren provider'dır.
/// 
/// Bu provider neden Infrastructure katmanında?
/// - JSONL dosyası okuma teknik bir detaydır.
/// - Kaikki/Wiktionary JSON property isimleri Application katmanına sızmamalıdır.
/// - Application katmanı sadece IMeaningImportProvider interface'ini bilir.
/// 
/// Bu provider'ın görevi:
/// - JSONL dosyasını satır satır okumak.
/// - Her satırı JSON object olarak parse etmek.
/// - Sadece istenen kaynak dildeki entry'leri almak.
/// - Sadece hedef dildeki translation kayıtlarını almak.
/// - Her translation'ı MeaningImportRow modeline çevirmek.
/// 
/// Not:
/// Bu fazda database'e kayıt atılmaz.
/// Sadece parser çıktısı standart modele dönüştürülür.
/// </summary>
public sealed class KaikkiMeaningImportProvider : IMeaningImportProvider
{
    /// <summary>
    /// Parser çok büyük dosyalarda aşırı hata mesajı üretmesin diye
    /// response errors listesini sınırlıyoruz.
    /// 
    /// ImportJob logging gelince detaylı satır bazlı loglar database'e yazılacak.
    /// Şimdilik provider result içinde ilk belirli sayıda hata yeterli.
    /// </summary>
    private const int MaxErrorMessages = 100;

    /// <summary>
    /// Kaikki/Wiktionary JSONL dosyasını stream üzerinden okur.
    /// 
    /// JSONL formatında her satır ayrı bir JSON object kabul edilir.
    /// Bu yüzden dosyayı komple memory'ye almadan line-by-line okuyoruz.
    /// </summary>
    public async Task<MeaningImportProviderResult> LoadAsync(
        MeaningImportProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SourceStream is null)
        {
            return MeaningImportProviderResult.Failure(
                new[] { "Kaikki source stream zorunludur." });
        }

        if (!request.SourceStream.CanRead)
        {
            return MeaningImportProviderResult.Failure(
                new[] { "Kaikki source stream okunabilir değil." });
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

        // Caller stream'i daha önce okumuş olabilir.
        // Seek destekliyorsa başa alarak güvenli başlıyoruz.
        if (request.SourceStream.CanSeek)
        {
            request.SourceStream.Position = 0;
        }

        var rows = new List<MeaningImportRow>();
        var errors = new List<string>();

        // Aynı dosya içinde aynı kelime + aynı anlam tekrar gelirse
        // duplicate import row üretmeyelim diye HashSet kullanıyoruz.
        var seenMeaningKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(
            request.SourceStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        var lineNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(line);
            }
            catch (JsonException)
            {
                AddError(
                    errors,
                    $"Line {lineNumber}: JSON parse edilemedi.");

                continue;
            }

            using (document)
            {
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    AddError(
                        errors,
                        $"Line {lineNumber}: JSON root object değil.");

                    continue;
                }

                var entryLanguageCode = NormalizeLanguageCode(
                    GetStringProperty(root, "lang_code") ??
                    GetStringProperty(root, "lang"));

                // Kaikki raw data içinde farklı diller olabilir.
                // Biz şu an İngilizce CEFR pool'u enrich ettiğimiz için sadece sourceLanguageCode = en entry'lerini alıyoruz.
                if (!string.Equals(
                        entryLanguageCode,
                        sourceLanguageCode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var sourceText = NormalizeDisplayText(
                    GetStringProperty(root, "word"));

                if (string.IsNullOrWhiteSpace(sourceText))
                {
                    AddError(
                        errors,
                        $"Line {lineNumber}: Entry word değeri boş olduğu için satır atlandı.");

                    continue;
                }

                var normalizedSourceText = NormalizeText(sourceText);

                if (string.IsNullOrWhiteSpace(normalizedSourceText))
                {
                    AddError(
                        errors,
                        $"Line {lineNumber}: Normalize edilmiş source text boş olduğu için satır atlandı.");

                    continue;
                }

                var partOfSpeech = NormalizeOptionalText(
                    GetStringProperty(root, "pos"));

                var firstGloss = ExtractFirstGloss(root);

                var isPhraseCandidate = request.IncludePhraseCandidates &&
                                        IsPhraseCandidate(root, sourceText);

                if (!root.TryGetProperty("translations", out var translationsElement) ||
                    translationsElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var translationIndex = 0;

                foreach (var translationElement in translationsElement.EnumerateArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    translationIndex++;

                    if (translationElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    var translationLanguageCode = NormalizeLanguageCode(
                        GetStringProperty(translationElement, "lang_code") ??
                        GetStringProperty(translationElement, "code") ??
                        GetStringProperty(translationElement, "lang"));

                    if (!string.Equals(
                            translationLanguageCode,
                            targetLanguageCode,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var meaningText = NormalizeDisplayText(
                        GetStringProperty(translationElement, "word") ??
                        GetStringProperty(translationElement, "translation") ??
                        GetStringProperty(translationElement, "text"));

                    if (string.IsNullOrWhiteSpace(meaningText))
                    {
                        continue;
                    }

                    var normalizedMeaningText = NormalizeText(meaningText);

                    if (string.IsNullOrWhiteSpace(normalizedMeaningText))
                    {
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

                    var shortDefinition = NormalizeOptionalText(
                        GetStringProperty(translationElement, "sense")) ??
                        firstGloss;

                    var externalSourceKey = BuildExternalSourceKey(
                        lineNumber,
                        translationIndex,
                        normalizedSourceText);

                    rows.Add(new MeaningImportRow
                    {
                        SourceText = sourceText,
                        NormalizedSourceText = normalizedSourceText,
                        SourceLanguageCode = sourceLanguageCode,
                        TargetLanguageCode = targetLanguageCode,
                        MeaningText = meaningText,
                        NormalizedMeaningText = normalizedMeaningText,
                        ShortDefinition = shortDefinition,
                        PartOfSpeech = partOfSpeech,
                        Category = null,
                        IsPhraseCandidate = isPhraseCandidate,
                        ContentSource = ContentSource.WiktionaryKaikki,
                        QualityStatus = ContentQualityStatus.Imported,
                        SourceProvider = ImportConstants.ProviderNames.WiktionaryKaikki,
                        License = null,
                        ExternalSourceKey = externalSourceKey,
                        SourceRowNumber = lineNumber,
                        SenseIndex = null,
                        TranslationIndex = translationIndex
                    });

                    if (request.MaxRows.HasValue &&
                        rows.Count >= request.MaxRows.Value)
                    {
                        return MeaningImportProviderResult.Success(rows, errors);
                    }
                }
            }
        }

        if (rows.Count == 0)
        {
            AddError(
                errors,
                "Kaikki JSONL okundu fakat import edilebilir Türkçe meaning satırı bulunamadı.");

            return MeaningImportProviderResult.Failure(errors);
        }

        return MeaningImportProviderResult.Success(rows, errors);
    }

    /// <summary>
    /// Hata mesajını listeye ekler.
    /// 
    /// Çok büyük import dosyalarında binlerce hata mesajı oluşabilir.
    /// Bu yüzden mesaj sayısını sınırlıyoruz.
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

    /// <summary>
    /// JSON object içinden string property okur.
    /// Property yoksa veya string değilse null döner.
    /// </summary>
    private static string? GetStringProperty(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var propertyElement))
        {
            return null;
        }

        return propertyElement.ValueKind == JsonValueKind.String
            ? propertyElement.GetString()
            : null;
    }

    /// <summary>
    /// Kaikki entry içindeki ilk gloss bilgisini döndürür.
    /// 
    /// Kaikki/Wiktionary datasında anlam açıklamaları genellikle senses[].glosses[] içinde bulunur.
    /// Bunu ShortDefinition için kullanabiliriz.
    /// </summary>
    private static string? ExtractFirstGloss(JsonElement root)
    {
        if (!root.TryGetProperty("senses", out var sensesElement) ||
            sensesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var senseElement in sensesElement.EnumerateArray())
        {
            if (senseElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!senseElement.TryGetProperty("glosses", out var glossesElement) ||
                glossesElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var glossElement in glossesElement.EnumerateArray())
            {
                if (glossElement.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var gloss = NormalizeOptionalText(glossElement.GetString());

                if (!string.IsNullOrWhiteSpace(gloss))
                {
                    return gloss;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Bir entry'nin phrase/phrasal verb/idiom adayı olup olmadığını belirler.
    /// 
    /// Bu fazda phrase'i database'e kaydetmiyoruz.
    /// Sadece parser çıktısında işaretliyoruz.
    /// 
    /// Aday kuralları:
    /// - Source text içinde boşluk varsa phrase adayıdır.
    /// - tags/categories içinde phrase, idiom, proverb, phrasal verb gibi ifadeler varsa phrase adayıdır.
    /// </summary>
    private static bool IsPhraseCandidate(
        JsonElement root,
        string sourceText)
    {
        if (sourceText.Contains(' ', StringComparison.Ordinal))
        {
            return true;
        }

        return HasPhraseLikeValue(root, "tags") ||
               HasPhraseLikeValue(root, "categories");
    }

    /// <summary>
    /// tags/categories gibi array alanlarında phrase benzeri değer var mı kontrol eder.
    /// 
    /// Kaikki çıktılarında bazı array'ler string,
    /// bazıları object olabilir.
    /// Bu yüzden iki ihtimali de destekliyoruz.
    /// </summary>
    private static bool HasPhraseLikeValue(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var arrayElement) ||
            arrayElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var itemElement in arrayElement.EnumerateArray())
        {
            string? value = itemElement.ValueKind switch
            {
                JsonValueKind.String => itemElement.GetString(),
                JsonValueKind.Object => GetStringProperty(itemElement, "name") ??
                                        GetStringProperty(itemElement, "kind") ??
                                        GetStringProperty(itemElement, "category"),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var normalizedValue = NormalizeText(value);

            if (normalizedValue.Contains("phrase", StringComparison.Ordinal) ||
                normalizedValue.Contains("idiom", StringComparison.Ordinal) ||
                normalizedValue.Contains("proverb", StringComparison.Ordinal) ||
                normalizedValue.Contains("phrasalverb", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
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
    /// kaikki:line-123:translation-2:abandon
    /// </summary>
    private static string BuildExternalSourceKey(
        int lineNumber,
        int translationIndex,
        string normalizedSourceText)
    {
        var sourceKey =
            $"kaikki:line-{lineNumber}:translation-{translationIndex}:{normalizedSourceText}";

        if (sourceKey.Length <= ImportConstants.MaxExternalSourceKeyLength)
        {
            return sourceKey;
        }

        return sourceKey[..ImportConstants.MaxExternalSourceKeyLength];
    }

    /// <summary>
    /// Dil kodunu normalize eder.
    /// 
    /// Örnek:
    /// " TR " => "tr"
    /// </summary>
    private static string NormalizeLanguageCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Kullanıcıya gösterilecek metni trimler.
    /// 
    /// Burada lowercase yapmıyoruz.
    /// Çünkü MeaningText kullanıcıya gösterilecek gerçek metindir.
    /// </summary>
    private static string NormalizeDisplayText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    /// <summary>
    /// Duplicate kontrolü ve eşleştirme için metni normalize eder.
    /// </summary>
    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Opsiyonel metni trimler.
    /// Boşsa null döner.
    /// </summary>
    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}