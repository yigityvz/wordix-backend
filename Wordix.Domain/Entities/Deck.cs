using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Kullanıcının oluşturduğu çalışma koleksiyonunu temsil eder.
/// 
/// Deck nedir?
/// - Kullanıcının dictionary'sindeki itemları grupladığı kişisel koleksiyondur.
/// - Sadece klasör gibi düşünülmemelidir.
/// - İleride quiz, statistics ve dashboard tarafında kaynak olarak kullanılacaktır.
/// 
/// Örnek:
/// - Software English
/// - Daily Phrases
/// - Internship Words
/// 
/// Yeni kullanıcı modeli:
/// - Backend UserProfile oluşturmaz.
/// - Deck ownership KeycloakUserId ile yapılır.
/// - KeycloakUserId JWT token içindeki "sub" claiminden gelir.
/// </summary>
public class Deck : AuditableEntity
{
    /// <summary>
    /// EF Core için protected constructor.
    /// 
    /// Dışarıdan boş Deck oluşturulmasını istemiyoruz.
    /// EF Core database'den veri okurken bu constructor'ı kullanabilir.
    /// </summary>
    protected Deck()
    {
    }

    /// <summary>
    /// Yeni bir deck oluşturur.
    /// 
    /// keycloakUserId:
    /// - Deck'in hangi kullanıcıya ait olduğunu gösterir.
    /// - UserProfileId değildir.
    /// - Token içindeki sub claiminden gelen kullanıcı id değeridir.
    /// 
    /// name:
    /// - Kullanıcının göreceği deck adıdır.
    /// 
    /// normalizedName:
    /// - Duplicate kontrolü için normalize edilmiş deck adıdır.
    /// - Örnek: " Software English " -> "software english"
    /// </summary>
    public Deck(
        string keycloakUserId,
        string name,
        string normalizedName,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("KeycloakUserId boş olamaz.", nameof(keycloakUserId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Deck adı boş olamaz.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Normalize edilmiş deck adı boş olamaz.", nameof(normalizedName));
        }

        KeycloakUserId = keycloakUserId.Trim();
        Name = name.Trim();
        NormalizedName = normalizedName.Trim().ToLowerInvariant();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        IsActive = true;
    }

    /// <summary>
    /// Deck'in sahibi olan Keycloak kullanıcısının id değeridir.
    /// 
    /// Bu alan sayesinde kullanıcı sadece kendi deck kayıtlarını görebilir/değiştirebilir.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanıcının göreceği deck adıdır.
    /// 
    /// Örnek:
    /// Software English
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Duplicate deck adı kontrolü için normalize edilmiş deck adıdır.
    /// 
    /// Örnek:
    /// " Software English " -> "software english"
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Deck açıklamasıdır.
    /// 
    /// Opsiyoneldir.
    /// Kullanıcı isterse deck'in ne amaçla oluşturulduğunu yazabilir.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Deck aktif mi bilgisidir.
    /// 
    /// Faz 20'de delete endpointi yazmayacağız.
    /// Ama ileride deck silme/arsivleme geldiğinde soft delete gibi kullanılabilir.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Deck adını ve açıklamasını günceller.
    /// 
    /// Faz 20'de update endpointi yazmayacağız.
    /// Ama entity davranışı future-ready dursun diye methodu burada tutuyoruz.
    /// </summary>
    public void UpdateDetails(
        string name,
        string normalizedName,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Deck adı boş olamaz.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("Normalize edilmiş deck adı boş olamaz.", nameof(normalizedName));
        }

        Name = name.Trim();
        NormalizedName = normalizedName.Trim().ToLowerInvariant();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Deck'i pasif hale getirir.
    /// 
    /// Faz 20'de delete endpointi yok.
    /// İleride soft delete davranışı gerektiğinde kullanılabilir.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Pasif deck'i tekrar aktif hale getirir.
    /// 
    /// MVP'de kullanılmayabilir ama entity davranışı olarak hazır durur.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        MarkAsUpdated();
    }
}