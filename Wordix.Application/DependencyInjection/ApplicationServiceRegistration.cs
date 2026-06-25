using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Behaviors;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Services;

namespace Wordix.Application.DependencyInjection;

/// <summary>
/// Application katmanındaki servislerin DI kayıtlarını yapan extension class.
/// 
/// Neden var?
/// - Program.cs sade kalsın.
/// - Application servisleri tek bir merkezi yerde register edilsin.
/// - MediatR, FluentValidation, pipeline behavior ve application service kayıtları
///   burada yönetilsin.
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Wordix Application servislerini DI container'a ekler.
    /// 
    /// Bu method Wordix.Api tarafındaki ApiServiceRegistration içinden çağrılır.
    /// Böylece Program.cs içinde tek tek MediatR, service, validator kaydı yapmak zorunda kalmayız.
    /// </summary>
    public static IServiceCollection AddWordixApplication(this IServiceCollection services)
    {
        // MediatR registration:
        // Application assembly içindeki IRequest, IRequestHandler, INotificationHandler gibi yapıları tarar.
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
        });

        // FluentValidation registration:
        // Application assembly içindeki AbstractValidator<T> implementasyonlarını tarar ve DI'a ekler.
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly);

        // MediatR pipeline behavior registration:
        //
        // Pipeline sırası registration sırasına göre çalışır:
        // 1. LoggingBehavior
        // 2. ValidationBehavior
        // 3. Handler
        //
        // Böylece loglama request'in validation dahil tüm süresini ölçer.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // UserProfileSyncService application seviyesinde bir workflow servisidir.
        // Current token kullanıcısını Wordix UserProfile kaydıyla eşleştirir.
        services.AddScoped<IUserProfileSyncService, UserProfileSyncService>();

        return services;
    }
}