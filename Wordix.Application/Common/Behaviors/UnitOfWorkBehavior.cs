using MediatR;
using Wordix.Application.Common.Interfaces.Persistence;

namespace Wordix.Application.Common.Behaviors;

/// <summary>
/// Transactional commandler için SaveChangesAsync çağrısını merkezi hale getiren MediatR pipeline behavior'dır.
/// 
/// Neden var?
/// - Handler içinde sürekli _unitOfWork.SaveChangesAsync(...) tekrarını azaltır.
/// - SaveChanges kararını use-case seviyesinde marker interface ile yönetir.
/// - Query handlerlarda gereksiz SaveChanges çalışmasını engeller.
/// 
/// Nasıl çalışır?
/// - Request ITransactionalRequest değilse sadece handler'ı çalıştırır.
/// - Request ITransactionalRequest ise handler başarılı bittikten sonra SaveChangesAsync çağırır.
/// 
/// Önemli:
/// - Handler exception fırlatırsa SaveChangesAsync çalışmaz.
/// - Bu yüzden exception fırlatmadan önce kayıt yapması gereken özel akışlar
///   bu marker interface'e alınmamalıdır.
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Query veya transactional olmayan request ise SaveChanges çalıştırmadan devam eder.
        if (request is not ITransactionalRequest)
        {
            return await next();
        }

        // Transactional commandlerde önce handler çalışır.
        // Handler entity ekleme/güncelleme/silme işlemlerini yapar.
        var response = await next();

        // Handler başarılı bittiyse değişiklikler tek noktadan kaydedilir.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}