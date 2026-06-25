using Wordix.Shared.Responses;

namespace Wordix.Application.Common.Exceptions;

/// <summary>
/// Validation hatalarını taşımak için kullanılan custom exception'dır.
/// 
/// Dikkat:
/// FluentValidation'ın da ValidationException sınıfı vardır.
/// Bizim sınıfımız Wordix.Application.Common.Exceptions namespace'i altındadır.
/// Middleware tarafında namespace alias kullanarak karışıklığı engelleyeceğiz.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Validation hatalarının listesi.
    /// Örnek:
    /// - Text boş olamaz.
    /// - QuestionCount 1 ile 50 arasında olmalı.
    /// </summary>
    public IReadOnlyCollection<ValidationError> Errors { get; }

    /// <summary>
    /// Tek mesajla validation exception oluşturmak için kullanılır.
    /// </summary>
    public ValidationException(string message)
        : base(message)
    {
        Errors = Array.Empty<ValidationError>();
    }

    /// <summary>
    /// Birden fazla validation hatasını taşıyan exception oluşturur.
    /// </summary>
    public ValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// IEnumerable ile gelen hataları güvenli şekilde listeye çevirir.
    /// </summary>
    public ValidationException(IEnumerable<ValidationError> errors)
        : this(errors.ToArray())
    {
    }
}