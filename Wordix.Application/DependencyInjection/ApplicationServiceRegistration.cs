using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Behaviors;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Services;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Application.Features.Quizzes.Services;

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

        services.AddSingleton<ITextNormalizer, TextNormalizer>();
        services.AddSingleton<ILookupClassifier, LookupClassifier>();

        // Quiz feature servisleri:
        // İlk prototipte multiple choice translation generator kullanılır.
        services.AddScoped<IQuizQuestionGenerator, MultipleChoiceTranslationQuestionGenerator>();

        // Quiz answer evaluation:
        // Cevap değerlendirme logic'ini handler'dan ayrı tutar.
        services.AddScoped<IQuizAnswerEvaluator, QuizAnswerEvaluator>();

        // Learning score calculation:
        // Cevap sonucuna göre confidence score hesaplar.
        services.AddScoped<ILearningScoreCalculator, LearningScoreCalculator>();

        // Review schedule calculation:
        // Cevap sonucu, confidence score ve repetition level'a göre bir sonraki tekrar tarihini hesaplar.
        services.AddScoped<IReviewScheduleCalculator, ReviewScheduleCalculator>();

        // Learning progress update:
        // Cevap sonucu, score sonucu ve mevcut progress değerlerine göre yeni progress state'i hesaplar.
        services.AddScoped<ILearningProgressUpdater, LearningProgressUpdater>();

        return services;
    }
}