namespace Wordix.Domain.Common;

/// <summary>
/// Tüm domain entity'leri için ortak temel sınıftır.
/// 
/// Bu sınıfın amacı:
/// - Her entity'de tekrar tekrar Id alanı yazmamızı engellemek.
/// - Domain entity'leri için ortak bir kimlik standardı oluşturmak.
/// - Entity'lerin veritabanında ve domain içinde benzersiz şekilde temsil edilmesini sağlamak.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliğidir.
    ///
    /// Guid kullanmamızın sebebi:
    /// - Uygulama tarafında benzersiz Id üretilebilir.
    /// - İleride distributed/microservice yapılara geçilirse daha güvenlidir.
    /// - Veritabanına gitmeden önce entity kimliği oluşabilir.
    ///
    /// protected set:
    /// - Dışarıdan rastgele Id değiştirilmesini engeller.
    /// - EF Core gibi ORM araçları yine bu alanı set edebilir.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// EF Core'un entity oluşturabilmesi için parameterless constructor bırakıyoruz.
    /// protected olmasının sebebi dış katmanların doğrudan BaseEntity oluşturamamasıdır.
    /// </summary>
    protected BaseEntity()
    {
    }

    /// <summary>
    /// Testlerde veya özel domain senaryolarında Id kontrollü verilebilsin diye ek constructor.
    /// </summary>
    /// <param name="id">Entity için kullanılacak benzersiz Id.</param>
    protected BaseEntity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity Id boş Guid olamaz.", nameof(id));
        }

        Id = id;
    }
}