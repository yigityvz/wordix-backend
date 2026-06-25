using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Wordix.Api.Controllers;

/// <summary>
/// Authentication ve authorization kurulumunu test etmek için oluşturulmuş geçici controller.
/// Bu controller iş mantığı içermez; sadece Keycloak JWT doğrulama ve rol kontrolünü test eder.
/// </summary>
[ApiController]
[Route("api/auth-test")]
public sealed class AuthTestController : ControllerBase // view yerine JSON response döndüreceğimiz için ControllerBase yazdık
{
    /// <summary>
    /// Public endpoint.
    /// Token gerektirmez.
    /// Herkes erişebilir.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous] // bu endpoint token istemez
    public IActionResult Public()
    {
        return Ok(new
        {
            Message = "Public endpoint çalışıyor. Bu endpoint token gerektirmez."
        });
    }

    /// <summary>
    /// Protected endpoint.
    /// Geçerli bir JWT token gerektirir.
    /// Rol fark etmeksizin giriş yapmış her kullanıcı erişebilir.
    /// </summary>
    [HttpGet("protected")] 
    [Authorize] // token ister ama rol istemez giriş yapmak yeterli
    public IActionResult Protected()
    {
        var username = User.Identity?.Name;

        var roles = User
            .FindAll(ClaimTypes.Role)
            .Select(roleClaim => roleClaim.Value)
            .ToList();

        return Ok(new
        {
            Message = "Protected endpoint çalışıyor. Geçerli token ile erişildi.",
            Username = username,
            Roles = roles
        });
    }

    /// <summary>
    /// Admin endpoint.
    /// Sadece AdminOnly policy'sini sağlayan kullanıcılar erişebilir.
    /// Bu policy Program.cs içinde admin rolü gerektirecek şekilde tanımlandı.
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]  // Sadece admin bu endpointi kullanabilir 
    public IActionResult Admin()
    {
        var username = User.Identity?.Name;  // keycloak claim'indeki preferred_username'den gelir 

        var roles = User
            .FindAll(ClaimTypes.Role)
            .Select(roleClaim => roleClaim.Value)
            .ToList();

        return Ok(new
        {
            Message = "Admin endpoint çalışıyor. Admin rolü doğrulandı.",
            Username = username,
            Roles = roles
        });
    }
}