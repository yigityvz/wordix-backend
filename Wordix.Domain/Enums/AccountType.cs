namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcının Wordix uygulamasındaki hesap tipini temsil eder.
/// 
/// Keycloak authentication ve role bilgisini yönetir.
/// Wordix database tarafında ise kullanıcının uygulama içi profil türünü
/// bu enum ile saklayabiliriz.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Normal uygulama kullanıcısı.
    /// Kelime arar, dictionary oluşturur, quiz çözer.
    /// </summary>
    BasicUser = 1,

    /// <summary>
    /// Sistem yönetimi ve analytics ekranları için kullanılacak admin kullanıcı.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Gelecekte öğretmen-öğrenci modülü için kullanılabilir.
    /// MVP kapsamında aktif kullanılmayacak.
    /// </summary>
    Teacher = 3,

    /// <summary>
    /// Gelecekte öğretmen-öğrenci modülü için kullanılabilir.
    /// MVP kapsamında aktif kullanılmayacak.
    /// </summary>
    Student = 4
}