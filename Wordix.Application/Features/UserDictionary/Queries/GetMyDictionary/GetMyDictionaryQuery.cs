using MediatR;
using Wordix.Application.Features.UserDictionary.Responses;

namespace Wordix.Application.Features.UserDictionary.Queries.GetMyDictionary;

/// <summary>
/// Current user'ın kendi dictionary listesini getiren query modelidir.
/// 
/// Neden query?
/// - Veri okuma işlemidir.
/// - Yeni kayıt oluşturmaz.
/// - Sistem durumunu değiştirmez.
/// 
/// Kullanıcı bilgisi request body/query string ile alınmaz.
/// Current user token üzerinden bulunur.
/// </summary>
public sealed record GetMyDictionaryQuery : IRequest<GetMyDictionaryResponse>
{
}