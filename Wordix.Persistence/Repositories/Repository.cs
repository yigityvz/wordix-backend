using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Common;
using Wordix.Persistence.Contexts;

namespace Wordix.Persistence.Repositories;

/// <summary>
/// Generic repository implementasyonudur.
/// 
/// Bu sınıf, Application katmanında tanımlanan IRepository<TEntity> sözleşmesini
/// EF Core kullanarak gerçekler.
/// 
/// Amaç:
/// - Application katmanını DbContext ve DbSet detaylarından uzak tutmak.
/// - Ortak CRUD operasyonlarını tek yerde toplamak.
/// - Her entity için tekrar tekrar aynı repository kodunu yazmayı engellemek.
/// 
/// Örnek kullanım:
/// IRepository<Language> ile Languages tablosuna erişilebilir.
/// IRepository<UserProfile> ile UserProfiles tablosuna erişilebilir.
/// IRepository<QuizSession> ile QuizSessions tablosuna erişilebilir.
/// </summary>
/// <typeparam name="TEntity">
/// BaseEntity'den türeyen herhangi bir domain entity'si.
/// </typeparam>
public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Wordix uygulamasının EF Core DbContext nesnesidir.
    /// 
    /// DbContext:
    /// - Veritabanı bağlantısını yönetir.
    /// - Entity tracking yapar.
    /// - Sorguları SQL'e çevirir.
    /// - SaveChanges çağrıldığında değişiklikleri database'e gönderir.
    /// </summary>
    private readonly WordixDbContext _dbContext;

    /// <summary>
    /// TEntity tipine karşılık gelen DbSet nesnesidir.
    /// 
    /// Örnek:
    /// TEntity = Language ise _dbSet, Languages tablosunu temsil eder.
    /// TEntity = Word ise _dbSet, Words tablosunu temsil eder.
    /// </summary>
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Repository oluşturulurken DbContext dependency injection ile gelir.
    /// 
    /// Aynı request içinde aynı DbContext instance'ı kullanılacağı için,
    /// farklı repository'ler aynı Unit of Work kapsamında birlikte çalışabilir.
    /// </summary>
    public Repository(WordixDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        // DbContext üzerinden ilgili entity tipinin DbSet'ini alıyoruz.
        // Böylece generic repository her entity için çalışabilir.
        _dbSet = _dbContext.Set<TEntity>();
    }

    /// <summary>
    /// Id değerine göre tek bir entity getirir.
    /// 
    /// Burada FirstOrDefaultAsync kullanıyoruz çünkü BaseEntity.Id ortak property'dir.
    /// Entity bulunamazsa null döner.
    /// 
    /// Bu method tracking açık şekilde çalışır.
    /// Yani dönen entity üzerinde domain methodlarıyla değişiklik yapılırsa,
    /// SaveChangesAsync çağrıldığında EF Core değişikliği database'e yazar.
    /// </summary>
    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        return await _dbSet
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    /// <summary>
    /// Verilen şarta uyan ilk entity'yi getirir.
    /// 
    /// Örnek:
    /// await repository.FirstOrDefaultAsync(x => x.Code == "en");
    /// 
    /// Expression kullanıldığı için EF Core bu şartı SQL WHERE koşuluna çevirebilir.
    /// </summary>
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return await _dbSet
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Verilen şarta uyan kayıt var mı kontrol eder.
    /// 
    /// AnyAsync, tüm kayıtları çekmez.
    /// SQL tarafında EXISTS benzeri verimli bir sorgu üretir.
    /// Bu yüzden var/yok kontrolünde liste çekmekten daha doğrudur.
    /// </summary>
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return await _dbSet
            .AnyAsync(predicate, cancellationToken);
    }




    /// <summary>
    /// Entity listesini getirir.
    /// 
    /// Predicate null gelirse tüm kayıtları listeler.
    /// Predicate verilirse sadece şarta uyan kayıtları döner.
    /// 
    /// İlk prototipte sade tutuyoruz.
    /// Sayfalama, sıralama ve include gibi ihtiyaçları ileride özel repository methodlarında çözeceğiz.
    /// </summary>
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbSet;

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Yeni entity'yi DbContext tracking sistemine ekler.
    /// 
    /// Dikkat:
    /// Bu method çağrıldığında database'e hemen INSERT atılmaz.
    /// Sadece EF Core'a "bu entity eklenecek" denir.
    /// Gerçek INSERT, UnitOfWork.SaveChangesAsync çağrıldığında çalışır.
    /// </summary>
    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Birden fazla entity'yi DbContext tracking sistemine ekler.
    /// 
    /// Örnek:
    /// Bir kelime için birden fazla Meaning eklemek.
    /// Bir quiz için birden fazla QuizOption eklemek.
    /// 
    /// Yine database'e kayıt hemen gitmez.
    /// SaveChangesAsync çağrılınca gider.
    /// </summary>
    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Var olan entity'nin güncellendiğini EF Core'a bildirir.
    /// 
    /// Eğer entity zaten DbContext tarafından takip ediliyorsa çoğu zaman bu methoda gerek kalmaz.
    /// Örneğin:
    /// var profile = await repository.GetByIdAsync(id);
    /// profile.ChangeDisplayName("Yiğit");
    /// await unitOfWork.SaveChangesAsync();
    /// 
    /// Bu senaryoda EF Core değişikliği zaten takip eder.
    /// 
    /// Ama detached entity senaryolarında Update kullanılabilir.
    /// </summary>
    public void Update(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dbSet.Update(entity);
    }

    /// <summary>
    /// Entity'yi fiziksel silme için işaretler.
    /// 
    /// Dikkat:
    /// Wordix'te çoğu ana veride fiziksel silme yerine:
    /// - IsActive = false
    /// - Soft delete
    /// gibi yaklaşımlar kullanacağız.
    /// 
    /// Bu method daha çok gerçekten silinmesi güvenli olan detay kayıtları için düşünülmelidir.
    /// </summary>
    public void Remove(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        _dbSet.Remove(entity);
    }
}