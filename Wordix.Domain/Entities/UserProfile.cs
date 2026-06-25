using Wordix.Domain.Common;
using Wordix.Domain.Enums;

namespace Wordix.Domain.Entities;

/// <summary>
/// Keycloak ile giriş yapan kullanıcının Wordix uygulamasındaki profil karşılığıdır.
/// 
/// Keycloak authentication tarafını yönetir.
/// Wordix ise uygulama içi kullanıcı bilgilerini burada tutar.
/// 
/// Örnek:
/// - Kullanıcının ana dili
/// - Hedef dili
/// - Hesap tipi
/// - Uygulamadaki görünen adı
/// </summary>
public class UserProfile : AuditableEntity
{

    /// <summary>
    /// EF Core'un entity oluşturabilmesi için protected constructor bırakıyoruz.
    /// Dışarıdan boş UserProfile oluşturulmasını istemiyoruz.
    /// </summary>
    protected UserProfile()
    {
    }

    /// <summary>
    /// Yeni bir Wordix kullanıcı profili oluşturur.
    /// 
    /// Bu constructor genelde /api/profile/me akışında kullanılacaktır.
    /// Token'dan gelen Keycloak bilgileriyle Wordix profili oluşturulur.
    /// </summary>
    public UserProfile(
        string keycloakUserId,
        string email,
        string username,
        AccountType accountType,
        string? displayName = null,
        Guid? nativeLanguageId = null,
        Guid? targetLanguageId = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            throw new ArgumentException("Keycloak kullanıcı Id boş olamaz.", nameof(keycloakUserId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email boş olamaz.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username boş olamaz.", nameof(username));
        }

        KeycloakUserId = keycloakUserId.Trim();
        Email = email.Trim().ToLowerInvariant();
        Username = username.Trim();
        AccountType = accountType;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        NativeLanguageId = nativeLanguageId;
        TargetLanguageId = targetLanguageId;
    }

    /// <summary>
    /// Keycloak içindeki kullanıcı Id bilgisidir.
    /// 
    /// Bu alan çok önemlidir çünkü JWT token içindeki subject/sub claim'i ile
    /// Wordix database'indeki kullanıcıyı eşleştirmek için kullanılır.
    /// </summary>
    public string KeycloakUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanıcının email adresidir.
    /// Keycloak token içinden okunabilir ve Wordix profilinde saklanır.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Kullanıcının username bilgisidir.
    /// Bazı durumlarda email ile aynı olabilir.
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Uygulamada gösterilecek kullanıcı adıdır.
    /// Boş bırakılırsa username veya email üzerinden gösterim yapılabilir.
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Kullanıcının Wordix içindeki hesap tipidir.
    /// 
    /// Keycloak rollerinden ayrı tutulur.
    /// Keycloak yetkilendirme içindir, AccountType ise uygulama içi profil anlamı taşır.
    /// </summary>
    public AccountType AccountType { get; private set; }

    /// <summary>
    /// Kullanıcının ana dilidir.
    /// Örneğin Türkçe için Language tablosundaki tr kaydına bağlanabilir.
    /// </summary>
    public Guid? NativeLanguageId { get; private set; }

    /// <summary>
    /// Kullanıcının öğrenmek istediği hedef dildir.
    /// Örneğin İngilizce için Language tablosundaki en kaydına bağlanabilir.
    /// </summary>
    public Guid? TargetLanguageId { get; private set; }

    /// <summary>
    /// Kullanıcı hesabı uygulama içinde aktif mi?
    /// 
    /// IsDeleted yerine IsActive kullanmamızın sebebi:
    /// Kullanıcı hesabı silinmemiş olabilir ama geçici olarak pasif yapılabilir.
    /// </summary>
    public bool IsActive { get; private set; } = true;
    

    /// <summary>
    /// Kullanıcının görünen adını günceller.
    /// 
    /// Profil güncelleme endpointinde kullanılabilir.
    /// </summary>
    public void ChangeDisplayName(string? displayName)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();

        // Profilde değişiklik olduğu için UpdatedAt alanını güncelliyoruz.
        MarkAsUpdated();
    }

    /// <summary>
    /// Kullanıcının dil tercihlerini günceller.
    /// 
    /// Ana dil ve hedef dil kullanıcı deneyimini etkiler.
    /// Örneğin İngilizce öğrenen Türk kullanıcı için:
    /// NativeLanguage = Turkish
    /// TargetLanguage = English
    /// </summary>
    public void ChangeLanguages(Guid? nativeLanguageId, Guid? targetLanguageId)
    {
        NativeLanguageId = nativeLanguageId;
        TargetLanguageId = targetLanguageId;

        MarkAsUpdated();
    }

    /// <summary>
    /// Kullanıcıyı uygulama içinde pasif hale getirir.
    /// 
    /// Bu fiziksel silme değildir.
    /// Kullanıcının geçmiş quiz, lookup ve progress verileri korunur.
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
    /// Pasif kullanıcıyı tekrar aktif hale getirir.
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