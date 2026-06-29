using Wordix.Application.Features.Lookups.Commands.CreateLookup;
using Wordix.Application.Features.Lookups.Requests;

namespace Wordix.Application.Features.Lookups.Mappers;

/// <summary>
/// Lookup feature'ına ait DTO → Command dönüşümlerini merkezi olarak yapan mapper sınıfıdır.
/// 
/// Bu sınıf neden var?
/// - Controller içinde property property manual mapping yapmak istemiyoruz.
/// - AutoMapper/Mapster gibi otomatik mapping kütüphanesi de kullanmak istemiyoruz.
/// - Bu yüzden explicit, okunabilir ve kontrollü bir mapper kullanıyoruz.
/// 
/// Bu yaklaşım:
/// - Magic mapping değildir.
/// - Controller'ı sadeleştirir.
/// - Mapping kurallarını tek yerde toplar.
/// - İleride DTO isimleri değişirse controller değil, mapper güncellenir.
/// </summary>
public static class LookupMapper
{
    /// <summary>
    /// API request DTO'sunu CreateLookupCommand modeline dönüştürür.
    /// 
    /// Dikkat:
    /// Controller içinde null kontrolü yapmıyoruz.
    /// Eğer request null gelirse boş string değerleriyle command oluşturuyoruz.
    /// Bu sayede validation işlemi controller'da değil,
    /// ValidationBehavior + CreateLookupCommandValidator tarafında yapılır.
    /// </summary>
    public static CreateLookupCommand ToCreateLookupCommand(
        LookupRequest? request)
    {
        return new CreateLookupCommand
        {
            Text = request?.Text ?? string.Empty,
            SourceLanguageCode = request?.SourceLanguageCode ?? string.Empty,
            TargetLanguageCode = request?.TargetLanguageCode ?? string.Empty
        };
    }
}