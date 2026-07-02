using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserDictionary.Commands.RemoveUserLearningFlag;

/// <summary>
/// RemoveUserLearningFlagCommand isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - UserLearningItem current user'a ait mi kontrol eder.
/// - FlagType string değerini domain enum'a çevirir.
/// - İlgili UserLearningFlag kaydını bulur.
/// - Flag'i fiziksel olarak siler.
/// - Silinen flag bilgisini response olarak döner.
/// 
/// Ownership zinciri:
/// UserLearningFlag -> UserLearningItem -> KeycloakUserId
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - HttpContext kullanmaz.
/// - Controller logic'i içermez.
/// - Response mapping'i handler içine dağıtmaz.
/// </summary>
public sealed class RemoveUserLearningFlagCommandHandler
    : IRequestHandler<RemoveUserLearningFlagCommand, UserLearningFlagResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningFlag> _userLearningFlagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserLearningFlagCommandHandler(
        ICurrentUserService currentUserService,
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<UserLearningFlag> userLearningFlagRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userLearningItemRepository = userLearningItemRepository;
        _userLearningFlagRepository = userLearningFlagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserLearningFlagResponse> Handle(
        RemoveUserLearningFlagCommand request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // Önce route'tan gelen UserLearningItemId gerçekten current user'a mı ait kontrol ediyoruz.
        //
        // Başka kullanıcıya ait item id gönderilirse NotFound döner.
        // Böylece başka kullanıcının item'ı var mı yok mu bilgisini sızdırmayız.
        var userLearningItem = await _userLearningItemRepository.FirstOrDefaultAsync(
            item =>
                item.Id == request.UserLearningItemId &&
                item.KeycloakUserId == keycloakUserId &&
                item.IsActive,
            cancellationToken);

        if (userLearningItem is null)
        {
            throw new NotFoundException(
                "User dictionary item",
                request.UserLearningItemId);
        }

        var flagType = ParseFlagType(request.FlagType);

        var flag = await _userLearningFlagRepository.FirstOrDefaultAsync(
            flag =>
                flag.UserLearningItemId == userLearningItem.Id &&
                flag.FlagType == flagType,
            cancellationToken);

        if (flag is null)
        {
            throw new NotFoundException(
                "User learning flag",
                $"{request.UserLearningItemId}:{request.FlagType}");
        }

        // Silmeden önce response modelini hazırlıyoruz.
        // Çünkü SaveChanges sonrası kayıt database'den kalkacak.
        var response = UserDictionaryMapper.ToUserLearningFlagResponse(flag);

        _userLearningFlagRepository.Remove(flag);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    /// <summary>
    /// String flag type değerini domain enum'a çevirir.
    /// 
    /// Normalde validator bu değerin geçerli olduğunu garanti eder.
    /// Yine de handler kendi içinde defensive davranır.
    /// </summary>
    private static UserLearningFlagType ParseFlagType(string flagType)
    {
        if (Enum.TryParse<UserLearningFlagType>(
                flagType?.Trim(),
                ignoreCase: true,
                out var parsedFlagType)
            && Enum.IsDefined(parsedFlagType))
        {
            return parsedFlagType;
        }

        throw new BusinessRuleException(
            $"Flag type '{flagType}' is not supported.",
            "FLAG_TYPE_NOT_SUPPORTED");
    }
}