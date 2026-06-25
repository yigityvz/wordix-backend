using FluentValidation;
using MediatR;
using Wordix.Shared.Responses;
using WordixValidationException = Wordix.Application.Common.Exceptions.ValidationException;

namespace Wordix.Application.Common.Behaviors;

/// <summary>
/// MediatR requestleri için merkezi validation pipeline behavior'dır.
/// 
/// Bu class ne yapar?
/// - MediatR üzerinden gelen command/query için ilgili FluentValidation validator'larını bulur.
/// - Validator varsa request'i handler'a gitmeden önce doğrular.
/// - Validation hatası varsa Wordix'in custom ValidationException sınıfını fırlatır.
/// - ExceptionMiddleware bu exception'ı standart ErrorResponse formatına çevirir.
/// 
/// Neden önemli?
/// - Her handler içinde manuel validation yazmayız.
/// - Validation logic ile business logic ayrılmış olur.
/// - Tüm validation hataları tek formatta döner.
/// </summary>
/// <typeparam name="TRequest">
/// MediatR'a gönderilen command/query tipidir.
/// Örnek:
/// - GetCurrentUserProfileQuery
/// - CreateLookupCommand
/// </typeparam>
/// <typeparam name="TResponse">
/// Handler'ın döndürdüğü response tipidir.
/// </typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Bu request tipi için DI container'da kayıtlı tüm validator'ları alır.
    /// 
    /// Örnek:
    /// Eğer TRequest = CreateLookupCommand ise,
    /// DI container içinde CreateLookupCommandValidator var mı bakılır.
    /// </summary>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// MediatR pipeline'da handler'dan önce çalışan methoddur.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Eğer bu request için hiç validator yoksa validation yapmadan handler'a geçeriz.
        if (!_validators.Any())
        {
            return await next();
        }

        // FluentValidation'ın beklediği validation context'i oluşturuyoruz.
        var validationContext = new ValidationContext<TRequest>(request);

        // Aynı request için birden fazla validator olabilir.
        // Hepsini async olarak çalıştırıyoruz.
        var validationResults = await Task.WhenAll(
            _validators.Select(validator =>
                validator.ValidateAsync(validationContext, cancellationToken)));

        // FluentValidation hata modelini bizim standart ValidationError modelimize çeviriyoruz.
        var validationErrors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .Select(failure => new ValidationError(
                propertyName: failure.PropertyName,
                errorMessage: failure.ErrorMessage,
                errorCode: failure.ErrorCode))
            .ToArray();

        // Hata varsa handler'a gitmiyoruz.
        // Custom ValidationException fırlatıyoruz.
        // Faz 10'daki ExceptionMiddleware bunu 400 VALIDATION_ERROR response'una çevirir.
        if (validationErrors.Length > 0)
        {
            throw new WordixValidationException(validationErrors);
        }

        // Hata yoksa pipeline'daki bir sonraki adıma geçiyoruz.
        // Bu genellikle asıl handler'dır.
        return await next();
    }
}