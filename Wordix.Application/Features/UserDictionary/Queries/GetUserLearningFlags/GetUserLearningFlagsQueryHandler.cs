using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Application.Features.UserDictionary.Mappers;
using Wordix.Domain.Entities;

namespace Wordix.Application.Features.UserDictionary.Queries.GetUserLearningFlags;

/// <summary>
/// GetUserLearningFlagsQuery isteğini işleyen handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Route'tan gelen UserLearningItemId gerçekten current user'a ait mi kontrol eder.
/// - İlgili UserLearningItem'a bağlı flagleri listeler.
/// - Response DTO mapping işlemini UserDictionaryMapper'a bırakır.
/// 
/// Ownership zinciri:
/// UserLearningFlag -> UserLearningItem -> KeycloakUserId
/// 
/// Bu handler ne yapmaz?
/// - DbContext kullanmaz.
/// - Controller logic'i içermez.
/// - Response propertylerini controller içinde dizmez.
/// </summary>
public sealed class GetUserLearningFlagsQueryHandler
    : IRequestHandler<GetUserLearningFlagsQuery, GetUserLearningFlagsResponse>
{
    private readonly IRepository<UserLearningItem> _userLearningItemRepository;
    private readonly IRepository<UserLearningFlag> _userLearningFlagRepository;

    public GetUserLearningFlagsQueryHandler(
        IRepository<UserLearningItem> userLearningItemRepository,
        IRepository<UserLearningFlag> userLearningFlagRepository)
    {
        _userLearningItemRepository = userLearningItemRepository;
        _userLearningFlagRepository = userLearningFlagRepository;
    }

    public async Task<GetUserLearningFlagsResponse> Handle(
        GetUserLearningFlagsQuery request,
        CancellationToken cancellationToken)
    {
        var keycloakUserId = request.KeycloakUserId;

        // Önce UserLearningItem current user'a ait mi kontrol ediyoruz.
        //
        // Flagler doğrudan KeycloakUserId tutmadığı için ownership kontrolü
        // UserLearningItem üzerinden yapılır.
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

        var flags = await _userLearningFlagRepository.ListAsync(
            flag => flag.UserLearningItemId == userLearningItem.Id,
            cancellationToken);

        var orderedFlags = flags
            .OrderBy(flag => flag.FlagType)
            .ThenByDescending(flag => flag.CreatedAt)
            .ToArray();

        return UserDictionaryMapper.ToGetUserLearningFlagsResponse(orderedFlags);
    }
}