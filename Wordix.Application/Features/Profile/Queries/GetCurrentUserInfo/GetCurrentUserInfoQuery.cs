using MediatR;
using Wordix.Application.Features.Profile.Responses;

namespace Wordix.Application.Features.Profile.Queries.GetCurrentUserInfo;

/// <summary>
/// O anki authenticated kullanıcının token bilgilerini isteyen query modelidir.
/// 
/// Query nedir?
/// - Sistemde veri değiştirmeyen okuma isteğidir.
/// - Burada amaç mevcut kullanıcının token üzerinden okunabilen bilgilerini döndürmektir.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcı kimliği Keycloak tarafından yönetilir.
/// - Bu query database'e gitmeden ICurrentUserService üzerinden current user bilgisini döndürür.
/// 
/// Bu query neden var?
/// - Mobil uygulama "ben kimim?" bilgisini alabilir.
/// - Swagger/Postman testlerinde token claimlerinin doğru okunduğu görülebilir.
/// - Role, email, username gibi bilgiler frontend tarafına standart response ile dönebilir.
/// </summary>
public sealed record GetCurrentUserInfoQuery
    : IRequest<CurrentUserInfoResponse>;