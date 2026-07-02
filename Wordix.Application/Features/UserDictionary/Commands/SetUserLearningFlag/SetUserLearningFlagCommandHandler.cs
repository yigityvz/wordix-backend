using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.UserDictionary.Commands.SetUserLearningFlag;

/// <summary>
/// SetUserLearningFlagCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - UserLearningItem current user'a ait mi kontrol eder.
/// - FlagType string değerini domain enum'a çevirir.
/// - Aynı flag zaten varsa mevcut flag'i döner.
/// - Yoksa UserLearningFlag entity'si oluşturur.
/// - UnitOfWork ile kaydeder.
/// - Response DTO'yu mapper üzerinden döner.
/// 
/// Önemli davranış:
/// Bu endpoint idempotent çalışır.
/// Yani aynı item'a aynı flag ikinci kez gönderilirse hata vermez,
/// mevcut flag kaydını döner.
/// 
/// Ownership zinciri:
/// UserLearningFlag -> UserLearningItem -> KeycloakUserId
/// </summary>
public sealed class SetUserLearningFlagCommandHandler
    : IRequestHandler<SetUserLearningFlagCommand, UserLearningFlagResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningFlag> _userLearningFlagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserLearningFlagCommandHandler(
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
        SetUserLearningFlagCommand request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

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

        // Idempotent davranış:
        // Aynı flag zaten varsa duplicate hata vermiyoruz.
        // Mevcut kaydı response olarak dönüyoruz.
        var existingFlag = await _userLearningFlagRepository.FirstOrDefaultAsync(
            flag =>
                flag.UserLearningItemId == userLearningItem.Id &&
                flag.FlagType == flagType,
            cancellationToken);

        if (existingFlag is not null)
        {
            return UserDictionaryMapper.ToUserLearningFlagResponse(existingFlag);
        }

        var flag = new UserLearningFlag(
            userLearningItem.Id,
            flagType);

        await _userLearningFlagRepository.AddAsync(
            flag,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDictionaryMapper.ToUserLearningFlagResponse(flag);
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