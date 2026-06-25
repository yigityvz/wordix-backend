using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Persistence.Contexts;
using Wordix.Persistence.Repositories;
using UnitOfWorkImplementation = Wordix.Persistence.UnitOfWork.UnitOfWork;

namespace Wordix.Persistence.DependencyInjection;

/// <summary>
/// Persistence katmanına ait servis kayıtlarını merkezi olarak yapan extension sınıfıdır.
/// 
/// Bu sınıfın amacı:
/// - DbContext kaydını yapmak.
/// - Repository implementasyonlarını DI container'a eklemek.
/// - UnitOfWork implementasyonunu DI container'a eklemek.
/// 
/// Böylece Api katmanı tek satırla Persistence bağımlılıklarını sisteme dahil edebilir:
/// services.AddWordixPersistence(configuration);
/// </summary>
public static class PersistenceServiceRegistration
{
    /// <summary>
    /// Wordix.Persistence katmanının ihtiyaç duyduğu servisleri kaydeder.
    /// 
    /// Burada yapılan kayıtlar:
    /// - WordixDbContext
    /// - Generic Repository
    /// - UnitOfWork
    /// - Özel repository implementasyonları
    /// </summary>
    public static IServiceCollection AddWordixPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WordixDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "WordixDb connection string bulunamadı. appsettings.Development.json veya user-secrets kontrol edilmeli.");
        }

        // WordixDbContext'i scoped olarak DI container'a ekler.
        //
        // AddDbContext default olarak scoped lifetime kullanır.
        // Yani her HTTP request için bir DbContext instance'ı oluşturulur.
        // Aynı request içinde repository ve UnitOfWork aynı DbContext'i paylaşır.
        services.AddDbContext<WordixDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Generic repository kaydı.
        //
        // typeof(IRepository<>) açık generic interface'i temsil eder.
        // typeof(Repository<>) açık generic implementasyonu temsil eder.
        //
        // Böylece biri IRepository<Language> isterse DI otomatik olarak
        // Repository<Language> oluşturabilir.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // UnitOfWork kaydı.
        //
        // Application katmanı IUnitOfWork ister.
        // DI container ise Persistence içindeki UnitOfWork implementasyonunu verir.
        //
        // UnitOfWorkImplementation alias kullanmamızın sebebi:
        // Namespace adı da UnitOfWork olduğu için okunabilirliği artırmak.
        services.AddScoped<IUnitOfWork, UnitOfWorkImplementation>();

        // Özel repository kayıtları.
        //
        // Bu repositoryler generic CRUD'dan daha özel sorgular içerir.
        // Örneğin:
        // - LearningItem + Word + Meaning lookup sorgusu
        // - Kullanıcı dictionary kontrolü
        // - Lookup history sorguları
        // - Quiz session/question/option sorguları
        services.AddScoped<ILearningItemRepository, LearningItemRepository>();
        services.AddScoped<IUserLearningItemRepository, UserLearningItemRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();

        return services;
    }
}