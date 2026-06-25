using Wordix.Api.Middlewares;

namespace Wordix.Api.DependencyInjection;

/// <summary>
/// WebApplication pipeline ayarlarını merkezi olarak topladığımız extension class.
/// 
/// Neden var?
/// - Program.cs dosyasını sade tutmak için.
/// - Middleware sırasını tek dosyada yönetmek için.
/// - Swagger, Authentication, Authorization ve Controller mapping ayarlarını
///   okunabilir şekilde bir arada tutmak için.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Wordix API request pipeline'ını yapılandırır.
    /// 
    /// Program.cs içinde sadece:
    /// app.UseWordixApiPipeline();
    /// şeklinde çağrılır.
    /// </summary>
    public static WebApplication UseWordixApiPipeline(this WebApplication app)
    {
        // ExceptionMiddleware'i pipeline'ın en başına yakın ekliyoruz.
        // Böylece alttaki middleware/controller/endpointlerde oluşan hataları
        // merkezi olarak yakalayıp standart ErrorResponse formatına çevirebilir.
        app.UseMiddleware<ExceptionMiddleware>();

        // Swagger sadece Development ortamında açık tutulur.
        // Production ortamında API dokümantasyonunu herkese açmak güvenlik açısından
        // ayrıca değerlendirilmelidir.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // HTTP isteklerini HTTPS'e yönlendirmek için kullanılır.
        // Local geliştirmede bazı durumlarda port/sertifika ayarlarına göre davranışı değişebilir.
        app.UseHttpsRedirection();

        // Authentication:
        // Kullanıcının kim olduğunu belirler.
        // JWT token geçerli mi, claimler okunabiliyor mu burada kontrol edilir.
        app.UseAuthentication();

        // Authorization:
        // Kullanıcının ilgili endpoint'e erişim yetkisi var mı kontrol eder.
        // Örnek: AdminOnly policy, BasicUserOnly policy.
        app.UseAuthorization();

        // Controller endpointlerini pipeline'a ekler.
        app.MapControllers();

        return app;
    }
}