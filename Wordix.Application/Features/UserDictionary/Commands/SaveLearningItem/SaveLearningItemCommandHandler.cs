using MediatR;
using Wordix.Application.Common.Exceptions;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.UserDictionary.Dtos.Responses;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;
using Wordix.Application.Features.UserDictionary.Mappers;

namespace Wordix.Application.Features.UserDictionary.Commands.SaveLearningItem;

/// <summary>
/// SaveLearningItemCommand isteğini işleyen MediatR handler'dır.
/// 
/// Bu handler ne yapar?
/// - Current user'ın KeycloakUserId değerini alır.
/// - Kaydedilecek LearningItem gerçekten var mı kontrol eder.
/// - SelectedMeaningId gönderildiyse ilgili LearningItem'a ait mi kontrol eder.
/// - SourceLookupHistoryId gönderildiyse current user'a ait mi kontrol eder.
/// - Kullanıcı bu LearningItem'ı daha önce dictionary'sine eklemiş mi kontrol eder.
/// - UserLearningItem oluşturur.
/// - UserLearningProgress oluşturur.
/// - İlk kayıt olayını LearningProgressHistory olarak kayıt altına alır.
/// - SaveLearningItemResponse döner.
/// 
/// Yeni kullanıcı modeli:
/// - Backend artık UserProfile oluşturmaz.
/// - Backend UserProfileId/UserId üretmez.
/// - Kullanıcı sahipliği token içindeki KeycloakUserId ile yapılır.
/// 
/// Bu handler neden Application katmanında?
/// - Bu bir use-case akışıdır.
/// - HTTP detayı bilmez.
/// - DbContext bilmez.
/// - Keycloak claim detayı bilmez.
/// - Repository ve servis abstraction'ları üzerinden çalışır.
/// </summary>
public sealed class SaveLearningItemCommandHandler
    : IRequestHandler<SaveLearningItemCommand, SaveLearningItemResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserLearningItemRepository _userLearningItemRepository;
    private readonly IRepository<LearningItem> _learningItemRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IRepository<LookupHistory> _lookupHistoryRepository;
    private readonly IRepository<UserLearningItem> _userLearningItemGenericRepository;
    private readonly IRepository<UserLearningProgress> _userLearningProgressRepository;
    private readonly IRepository<LearningProgressHistory> _learningProgressHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Handler ihtiyacı olan tüm servisleri DI üzerinden alır.
    /// 
    /// Burada DbContext yok.
    /// Burada HttpContext yok.
    /// Burada controller yok.
    /// 
    /// Current user bilgisi ICurrentUserService üzerinden alınır.
    /// Bu servis Application katmanına sadece gerekli kullanıcı bilgisini sağlar.
    /// </summary>
    public SaveLearningItemCommandHandler(
        ICurrentUserService currentUserService,
        IUserLearningItemRepository userLearningItemRepository,
        IRepository<LearningItem> learningItemRepository,
        IRepository<Meaning> meaningRepository,
        IRepository<LookupHistory> lookupHistoryRepository,
        IRepository<UserLearningItem> userLearningItemGenericRepository,
        IRepository<UserLearningProgress> userLearningProgressRepository,
        IRepository<LearningProgressHistory> learningProgressHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userLearningItemRepository = userLearningItemRepository;
        _learningItemRepository = learningItemRepository;
        _meaningRepository = meaningRepository;
        _lookupHistoryRepository = lookupHistoryRepository;
        _userLearningItemGenericRepository = userLearningItemGenericRepository;
        _userLearningProgressRepository = userLearningProgressRepository;
        _learningProgressHistoryRepository = learningProgressHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// SaveLearningItemCommand çalıştırıldığında MediatR tarafından çağrılan ana methoddur.
    /// </summary>
    public async Task<SaveLearningItemResponse> Handle(
        SaveLearningItemCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Current user'ın KeycloakUserId değerini alıyoruz.
        //
        // Bu değer JWT token içindeki "sub" claiminden gelir.
        // Backend burada UserProfile oluşturmaz, UserProfileId üretmez.
        // Kullanıcıya ait dictionary/progress/quiz gibi kayıtlar bu KeycloakUserId ile ilişkilendirilir.
        var keycloakUserId = _currentUserService.GetRequiredKeycloakUserId();

        // 2. Kaydedilecek LearningItem gerçekten var mı kontrol ediyoruz.
        var learningItem = await GetRequiredLearningItemAsync(
            request.LearningItemId,
            cancellationToken);

        // 3. Kullanıcı bu LearningItem'ı daha önce kaydetmiş mi kontrol ediyoruz.
        // Aynı kullanıcı aynı LearningItem'ı ikinci kez kaydedemez.
        // Yeni mimaride duplicate kontrolü UserProfileId ile değil KeycloakUserId ile yapılır.
        var alreadySaved = await _userLearningItemRepository.ExistsByUserAndLearningItemAsync(
            keycloakUserId,
            learningItem.Id,
            cancellationToken);

        if (alreadySaved)
        {
            throw new BusinessRuleException(
                "This learning item is already saved in your dictionary.",
                "LEARNING_ITEM_ALREADY_SAVED");
        }

        // 4. SelectedMeaningId gönderildiyse gerçekten bu LearningItem'a ait mi kontrol ediyoruz.
        var selectedMeaning = await GetSelectedMeaningOrNullAsync(
            request.SelectedMeaningId,
            learningItem.Id,
            cancellationToken);

        // 5. SelectedMeaningId gönderilmediyse primary meaning bulmaya çalışıyoruz.
        // Böylece frontend anlam id göndermese bile sistem makul bir default seçebilir.
        selectedMeaning ??= await GetPrimaryMeaningOrNullAsync(
            learningItem.Id,
            cancellationToken);

        // 6. SourceLookupHistoryId gönderildiyse bu lookup history current user'a ait mi kontrol ediyoruz.
        // Kullanıcı başkasının lookup history id'sini gönderip ilişki kuramamalı.
        await EnsureSourceLookupHistoryBelongsToCurrentUserAsync(
            request.SourceLookupHistoryId,
            keycloakUserId,
            cancellationToken);

        // 7. Kullanıcı dictionary kaydı oluşturuyoruz.
        // UserLearningItem artık UserProfileId değil KeycloakUserId alır.
        var userLearningItem = new UserLearningItem(
            keycloakUserId,
            learningItem.Id,
            selectedMeaning?.Id,
            request.SourceLookupHistoryId);

        // 8. Kullanıcı öğrenme progress kaydını oluşturuyoruz.
        // Mevcut UserLearningProgress entity'si UserLearningItem üzerinden bire bir ilerler.
        // Bu yüzden KeycloakUserId veya LearningItemId progress constructor'ına verilmez.
        var userLearningProgress = new UserLearningProgress(userLearningItem.Id);

        // 9. İlk kayıt olayını progress history olarak kayıt altına alıyoruz.
        // Mevcut LearningProgressHistory entity'sinde event type yok.
        // Bu entity progress durum/skor değişimini tutar.
        //
        // İlk dictionary kaydında eski bir progress olmadığı için,
        // prototipte old/new değerleri başlangıç değerleriyle aynı yazıyoruz.
        // Asıl quiz cevaplama fazında bu history gerçek değişimleri tutacak.
        var progressHistory = new LearningProgressHistory(
            userLearningProgress.Id,
            userLearningProgress.LearningStatus,
            userLearningProgress.LearningStatus,
            userLearningProgress.LearningConfidenceScore,
            userLearningProgress.LearningConfidenceScore,
            "Learning item saved to dictionary.");

        // 10. Entity'leri repository üzerinden ekliyoruz.
        await _userLearningItemGenericRepository.AddAsync(
            userLearningItem,
            cancellationToken);

        await _userLearningProgressRepository.AddAsync(
            userLearningProgress,
            cancellationToken);

        await _learningProgressHistoryRepository.AddAsync(
            progressHistory,
            cancellationToken);

        // 11. Tek SaveChanges ile hepsini kaydediyoruz.
        // EF Core SaveChanges kendi transaction mantığıyla bu kayıtları birlikte işler.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 12. API response mapping işini feature mapper'a bırakıyoruz.
        // Handler entity oluşturma ve use-case akışını yönetir;
        // response DTO propertylerini tek tek dizmez.
        return UserDictionaryMapper.ToSaveLearningItemResponse(
            userLearningItem: userLearningItem,
            learningItem: learningItem,
            userLearningProgress: userLearningProgress);
    }

    /// <summary>
    /// LearningItem var mı ve aktif mi kontrol eder.
    /// Bulamazsa NotFoundException fırlatır.
    /// </summary>
    private async Task<LearningItem> GetRequiredLearningItemAsync(
        Guid learningItemId,
        CancellationToken cancellationToken)
    {
        var learningItem = await _learningItemRepository.FirstOrDefaultAsync(
            item => item.Id == learningItemId && item.IsActive,
            cancellationToken);

        if (learningItem is null)
        {
            throw new NotFoundException("Learning item", learningItemId);
        }

        return learningItem;
    }

    /// <summary>
    /// SelectedMeaningId gönderildiyse bu meaning'i bulur ve LearningItem'a ait mi kontrol eder.
    /// Gönderilmediyse null döner.
    /// </summary>
    private async Task<Meaning?> GetSelectedMeaningOrNullAsync(
        Guid? selectedMeaningId,
        Guid learningItemId,
        CancellationToken cancellationToken)
    {
        if (selectedMeaningId is null)
        {
            return null;
        }

        var meaning = await _meaningRepository.FirstOrDefaultAsync(
            meaning => meaning.Id == selectedMeaningId.Value,
            cancellationToken);

        if (meaning is null)
        {
            throw new NotFoundException("Meaning", selectedMeaningId.Value);
        }

        if (meaning.LearningItemId != learningItemId)
        {
            throw new BusinessRuleException(
                "Selected meaning does not belong to the specified learning item.",
                "SELECTED_MEANING_DOES_NOT_BELONG_TO_LEARNING_ITEM");
        }

        return meaning;
    }

    /// <summary>
    /// Kullanıcı selectedMeaningId göndermediyse ilgili LearningItem için primary meaning bulmaya çalışır.
    /// Bulamazsa null döner.
    /// </summary>
    private async Task<Meaning?> GetPrimaryMeaningOrNullAsync(
        Guid learningItemId,
        CancellationToken cancellationToken)
    {
        return await _meaningRepository.FirstOrDefaultAsync(
            meaning => meaning.LearningItemId == learningItemId && meaning.IsPrimary,
            cancellationToken);
    }

    /// <summary>
    /// SourceLookupHistoryId gönderildiyse bu kaydın current user'a ait olduğunu doğrular.
    /// 
    /// Neden gerekli?
    /// Kullanıcı request body içine başka bir kullanıcının LookupHistoryId değerini koymamalı.
    /// Bu ownership kontrolüdür.
    /// 
    /// Yeni mimaride ownership kontrolü UserProfileId ile değil,
    /// KeycloakUserId ile yapılır.
    /// </summary>
    private async Task EnsureSourceLookupHistoryBelongsToCurrentUserAsync(
        Guid? sourceLookupHistoryId,
        string keycloakUserId,
        CancellationToken cancellationToken)
    {
        if (sourceLookupHistoryId is null)
        {
            return;
        }

        var lookupHistory = await _lookupHistoryRepository.FirstOrDefaultAsync(
            history => history.Id == sourceLookupHistoryId.Value,
            cancellationToken);

        if (lookupHistory is null)
        {
            throw new NotFoundException("Lookup history", sourceLookupHistoryId.Value);
        }

        if (!string.Equals(
                lookupHistory.KeycloakUserId,
                keycloakUserId,
                StringComparison.Ordinal))
        {
            throw new ForbiddenException(
                "You cannot use another user's lookup history as save source.");
        }
    }
}
