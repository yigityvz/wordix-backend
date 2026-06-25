namespace Wordix.Application.Common.Exceptions;

/// <summary>
/// Kullanıcının ilgili kaynağa erişim yetkisi olmadığı durumlarda kullanılan exception'dır.
/// 
/// Örnek:
/// - Kullanıcı başka bir kullanıcının dictionary item'ını görmeye çalışır.
/// - Kullanıcı başkasının quiz session detayına erişmeye çalışır.
/// - Basic user admin işlemine benzer bir use-case çalıştırmaya çalışır.
/// </summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>
    /// Varsayılan forbidden mesajı ile exception oluşturur.
    /// </summary>
    public ForbiddenException()
        : base("You do not have permission to access this resource.")
    {
    }

    /// <summary>
    /// Özel mesaj ile forbidden exception oluşturur.
    /// </summary>
    public ForbiddenException(string message)
        : base(message)
    {
    }
}