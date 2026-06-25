using Wordix.Domain.Common;

namespace Wordix.Domain.Entities;

/// <summary>
/// Wordix sisteminde desteklenen dilleri temsil eder.
/// 
/// Örneğin:
/// - en / English / English
/// - tr / Turkish / Türkçe
/// - de / German / Deutsch
/// 
/// Dilleri enum yapmak yerine entity yapmamızın sebebi:
/// Sisteme yeni dil eklemek istediğimizde kod değiştirmeden database üzerinden yönetebilmek.
/// </summary>
public class Language : AuditableEntity
{

    /// <summary>
    /// EF Core için protected constructor.
    /// </summary>
    protected Language()
    {
    }

    /// <summary>
    /// Yeni desteklenen dil kaydı oluşturur.
    /// </summary>
    public Language(string code, string name, string nativeName)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Dil kodu boş olamaz.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dil adı boş olamaz.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(nativeName))
        {
            throw new ArgumentException("Dilin yerel adı boş olamaz.", nameof(nativeName));
        }

        Code = code.Trim().ToLowerInvariant();
        Name = name.Trim();
        NativeName = nativeName.Trim();
    }

    /// <summary>
    /// Dil kodudur.
    /// ISO benzeri kısa kodlar kullanılır.
    /// Örnek: en, tr, de
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Dilin İngilizce veya sistem genelindeki adıdır.
    /// Örnek: English, Turkish, German
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Dilin kendi dilindeki adıdır.
    /// Örnek:
    /// English için English
    /// Turkish için Türkçe
    /// German için Deutsch
    /// </summary>
    public string NativeName { get; private set; } = string.Empty;

    /// <summary>
    /// Dil uygulamada aktif kullanılabilir mi?
    /// 
    /// Örneğin Almanca kaydı sistemde durabilir ama henüz aktif edilmeyebilir.
    /// </summary>
    public bool IsActive { get; private set; } = true;


    /// <summary>
    /// Dil bilgilerini günceller.
    /// 
    /// Code alanını burada değiştirmiyoruz.
    /// Çünkü code ilişkilerde ve sorgularda sabit kimlik gibi kullanılabilir.
    /// Eğer code değişecekse bunu ayrı ve kontrollü bir iş kuralıyla yapmak daha doğru olur.
    /// </summary>
    public void UpdateNames(string name, string nativeName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dil adı boş olamaz.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(nativeName))
        {
            throw new ArgumentException("Dilin yerel adı boş olamaz.", nameof(nativeName));
        }

        Name = name.Trim();
        NativeName = nativeName.Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Dili uygulamada kullanılabilir hale getirir.
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

    /// <summary>
    /// Dili uygulamada pasif hale getirir.
    /// 
    /// Bu silme işlemi değildir.
    /// Dil kaydı database'de kalır ama yeni işlemlerde kullanıma kapatılabilir.
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
}