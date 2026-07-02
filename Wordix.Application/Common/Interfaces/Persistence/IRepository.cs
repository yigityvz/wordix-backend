using System.Linq.Expressions;
using Wordix.Domain.Common;

namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Tüm entity'ler için ortak temel repository interface'idir.
/// 
/// Bu interface'in amacı:
/// - Application katmanının EF Core DbContext'i doğrudan bilmesini engellemek.
/// - Genel veri okuma/yazma operasyonlarını standart hale getirmek.
/// - Repository implementasyonunu Persistence katmanına bırakmak.
/// 
/// TEntity generic olduğu için:
/// - Language için kullanılabilir.
/// - UserProfile için kullanılabilir.
/// - LearningItem için kullanılabilir.
/// - QuizSession için kullanılabilir.
/// </summary>
/// <typeparam name="TEntity">
/// BaseEntity'den türeyen herhangi bir domain entity'si.
/// </typeparam>
public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Id değerine göre tek bir entity getirir.
    /// 
    /// Örnek:
    /// - UserProfile Id ile kullanıcı profili çekmek.
    /// - LearningItem Id ile içerik kontrol etmek.
    /// - QuizSession Id ile quiz oturumu bulmak.
    /// 
    /// Entity bulunamazsa null döner.
    /// Böylece handler tarafında NotFound/business rule kararı verilebilir.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir şarta uyan ilk entity'yi getirir.
    /// 
    /// Örnek:
    /// - Email'e göre UserProfile bulmak.
    /// - Language.Code == "en" olan dili bulmak.
    /// - NormalizedText == "achieve" olan kelimeyi bulmak.
    /// 
    /// Burada Expression kullanmamızın sebebi:
    /// EF Core bu ifadeyi SQL sorgusuna çevirebilir.
    /// Ama Application yine de DbContext veya DbSet bilmez.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir şarta uyan kayıt var mı kontrol eder.
    /// 
    /// Örnek:
    /// - Kullanıcı aynı LearningItem'ı daha önce kaydetmiş mi?
    /// - Aynı dil kodu zaten var mı?
    /// - Aynı normalized word zaten var mı?
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);



    /// <summary>
    /// Belirli bir şarta uyan entity listesini getirir.
    /// 
    /// Örnek:
    /// - Kullanıcının dictionary kayıtlarını listelemek.
    /// - Bir LearningItem'ın anlamlarını listelemek.
    /// - Kullanıcının quiz geçmişini listelemek.
    /// 
    /// IReadOnlyList dönmemizin sebebi:
    /// Handler tarafı listeyi değiştirmesin, sadece okusun.
    /// </summary>
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir entity'yi DbContext tracking sistemine ekler.
    /// 
    /// Dikkat:
    /// Bu method tek başına database'e kayıt atmaz.
    /// Gerçek kayıt UnitOfWork.SaveChangesAsync çağrıldığında database'e gider.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Birden fazla entity'yi DbContext tracking sistemine ekler.
    /// 
    /// Örnek:
    /// - Bir kelime için birden fazla Meaning eklemek.
    /// - Quiz oluştururken birden fazla QuizQuestion/QuizOption eklemek.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Var olan bir entity'nin güncellendiğini EF Core'a bildirir.
    /// 
    /// Çoğu durumda EF Core tracking açık olduğu için bu methoda gerek kalmayabilir.
    /// Ama detached entity senaryolarında kullanılabilir.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Bir entity'yi fiziksel silmek için işaretler.
    /// 
    /// Dikkat:
    /// Wordix'te çoğu yerde fiziksel silme yerine IsActive=false veya soft delete tercih edeceğiz.
    /// Bu method daha çok gerçekten silinmesi gereken teknik/detail kayıtları için düşünülmelidir.
    /// </summary>
    void Remove(TEntity entity);
}