namespace Wordix.Application.Common.Interfaces.Import;

/// <summary>
/// FreeDict English-Turkish TEI XML sözlüğünü parse eden meaning provider sözleşmesidir.
/// 
/// Neden ayrı interface?
/// - Projede zaten IMeaningImportProvider interface'ini Kaikki provider kullanıyor.
/// - DI container'a ikinci IMeaningImportProvider eklersek mevcut Kaikki handler yanlış provider alabilir.
/// - Bu yüzden FreeDict için marker interface oluşturuyoruz.
/// 
/// Bu interface yine IMeaningImportProvider davranışını taşır.
/// Yani aynı LoadAsync modelini kullanır.
/// </summary>
public interface IFreeDictMeaningImportProvider : IMeaningImportProvider
{
}