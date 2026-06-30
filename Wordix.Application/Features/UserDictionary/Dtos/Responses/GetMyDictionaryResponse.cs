namespace Wordix.Application.Features.UserDictionary.Dtos.Responses;

/// <summary>
/// Kullanıcının kendi dictionary listesini dönen response modelidir.
/// 
/// GET /api/user-dictionary endpointi bu modeli dönecek.
/// </summary>
public sealed class GetMyDictionaryResponse
{
    /// <summary>
    /// Kullanıcının dictionary'sindeki toplam item sayısıdır.
    /// 
    /// İlk prototipte pagination yapmıyoruz.
    /// Yine de frontend'in liste sayısını gösterebilmesi için total count dönüyoruz.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Dictionary item listesi.
    /// </summary>
    public IReadOnlyCollection<UserDictionaryItemResponse> Items { get; init; }
        = Array.Empty<UserDictionaryItemResponse>();
}
