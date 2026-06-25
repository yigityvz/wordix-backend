namespace Wordix.Domain.Common;

/// <summary>
/// Oluşturulma ve güncellenme tarihlerini tutan ortak base sınıftır.
/// 
/// Bu sınıfı, kayıt geçmişi önemli olan entity'lerde kullanacağız.
/// Örneğin:
/// - UserProfile
/// - LearningItem
/// - Word
/// - Meaning
/// - QuizSession
/// - UserLearningProgress
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Entity'nin oluşturulma zamanıdır.
    ///
    /// UTC kullanmamızın sebebi:
    /// - Sunucu farklı ülkede olabilir.
    /// - Kullanıcı farklı timezone'da olabilir.
    /// - Database tarafında tek standart zaman tutmak daha güvenlidir.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Entity'nin en son güncellenme zamanıdır.
    ///
    /// Nullable olmasının sebebi:
    /// - Yeni oluşturulan bir kayıt henüz hiç güncellenmemiş olabilir.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// EF Core için protected parameterless constructor.
    /// </summary>
    protected AuditableEntity()
    {
    }

    /// <summary>
    /// Id'nin kontrollü verilebildiği constructor.
    /// BaseEntity constructor'ına Id bilgisini gönderir.
    /// </summary>
    protected AuditableEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// CreatedAt alanını kontrollü şekilde set etmek için kullanılır.
    ///
    /// Bunu ileride Persistence katmanında SaveChanges override ederken kullanabiliriz.
    /// Böylece CreatedAt alanı dışarıdan rastgele değiştirilmez,
    /// ama sistem kontrollü biçimde set edebilir.
    /// </summary>
    /// <param name="createdAtUtc">UTC formatında oluşturulma tarihi.</param>
    public void SetCreatedAt(DateTime createdAtUtc)
    {
        CreatedAt = createdAtUtc;
    }

    /// <summary>
    /// Entity güncellendiğinde UpdatedAt alanını işaretlemek için kullanılır.
    ///
    /// Örneğin kullanıcı profilini güncellediğinde,
    /// kelime anlamı değiştiğinde veya progress güncellendiğinde çağrılabilir.
    /// </summary>
    /// <param name="updatedAtUtc">
    /// UTC formatında güncelleme tarihi.
    /// Eğer değer verilmezse DateTime.UtcNow kullanılır.
    /// </param>
    public void MarkAsUpdated(DateTime? updatedAtUtc = null)
    {
        UpdatedAt = updatedAtUtc ?? DateTime.UtcNow;
    }
}