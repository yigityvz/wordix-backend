using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Translation;
using Wordix.Application.Common.Models.Import;
using Wordix.Application.Common.Models.Translation;
using Wordix.Application.Features.Imports.Services;
using Wordix.Domain.Enums;
using Wordix.Infrastructure.Options;

namespace Wordix.Infrastructure.Providers.Translation;

/// <summary>
/// Azure Translator REST API üzerinden metin çevirisi yapan provider implementasyonudur.
/// 
/// Bu class neden Infrastructure katmanında?
/// - HTTP isteği atar.
/// - Azure endpoint, header ve response formatını bilir.
/// - Bunlar Application katmanının bilmemesi gereken teknik detaylardır.
/// 
/// 24K güncellemesi:
/// - Önce ExternalContentCache kontrol eder.
/// - Cache varsa Azure'a gitmeden result döner.
/// - Cache hit/success/failure durumlarını ProviderRequestLog tablosuna yazar.
/// - Azure başarılı dönerse sonucu cache'e kaydeder.
/// </summary>
public sealed class AzureTranslatorProvider : ITranslationProvider
{
    private const string SubscriptionKeyHeaderName = "Ocp-Apim-Subscription-Key";
    private const string SubscriptionRegionHeaderName = "Ocp-Apim-Subscription-Region";

    private const string OperationName = "Translate";

    /// <summary>
    /// Azure translation sonuçları genelde kısa vadede değişmez.
    /// İlk aşamada 30 günlük cache yeterli ve güvenli bir varsayımdır.
    /// İleride bunu AzureTranslatorOptions içine configurable yapabiliriz.
    /// </summary>
    private static readonly TimeSpan DefaultCacheLifetime = TimeSpan.FromDays(30);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AzureTranslatorOptions _options;
    private readonly IExternalContentCacheService _externalContentCacheService;
    private readonly IProviderRequestLogService _providerRequestLogService;

    public AzureTranslatorProvider(
        HttpClient httpClient,
        IOptions<AzureTranslatorOptions> options,
        IExternalContentCacheService externalContentCacheService,
        IProviderRequestLogService providerRequestLogService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _externalContentCacheService = externalContentCacheService;
        _providerRequestLogService = providerRequestLogService;
    }

    /// <summary>
    /// Bu provider'ın ürettiği içeriğin gerçek kaynağıdır.
    /// </summary>
    public ContentSource ContentSource => ContentSource.AzureTranslator;

    /// <summary>
    /// Provider'ın okunabilir adıdır.
    /// Meaning.SourceProvider veya log kayıtlarında kullanılabilir.
    /// </summary>
    public string ProviderName => ImportConstants.ProviderNames.AzureTranslator;

    /// <summary>
    /// Azure Translator REST API ile tek metin çevirisi yapar.
    /// 
    /// Akış:
    /// 1. Input normalize edilir.
    /// 2. Cache key üretilir.
    /// 3. Usable cache varsa Azure'a gidilmeden result döner.
    /// 4. Cache yoksa Azure çağrısı yapılır.
    /// 5. Başarılı Azure sonucu cache'e yazılır.
    /// 6. Her durumda provider request log best-effort yazılır.
    /// </summary>
    public async Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var sourceText = NormalizeDisplayText(request.Text);
        var sourceLanguageCode = NormalizeLanguageCode(request.SourceLanguageCode);
        var targetLanguageCode = NormalizeLanguageCode(request.TargetLanguageCode);

        var cacheKey = BuildCacheKey(
            sourceText,
            sourceLanguageCode,
            targetLanguageCode);

        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "EMPTY_SOURCE_TEXT",
                errorMessage: "Çevrilecek metin boş olamaz.",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(sourceLanguageCode))
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "EMPTY_SOURCE_LANGUAGE",
                errorMessage: "Kaynak dil kodu boş olamaz.",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "EMPTY_TARGET_LANGUAGE",
                errorMessage: "Hedef dil kodu boş olamaz.",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }

        // Önce cache kontrolü yapıyoruz.
        // Cache hit varsa Azure'a hiç gitmeden result döner.
        var cachedResult = await TryGetCachedTranslationResultAsync(
            cacheKey,
            sourceText,
            sourceLanguageCode,
            targetLanguageCode,
            stopwatch,
            cancellationToken);

        if (cachedResult is not null)
        {
            return cachedResult;
        }

        if (!_options.IsEnabled)
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "PROVIDER_DISABLED",
                errorMessage: "Azure Translator provider devre dışı.",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(_options.SubscriptionKey))
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "MISSING_SUBSCRIPTION_KEY",
                errorMessage: "Azure Translator subscription key tanımlı değil.",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }

        var requestUri = BuildTranslateUri(
            _options.Endpoint,
            sourceLanguageCode,
            targetLanguageCode);

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            requestUri);

        httpRequest.Headers.Add(
            SubscriptionKeyHeaderName,
            _options.SubscriptionKey.Trim());

        if (!string.IsNullOrWhiteSpace(_options.Region))
        {
            httpRequest.Headers.Add(
                SubscriptionRegionHeaderName,
                _options.Region.Trim());
        }

        var requestBody = new[]
        {
            new AzureTranslateRequestItem
            {
                Text = sourceText
            }
        };

        var requestJson = JsonSerializer.Serialize(
            requestBody,
            JsonOptions);

        httpRequest.Content = new StringContent(
            requestJson,
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = BuildHttpErrorMessage(
                    response.StatusCode,
                    responseContent);

                var providerStatus = MapHttpStatusToProviderStatus(response.StatusCode);
                var errorCode = BuildHttpErrorCode(response.StatusCode);

                return await CreateFailureResultWithLogAsync(
                    sourceText,
                    sourceLanguageCode,
                    targetLanguageCode,
                    cacheKey,
                    errorCode,
                    errorMessage,
                    providerStatus,
                    stopwatch,
                    cancellationToken,
                    httpStatusCode: (int)response.StatusCode);
            }

            var azureResponse = JsonSerializer.Deserialize<IReadOnlyCollection<AzureTranslateResponseItem>>(
                responseContent,
                JsonOptions);

            var translatedText = azureResponse?
                .FirstOrDefault()?
                .Translations
                .FirstOrDefault()?
                .Text;

            translatedText = NormalizeDisplayText(translatedText);

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                return await CreateFailureResultWithLogAsync(
                    sourceText,
                    sourceLanguageCode,
                    targetLanguageCode,
                    cacheKey,
                    errorCode: "EMPTY_TRANSLATED_TEXT",
                    errorMessage: "Azure Translator başarılı döndü fakat translated text boş geldi.",
                    status: ProviderRequestStatus.Failed,
                    stopwatch,
                    cancellationToken,
                    httpStatusCode: (int)response.StatusCode);
            }

            var result = TranslationResult.Success(
                sourceText,
                translatedText,
                sourceLanguageCode,
                targetLanguageCode,
                ContentSource,
                ProviderName,
                ContentQualityStatus.AutoGenerated);

            // Azure başarılı döndüyse sonucu cache'e yazıyoruz.
            // Cache yazma best-effort çalışır; cache yazılamazsa çeviri sonucunu bozmayız.
            await TrySetCacheAsync(
                cacheKey,
                result,
                cancellationToken);

            await TryCreateProviderLogAsync(
                status: ProviderRequestStatus.Succeeded,
                requestKey: cacheKey,
                normalizedInput: sourceText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                wasServedFromCache: false,
                durationMs: ToDurationMs(stopwatch),
                httpStatusCode: (int)response.StatusCode,
                errorCode: null,
                errorMessage: null,
                cancellationToken);

            return result;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "TIMEOUT",
                errorMessage: "Azure Translator isteği timeout oldu.",
                status: ProviderRequestStatus.Timeout,
                stopwatch,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "HTTP_REQUEST_FAILED",
                errorMessage: $"Azure Translator HTTP isteği başarısız oldu: {exception.Message}",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }
        catch (JsonException exception)
        {
            return await CreateFailureResultWithLogAsync(
                sourceText,
                sourceLanguageCode,
                targetLanguageCode,
                cacheKey,
                errorCode: "RESPONSE_PARSE_FAILED",
                errorMessage: $"Azure Translator response parse edilemedi: {exception.Message}",
                status: ProviderRequestStatus.Failed,
                stopwatch,
                cancellationToken);
        }
    }

    /// <summary>
    /// Cache'de kullanılabilir translation sonucu varsa onu TranslationResult'a çevirir.
    /// 
    /// Burada dikkat:
    /// - Cache payload bozuksa kullanıcıya hata döndürmüyoruz.
    /// - Cache bozuksa Azure'a gerçek çağrı yapılmasına izin veriyoruz.
    /// - Cache hit başarılıysa ProviderRequestLog = ServedFromCache yazıyoruz.
    /// </summary>
    private async Task<TranslationResult?> TryGetCachedTranslationResultAsync(
        string cacheKey,
        string sourceText,
        string sourceLanguageCode,
        string targetLanguageCode,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        try
        {
            var cache = await _externalContentCacheService.GetUsableByKeyAsync(
                cacheKey,
                cancellationToken);

            if (cache is null)
            {
                return null;
            }

            var payload = JsonSerializer.Deserialize<AzureTranslationCachePayload>(
                cache.CachedPayloadJson,
                JsonOptions);

            if (payload is null ||
                string.IsNullOrWhiteSpace(payload.TranslatedText))
            {
                return null;
            }

            var cachedResult = TranslationResult.Success(
                sourceText: string.IsNullOrWhiteSpace(payload.SourceText)
                    ? sourceText
                    : payload.SourceText,
                translatedText: payload.TranslatedText,
                sourceLanguageCode: string.IsNullOrWhiteSpace(payload.SourceLanguageCode)
                    ? sourceLanguageCode
                    : payload.SourceLanguageCode,
                targetLanguageCode: string.IsNullOrWhiteSpace(payload.TargetLanguageCode)
                    ? targetLanguageCode
                    : payload.TargetLanguageCode,
                contentSource: ContentSource,
                sourceProvider: ProviderName,
                qualityStatus: ContentQualityStatus.AutoGenerated);

            await TryCreateProviderLogAsync(
                status: ProviderRequestStatus.ServedFromCache,
                requestKey: cacheKey,
                normalizedInput: sourceText,
                sourceLanguageCode: sourceLanguageCode,
                targetLanguageCode: targetLanguageCode,
                wasServedFromCache: true,
                durationMs: ToDurationMs(stopwatch),
                httpStatusCode: null,
                errorCode: null,
                errorMessage: null,
                cancellationToken);

            return cachedResult;
        }
        catch
        {
            // Cache/log altyapısındaki hata Azure çeviri akışını bozmamalı.
            // Cache okunamazsa gerçek provider çağrısına devam ederiz.
            return null;
        }
    }

    /// <summary>
    /// Başarılı Azure sonucunu ExternalContentCache tablosuna yazar.
    /// </summary>
    private async Task TrySetCacheAsync(
        string cacheKey,
        TranslationResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = new AzureTranslationCachePayload
            {
                SourceText = result.SourceText,
                TranslatedText = result.TranslatedText,
                SourceLanguageCode = result.SourceLanguageCode,
                TargetLanguageCode = result.TargetLanguageCode,
                SourceProvider = result.SourceProvider
            };

            var payloadJson = JsonSerializer.Serialize(
                payload,
                JsonOptions);

            await _externalContentCacheService.SetAsync(
                new ExternalContentCacheSetRequest
                {
                    ProviderType = ProviderType.Translation,
                    ProviderName = ProviderName,
                    OperationName = OperationName,
                    CacheKey = cacheKey,
                    CachedPayloadJson = payloadJson,
                    NormalizedInput = LimitLength(
                        NormalizeForStorage(result.SourceText),
                        maxLength: 500),
                    SourceLanguageCode = result.SourceLanguageCode,
                    TargetLanguageCode = result.TargetLanguageCode,
                    ContentSource = ContentSource.AzureTranslator,
                    QualityStatus = ContentQualityStatus.AutoGenerated,
                    ExpiresAtUtc = DateTime.UtcNow.Add(DefaultCacheLifetime)
                },
                cancellationToken);
        }
        catch
        {
            // Cache yazma başarısız olsa bile çeviri başarılıysa üst katmana success dönmeliyiz.
            // Bu yüzden burada exception yutmamız bilinçli.
        }
    }

    /// <summary>
    /// Provider log kaydını best-effort oluşturur.
    /// 
    /// Neden best-effort?
    /// - Log yazılamadığı için kullanıcı lookup akışının bozulmasını istemiyoruz.
    /// - Asıl işlem translation result üretmektir.
    /// - Log hataları ileride merkezi logger ile ayrıca ele alınabilir.
    /// </summary>
    private async Task TryCreateProviderLogAsync(
        ProviderRequestStatus status,
        string requestKey,
        string? normalizedInput,
        string sourceLanguageCode,
        string targetLanguageCode,
        bool wasServedFromCache,
        int? durationMs,
        int? httpStatusCode,
        string? errorCode,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            await _providerRequestLogService.CreateAsync(
                new ProviderRequestLogCreateRequest
                {
                    ProviderType = ProviderType.Translation,
                    ProviderName = ProviderName,
                    OperationName = OperationName,
                    Status = status,
                    RequestKey = requestKey,
                    NormalizedInput = LimitLength(
                        NormalizeForStorage(normalizedInput),
                        maxLength: 500),
                    SourceLanguageCode = sourceLanguageCode,
                    TargetLanguageCode = targetLanguageCode,
                    WasServedFromCache = wasServedFromCache,
                    DurationMs = durationMs,
                    HttpStatusCode = httpStatusCode,
                    ErrorCode = errorCode,
                    ErrorMessage = LimitLength(errorMessage, maxLength: 1000)
                },
                cancellationToken);
        }
        catch
        {
            // Provider log yazma hatası translation akışını bozmamalı.
        }
    }

    /// <summary>
    /// Failure result üretir ve provider log yazar.
    /// </summary>
    private async Task<TranslationResult> CreateFailureResultWithLogAsync(
        string sourceText,
        string sourceLanguageCode,
        string targetLanguageCode,
        string cacheKey,
        string errorCode,
        string errorMessage,
        ProviderRequestStatus status,
        Stopwatch stopwatch,
        CancellationToken cancellationToken,
        int? httpStatusCode = null)
    {
        await TryCreateProviderLogAsync(
            status: status,
            requestKey: cacheKey,
            normalizedInput: sourceText,
            sourceLanguageCode: sourceLanguageCode,
            targetLanguageCode: targetLanguageCode,
            wasServedFromCache: false,
            durationMs: ToDurationMs(stopwatch),
            httpStatusCode: httpStatusCode,
            errorCode: errorCode,
            errorMessage: errorMessage,
            cancellationToken);

        return TranslationResult.Failure(
            sourceText,
            sourceLanguageCode,
            targetLanguageCode,
            ContentSource,
            ProviderName,
            errorMessage);
    }

    /// <summary>
    /// Azure translate endpoint URI'sini üretir.
    /// 
    /// Örnek:
    /// https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=en&to=tr
    /// </summary>
    private static string BuildTranslateUri(
        string endpoint,
        string sourceLanguageCode,
        string targetLanguageCode)
    {
        var normalizedEndpoint = string.IsNullOrWhiteSpace(endpoint)
            ? "https://api.cognitive.microsofttranslator.com"
            : endpoint.Trim().TrimEnd('/');

        var from = Uri.EscapeDataString(sourceLanguageCode);
        var to = Uri.EscapeDataString(targetLanguageCode);

        return $"{normalizedEndpoint}/translate?api-version=3.0&from={from}&to={to}";
    }

    /// <summary>
    /// Cache ve provider log için teknik request key üretir.
    /// 
    /// Neden hash kullanıyoruz?
    /// - Source text uzun olabilir.
    /// - CacheKey kolonumuz 500 karakter.
    /// - Aynı input için sabit ama kısa bir key üretmek istiyoruz.
    /// </summary>
    private static string BuildCacheKey(
        string sourceText,
        string sourceLanguageCode,
        string targetLanguageCode)
    {
        var normalizedInput = NormalizeForStorage(sourceText);
        var hashInput = $"{sourceLanguageCode}:{targetLanguageCode}:{normalizedInput}";

        var hashBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(hashInput));

        var hash = Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();

        return $"{ImportConstants.ProviderNames.AzureTranslator}:translate:{sourceLanguageCode}:{targetLanguageCode}:{hash}"
            .ToLowerInvariant();
    }

    /// <summary>
    /// HTTP hata mesajını güvenli ve kısa hale getirir.
    /// </summary>
    private static string BuildHttpErrorMessage(
        HttpStatusCode statusCode,
        string responseContent)
    {
        var safeResponseContent = string.IsNullOrWhiteSpace(responseContent)
            ? string.Empty
            : responseContent.Trim();

        if (safeResponseContent.Length > 500)
        {
            safeResponseContent = safeResponseContent[..500];
        }

        return $"Azure Translator başarısız döndü. StatusCode: {(int)statusCode} {statusCode}. Response: {safeResponseContent}";
    }

    /// <summary>
    /// HTTP status code değerini provider request status değerine çevirir.
    /// </summary>
    private static ProviderRequestStatus MapHttpStatusToProviderStatus(
        HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => ProviderRequestStatus.Timeout,
            (HttpStatusCode)429 => ProviderRequestStatus.RateLimited,
            _ => ProviderRequestStatus.Failed
        };
    }

    /// <summary>
    /// HTTP status code için standart hata kodu üretir.
    /// </summary>
    private static string BuildHttpErrorCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.RequestTimeout => "TIMEOUT",
            (HttpStatusCode)429 => "RATE_LIMITED",
            _ => "AZURE_TRANSLATOR_HTTP_ERROR"
        };
    }

    /// <summary>
    /// Stopwatch değerini int milliseconds formatına çevirir.
    /// </summary>
    private static int ToDurationMs(Stopwatch stopwatch)
    {
        var elapsed = stopwatch.ElapsedMilliseconds;

        return elapsed > int.MaxValue
            ? int.MaxValue
            : (int)elapsed;
    }

    /// <summary>
    /// Dil kodunu normalize eder.
    /// </summary>
    private static string NormalizeLanguageCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Kullanıcıya veya üst katmana dönecek metni trimler.
    /// Lowercase yapmaz; çeviri metninin doğal halini korur.
    /// </summary>
    private static string NormalizeDisplayText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    /// <summary>
    /// Cache/log storage için metni normalize eder.
    /// </summary>
    private static string NormalizeForStorage(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Database kolon limitlerini aşmamak için metni kısaltır.
    /// </summary>
    private static string? LimitLength(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        return trimmed.Length <= maxLength
            ? trimmed
            : trimmed[..maxLength];
    }

    /// <summary>
    /// Azure request body item modelidir.
    /// Azure translate endpoint'i JSON array içinde Text alanı bekler.
    /// </summary>
    private sealed record AzureTranslateRequestItem
    {
        [JsonPropertyName("Text")]
        public string Text { get; init; } = string.Empty;
    }

    /// <summary>
    /// Azure translate response ana item modelidir.
    /// </summary>
    private sealed record AzureTranslateResponseItem
    {
        [JsonPropertyName("translations")]
        public IReadOnlyCollection<AzureTranslationItem> Translations { get; init; }
            = Array.Empty<AzureTranslationItem>();
    }

    /// <summary>
    /// Azure response içindeki tek translation sonucudur.
    /// </summary>
    private sealed record AzureTranslationItem
    {
        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;

        [JsonPropertyName("to")]
        public string To { get; init; } = string.Empty;
    }

    /// <summary>
    /// ExternalContentCache içine yazdığımız sade Azure translation payload modelidir.
    /// 
    /// Secret bilgi içermez.
    /// API key, header, token veya connection string burada tutulmaz.
    /// </summary>
    private sealed record AzureTranslationCachePayload
    {
        public string SourceText { get; init; } = string.Empty;

        public string TranslatedText { get; init; } = string.Empty;

        public string SourceLanguageCode { get; init; } = string.Empty;

        public string TargetLanguageCode { get; init; } = string.Empty;

        public string SourceProvider { get; init; } = string.Empty;
    }
}