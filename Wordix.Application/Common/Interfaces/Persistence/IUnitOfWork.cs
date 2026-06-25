namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Unit of Work interface'i, bir use-case içindeki database değişikliklerini
/// tek bir işlem gibi yönetmek için kullanılır.
/// 
/// Repository'ler entity ekler/günceller/siler.
/// UnitOfWork ise bu değişiklikleri database'e ne zaman yazacağımıza karar verir.
/// 
/// Örnek lookup akışı:
/// - LearningItem ekle
/// - Word ekle
/// - Meaning ekle
/// - LookupHistory ekle
/// - UnitOfWork.SaveChangesAsync çağır
/// 
/// Böylece her repository kendi başına SaveChanges çağırmaz.
/// Tüm değişiklikler use-case sonunda tek seferde kaydedilir.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// DbContext tarafından takip edilen tüm değişiklikleri database'e kaydeder.
    /// 
    /// Repository Add/Update/Remove çağrıları genelde sadece entity'yi tracking sistemine ekler.
    /// Asıl INSERT/UPDATE/DELETE SQL komutları SaveChangesAsync ile çalışır.
    /// </summary>
    /// <returns>
    /// Database'de etkilenen kayıt sayısını döner.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicit transaction başlatır.
    /// 
    /// Her use-case için transaction açmak zorunda değiliz.
    /// EF Core SaveChanges zaten tek çağrı içindeki işlemleri transaction içinde yürütür.
    /// 
    /// Ama bazı karmaşık akışlarda birden fazla SaveChanges veya daha kontrollü işlem gerekiyorsa
    /// transaction başlatmak faydalı olur.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Başlatılmış transaction'ı onaylar.
    /// 
    /// Yani yapılan işlemler kalıcı hale gelir.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Başlatılmış transaction'ı geri alır.
    /// 
    /// Bir hata oluştuğunda database değişikliklerinin iptal edilmesi için kullanılır.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}