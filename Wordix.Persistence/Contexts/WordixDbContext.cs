using Microsoft.EntityFrameworkCore;
using Wordix.Domain.Entities;
using Wordix.Persistence.SeedData;

namespace Wordix.Persistence.Contexts;

/// <summary>
/// Wordix uygulamasının EF Core DbContext sınıfıdır.
/// 
/// DbContext'in görevi:
/// - Domain entity'lerini veritabanı tabloları ile eşleştirmek.
/// - DbSet'ler üzerinden sorgu ve kayıt işlemlerini yönetmek.
/// - Entity configuration dosyalarını uygulamak.
/// - Migration üretirken EF Core'a model bilgisini vermek.
/// 
/// Bu sınıf Persistence katmanındadır.
/// Çünkü EF Core ve MSSQL detayları Domain katmanına ait değildir.
/// </summary>
public class WordixDbContext : DbContext
{
    /// <summary>
    /// DbContext ayarları dışarıdan dependency injection ile gelir.
    /// 
    /// Örneğin:
    /// - Hangi connection string kullanılacak?
    /// - SQL Server mı kullanılacak?
    /// - Logging açık mı olacak?
    /// 
    /// Bunları Program.cs içinde değil, Persistence DI registration içinde ayarlayacağız.
    /// </summary>
    public WordixDbContext(DbContextOptions<WordixDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Kullanıcının quiz, öneri, motivasyon ve uygulama tercihlerini tutar.
    /// 
    /// Önemli:
    /// UserPreference artık UserProfileId üzerinden değil,
    /// Keycloak token içindeki "sub" claiminden gelen KeycloakUserId üzerinden kullanıcıya bağlanacaktır.
    /// </summary>
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    /// <summary>
    /// Sistemde desteklenen dilleri tutar.
    /// </summary>
    public DbSet<Language> Languages => Set<Language>();

    /// <summary>
    /// Word, Phrase ve Sentence gibi öğrenilebilir içeriklerin ortak çatısıdır.
    /// </summary>
    public DbSet<LearningItem> LearningItems => Set<LearningItem>();

    /// <summary>
    /// İlk prototipte aktif kullanılacak global kelime tablosudur.
    /// </summary>
    public DbSet<Word> Words => Set<Word>();

    /// <summary>
    /// Global phrase / kalıp ifade tablosudur.
    /// 
    /// Phrase de Word gibi kullanıcıya özel değildir.
    /// Kullanıcı phrase'i kendi dictionary'sine kaydettiğinde UserLearningItem oluşur.
    /// </summary>
    public DbSet<Phrase> Phrases => Set<Phrase>();

    /// <summary>
    /// Word veya ileride Phrase anlamlarını tutar.
    /// </summary>
    public DbSet<Meaning> Meanings => Set<Meaning>();

    /// <summary>
    /// Kullanıcının lookup/search geçmişini tutar.
    /// </summary>
    public DbSet<LookupHistory> LookupHistories => Set<LookupHistory>();

    /// <summary>
    /// Kullanıcının kişisel dictionary kayıtlarını tutar.
    /// </summary>
    public DbSet<UserLearningItem> UserLearningItems => Set<UserLearningItem>();

    /// <summary>
    /// Kullanıcının bir dictionary item üzerindeki güncel öğrenme durumunu tutar.
    /// </summary>
    public DbSet<UserLearningProgress> UserLearningProgresses => Set<UserLearningProgress>();

    /// <summary>
    /// Kullanıcının başlattığı quiz oturumlarını tutar.
    /// </summary>
    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();

    /// <summary>
    /// Quiz oturumundaki soruları tutar.
    /// </summary>
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    /// <summary>
    /// Çoktan seçmeli quiz seçeneklerini tutar.
    /// </summary>
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();

    /// <summary>
    /// Kullanıcının quiz sorularına verdiği cevapları tutar.
    /// </summary>
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    /// <summary>
    /// UserLearningProgress değişim geçmişini tutar.
    /// </summary>
    public DbSet<LearningProgressHistory> LearningProgressHistories => Set<LearningProgressHistory>();

    /// <summary>
    /// EF Core model oluştururken çalışır.
    /// 
    /// Burada tek tek configuration çağırmak yerine assembly taraması yapıyoruz.
    /// Böylece Wordix.Persistence içindeki IEntityTypeConfiguration implementasyonları
    /// otomatik olarak uygulanır.
    /// 
    /// Bu yaklaşım Program.cs ve DbContext'i sade tutar.
    /// Yeni entity configuration eklediğimizde burayı değiştirmemize gerek kalmaz.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Persistence assembly'sindeki tüm IEntityTypeConfiguration<T> sınıflarını otomatik uygular.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WordixDbContext).Assembly);

        // İlk prototip için başlangıç dil, kelime ve anlam verilerini ekler.
        modelBuilder.SeedPrototypeData();
    }
}