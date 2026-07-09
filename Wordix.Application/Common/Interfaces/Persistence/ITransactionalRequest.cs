namespace Wordix.Application.Common.Interfaces.Persistence;

/// <summary>
/// Database state değiştiren MediatR requestlerini işaretlemek için kullanılan marker interface'tir.
/// 
/// Marker interface ne demek?
/// - İçinde method/property bulunmaz.
/// - Sadece "bu request özel bir davranışa tabi olsun" demek için kullanılır.
/// 
/// Bu interface'i implement eden commandlerde:
/// - Handler kendi işini yapar.
/// - Handler başarılı biterse UnitOfWorkBehavior otomatik SaveChangesAsync çağırır.
/// 
/// Query requestleri bu interface'i implement etmemelidir.
/// Çünkü query'ler sadece veri okur, database state değiştirmez.
/// </summary>
public interface ITransactionalRequest
{
}