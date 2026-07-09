using MediatR;
using Wordix.Application.Common.Interfaces.Identity;

namespace Wordix.Application.Common.Behaviors;

/// <summary>
/// Current user bilgisini MediatR request pipeline içinde çözen behavior'dır.
/// 
/// Neden var?
/// - User-owned use-case'lerde KeycloakUserId zorunludur.
/// - Önceden her handler içinde ICurrentUserService inject edilip
///   GetRequiredKeycloakUserId() çağrılıyordu.
/// - Bu tekrar eden kodu merkezi pipeline'a taşıyoruz.
/// 
/// Bu behavior ne yapar?
/// - Eğer request IRequiresCurrentUser implemente ediyorsa,
///   ICurrentUserService üzerinden KeycloakUserId değerini alır.
/// - Bu değeri request.KeycloakUserId propertysine yazar.
/// - Handler çalıştığında current user bilgisi request üzerinde hazır olur.
/// 
/// Clean Architecture açısından:
/// - Application katmanı hala HttpContext bilmez.
/// - Keycloak claim okuma detayı Infrastructure tarafındaki ICurrentUserService implementation'ında kalır.
/// - Handlerlar authentication detayından daha da bağımsız hale gelir.
/// </summary>
public sealed class CurrentUserBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public CurrentUserBehavior(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Sadece current user isteyen requestlerde çalışır.
        // Böylece public/global/admin olmayan farklı requestleri zorla kullanıcıya bağlamayız.
        if (request is IRequiresCurrentUser currentUserRequest)
        {
            // Bu method authenticated user zorunluluğunu da kontrol eder.
            // Kullanıcı token'sız veya geçersiz token ile gelmişse UnauthorizedAccessException fırlatır.
            var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

            // Client bu değeri göndermez.
            // Controller set etmez.
            // Pipeline current request scope içinde güvenli şekilde doldurur.
            currentUserRequest.KeycloakUserId = keycloakUserId;
        }

        return await next();
    }
}