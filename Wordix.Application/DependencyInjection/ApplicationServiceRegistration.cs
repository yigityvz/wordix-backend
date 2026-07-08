using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Behaviors;
using Wordix.Application.Features.Lookups.Services;
using Wordix.Application.Features.Quizzes.Services;
using Wordix.Application.Common.Interfaces.Import;
using Wordix.Application.Features.Imports.Services;

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

        // Meaning enrichment service kaydı.
        //
        // Bu service Kaikki/Wiktionary parser'dan gelen meaning satırlarını
        // DB'deki mevcut LearningItem/Word kayıtlarıyla eşleştirir.
        // Application katmanında kalır çünkü repository interface'leri üzerinden çalışır,
        // DbContext bilmez.
        services.AddScoped<IMeaningEnrichmentService, MeaningEnrichmentService>();

        // Provider-created phrase creation service kaydı.
        //
        // Bu service Azure gibi provider'lardan gelen phrase sonucunu
        // global LearningItem + Phrase + Meaning catalog yapısına kaydeder.
        // 24L'de lookup fallback akışına bağlanacaktır.
        services.AddScoped<IProviderPhraseCreationService, ProviderPhraseCreationService>();

        services.AddScoped<IExampleSentenceEnrichmentService, ExampleSentenceEnrichmentService>();

        services.AddScoped<IImportJobService, ImportJobService>();

        services.AddScoped<IProviderRequestLogService, ProviderRequestLogService>();

        services.AddScoped<IExternalContentCacheService, ExternalContentCacheService>();

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

        // Önemli:
        // Eski mimaride burada ICurrentUserProfileService register ediliyordu.
        // O servis token kullanıcısını Wordix UserProfile kaydıyla eşleştiriyor
        // ve gerekirse UserProfile + UserPreference oluşturuyordu.
        //
        // Yeni mimaride backend artık UserProfileId/UserId üretmez.
        // Kullanıcı sahipliği Keycloak token içindeki "sub" claiminden gelen
        // KeycloakUserId ile yapılır.
        //
        // Bu yüzden ICurrentUserProfileService registration'ı kaldırıldı.
        // Current user token bilgisi ICurrentUserService üzerinden okunur.
        // ICurrentUserService implementation'ı API/Infrastructure tarafında register edilir.

        services.AddSingleton<ITextNormalizer, TextNormalizer>();
        services.AddSingleton<ILookupClassifier, LookupClassifier>();

        // Quiz question generator registrations.
        //
        // Test Quiz:
        // - MultipleChoiceTranslationQuestionGenerator
        //
        // Writing Quiz:
        // - WrittenTranslationQuestionGenerator
        //
        // Handler doğrudan tek bir generator'a bağımlı olmaz.
        // QuizType'a göre doğru generator'ı QuizQuestionGeneratorResolver seçer.
        services.AddScoped<MultipleChoiceTranslationQuestionGenerator>();
        services.AddScoped<WrittenTranslationQuestionGenerator>();
        services.AddScoped<IQuizQuestionGeneratorResolver, QuizQuestionGeneratorResolver>();

        // System recommendation:
        // Faz 23 itibarıyla quiz içine local database'den sistem önerisi itemlar karıştırılabilir.
        // Handler recommendation algoritmasını bilmez; IQuizRecommendationService üzerinden çalışır.
        services.AddScoped<IQuizRecommendationService, QuizRecommendationService>();

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