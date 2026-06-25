using Microsoft.EntityFrameworkCore.Storage;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.UnitOfWork;

/// <summary>
/// EF Core tabanlı Unit of Work implementasyonudur.
/// 
/// Bu sınıfın görevi:
/// - DbContext tarafından takip edilen değişiklikleri database'e kaydetmek.
/// - Gerekirse explicit transaction başlatmak.
/// - Transaction'ı commit veya rollback etmek.
/// 
/// Repository'ler entity ekleme/güncelleme/silme işlemlerini hazırlar.
/// UnitOfWork ise bu değişikliklerin ne zaman database'e yazılacağını yönetir.
/// </summary>
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    /// <summary>
    /// Wordix uygulamasının EF Core DbContext nesnesidir.
    /// 
    /// DbContext aynı zamanda EF Core'un kendi Unit of Work mekanizması gibi çalışır.
    /// Çünkü değişiklikleri takip eder ve SaveChangesAsync ile tek seferde database'e yazar.
    /// 
    /// Biz burada DbContext'i doğrudan Application katmanına açmamak için
    /// kendi IUnitOfWork abstraction'ımız arkasına saklıyoruz.
    /// </summary>
    private readonly WordixDbContext _dbContext;

    /// <summary>
    /// Aktif explicit transaction bilgisidir.
    /// 
    /// Null ise şu anda manuel başlatılmış transaction yok demektir.
    /// </summary>
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// UnitOfWork oluşturulurken WordixDbContext dependency injection ile gelir.
    /// 
    /// Repository sınıfları da aynı request scope içinde aynı DbContext instance'ını kullanır.
    /// Böylece farklı repository'lerde yapılan değişiklikler aynı SaveChangesAsync çağrısında
    /// birlikte database'e yazılabilir.
    /// </summary>
    public UnitOfWork(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// DbContext tarafından takip edilen tüm değişiklikleri database'e kaydeder.
    /// 
    /// Örnek:
    /// - AddAsync ile eklenmiş entity'ler INSERT olur.
    /// - Değiştirilmiş tracked entity'ler UPDATE olur.
    /// - Remove ile işaretlenmiş entity'ler DELETE olur.
    /// 
    /// Bu method repository içinde değil, UnitOfWork içinde yer alır.
    /// Çünkü bir use-case sonunda tüm değişiklikleri tek noktadan kaydetmek istiyoruz.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Explicit transaction başlatır.
    /// 
    /// Normalde tek SaveChangesAsync çağrısı için EF Core zaten transaction kullanır.
    /// Ancak birden fazla SaveChanges veya daha karmaşık akışlarda transaction'ı bizim
    /// yönetmemiz gerekebilir.
    /// 
    /// Eğer zaten aktif bir transaction varsa tekrar transaction başlatmıyoruz.
    /// Çünkü nested transaction yönetimi bu aşamada desteklenmeyecek.
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException("Zaten aktif bir transaction var. Yeni transaction başlatılamaz.");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Aktif transaction'ı commit eder.
    /// 
    /// Commit, transaction kapsamındaki işlemleri kalıcı hale getirir.
    /// 
    /// Önemli:
    /// Bu method SaveChangesAsync çağırmaz.
    /// SaveChangesAsync'i handler/use-case içinde açık şekilde çağıracağız.
    /// Böylece kodda şu ayrım net kalır:
    /// - SaveChangesAsync: değişiklikleri database'e gönderir.
    /// - CommitTransactionAsync: transaction'ı onaylar.
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            throw new InvalidOperationException("Commit edilecek aktif bir transaction bulunamadı.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    /// <summary>
    /// Aktif transaction'ı geri alır.
    /// 
    /// Bir hata oluştuğunda transaction kapsamındaki işlemlerin kalıcı olmaması için kullanılır.
    /// 
    /// Örneğin:
    /// QuizAnswer kaydedilirken progress update sırasında hata olursa,
    /// yapılan değişiklikler rollback ile geri alınabilir.
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return;
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeCurrentTransactionAsync();
        }
    }

    /// <summary>
    /// UnitOfWork dispose edilirken aktif transaction kaldıysa temizler.
    /// 
    /// Normalde commit veya rollback sonrası transaction dispose edilir.
    /// Ama beklenmeyen bir durumda açık transaction kalırsa kaynak sızıntısı olmasın diye
    /// burada da güvenli temizlik yapıyoruz.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeCurrentTransactionAsync();
    }

    /// <summary>
    /// Aktif transaction nesnesini dispose edip null'a çeker.
    /// 
    /// Bu method private tutuldu çünkü transaction yaşam döngüsünü dışarıya açmak istemiyoruz.
    /// UnitOfWork kendi iç durumunu kendisi yönetir.
    /// </summary>
    private async Task DisposeCurrentTransactionAsync()
    {
        if (_currentTransaction is null)
        {
            return;
        }

        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}