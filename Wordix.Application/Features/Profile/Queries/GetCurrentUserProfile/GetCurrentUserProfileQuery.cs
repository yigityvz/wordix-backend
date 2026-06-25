using MediatR;
using Wordix.Application.Features.Profile.Responses;

namespace Wordix.Application.Features.Profile.Queries.GetCurrentUserProfile;

/// <summary>
/// O anki authenticated kullanıcının Wordix profilini isteyen query modelidir.
/// 
/// Query nedir?
/// - Sistemde veri değiştirmeyen okuma isteğidir.
/// - Burada amaç mevcut kullanıcının profil bilgisini döndürmektir.
/// 
/// Not:
/// Bu query teknik olarak UserProfile yoksa oluşturma akışını tetikliyor.
/// Yani "get or create" davranışı var.
/// Ancak dışarıdan bakıldığında endpoint'in amacı kullanıcının profilini okumaktır.
/// Bu yüzden bunu Query olarak modelledik.
/// </summary>
public sealed record GetCurrentUserProfileQuery
    : IRequest<CurrentUserProfileResponse>;