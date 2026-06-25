namespace Wordix.Domain.Common;

/// <summary>
/// Soft delete destekleyen entity'ler için ortak base sınıftır.
/// 
/// Soft delete ne demek?
/// - Kaydı veritabanından fiziksel olarak silmeyiz.
/// - Bunun yerine IsDeleted = true yaparız.
/// - Böylece geçmiş kayıtları, analytics verilerini ve ilişkileri koruyabiliriz.
/// </summary>
public abstract class SoftDeleteEntity : AuditableEntity
{
    /// <summary>
    /// Kayıt silinmiş gibi davranılsın mı?
    ///
    /// false: kayıt aktif olarak kullanılabilir.
    /// true: kayıt uygulama tarafından silinmiş kabul edilir.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Kaydın ne zaman soft delete edildiğini tutar.
    ///
    /// Nullable olmasının sebebi:
    /// - Silinmemiş kayıtlarda DeletedAt değeri olmaz.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// EF Core için protected parameterless constructor.
    /// </summary>
    protected SoftDeleteEntity()
    {
    }

    /// <summary>
    /// Id'nin kontrollü verilebildiği constructor.
    /// </summary>
    protected SoftDeleteEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Kaydı fiziksel olarak silmeden silinmiş kabul eder.
    ///
    /// Örneğin:
    /// - Kullanıcı dictionary'den bir kelimeyi kaldırdı.
    /// - Deck pasifleştirildi.
    /// - Admin bir içeriği yayından kaldırdı.
    ///
    /// Bu durumda veri tamamen kaybolmaz, sadece aktif sorgularda görünmez.
    /// </summary>
    /// <param name="deletedAtUtc">
    /// UTC formatında silinme tarihi.
    /// Eğer değer verilmezse DateTime.UtcNow kullanılır.
    /// </param>
    public void SoftDelete(DateTime? deletedAtUtc = null)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAtUtc ?? DateTime.UtcNow;

        // Silme işlemi de entity üzerinde bir değişiklik olduğu için UpdatedAt güncellenir.
        MarkAsUpdated(DeletedAt);
    }

    /// <summary>
    /// Soft delete edilmiş bir kaydı tekrar kullanılabilir hale getirir.
    ///
    /// Örneğin kullanıcı daha önce kaldırdığı bir dictionary item'ı geri getirmek isterse
    /// bu mantık kullanılabilir.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAt = null;

        // Geri alma işlemi de entity üzerinde bir değişikliktir.
        MarkAsUpdated();
    }
}