namespace Wordix.Application.Features.Lookups.Requests;

/// <summary>
/// Lookup endpointine dışarıdan gönderilecek request modelidir.
/// 
/// Bu model API request body contract'ını temsil eder.
/// Yani kullanıcı Swagger/Postman/frontend üzerinden kelime ararken bu modeldeki alanları gönderir.
/// 
/// Örnek JSON:
/// {
///   "text": "achieve",
///   "sourceLanguageCode": "en",
///   "targetLanguageCode": "tr"
/// }
/// </summary>
public sealed class LookupRequest
{
    /// <summary>
    /// Kullanıcının aramak istediği ham metindir.
    /// 
    /// Örnekler:
    /// - achieve
    /// - improve
    /// - give up
    /// - I want to improve my English.
    /// 
    /// Bu değer controller'da doğrudan işlenmeyecek.
    /// Controller bu değeri CreateLookupCommand içine aktaracak.
    /// Normalize etme işi ise ayrı servis tarafından yapılacak.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Aranan metnin kaynak dil kodudur.
    /// 
    /// İlk prototipte varsayılan olarak İngilizce kullanıyoruz.
    /// Örnek:
    /// en
    /// 
    /// İleride bu değer kullanıcı profilindeki hedef/ana dil ayarlarından da türetilebilir.
    /// </summary>
    public string SourceLanguageCode { get; init; } = "en";

    /// <summary>
    /// Kullanıcının anlamını görmek istediği hedef dil kodudur.
    /// 
    /// İlk prototipte varsayılan olarak Türkçe kullanıyoruz.
    /// Örnek:
    /// tr
    /// </summary>
    public string TargetLanguageCode { get; init; } = "tr";
}