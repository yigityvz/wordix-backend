using Wordix.Application.Common.Constants;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Lookups.Models;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Lookups.Services;

/// <summary>
/// Provider'dan gelen phrase sonucunu global catalog'a kaydeden application service.
/// 
/// Bu service ne yapar?
/// - Azure gibi provider'lardan gelen phrase çeviri sonucunu alır.
/// - Aynı phrase daha önce oluşturulmuş mu kontrol eder.
/// - Yoksa LearningItem + Phrase + Meaning oluşturur.
/// 
/// Bu service neden Application katmanında?
/// - Bu bir use-case/business akışıdır.
/// - DbContext bilmez.
/// - Repository abstraction'ları ve UnitOfWork üzerinden çalışır.
/// </summary>
public sealed class ProviderPhraseCreationService : IProviderPhraseCreationService
{
    private readonly ILearningItemRepository _learningItemRepository;
    private readonly IRepository<LearningItem> _learningItemGenericRepository;
    private readonly IRepository<Phrase> _phraseRepository;
    private readonly IRepository<Meaning> _meaningRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProviderPhraseCreationService(
        ILearningItemRepository learningItemRepository,
        IRepository<LearningItem> learningItemGenericRepository,
        IRepository<Phrase> phraseRepository,
        IRepository<Meaning> meaningRepository,
        IUnitOfWork unitOfWork)
    {
        _learningItemRepository = learningItemRepository;
        _learningItemGenericRepository = learningItemGenericRepository;
        _phraseRepository = phraseRepository;
        _meaningRepository = meaningRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Provider'dan gelen phrase'i LearningItem + Phrase + Meaning olarak oluşturur.
    /// </summary>
    public async Task<ProviderPhraseCreationResult> CreateAsync(
        ProviderPhraseCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        var displayText = NormalizeDisplayText(request.Text);
        var normalizedText = NormalizeText(request.NormalizedText);

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            normalizedText = NormalizeText(displayText);
        }

        var meaningText = NormalizeDisplayText(request.MeaningText);

        if (request.SourceLanguageId == Guid.Empty)
        {
            return ProviderPhraseCreationResult.Failure(
                normalizedText,
                "SourceLanguageId boş Guid olamaz.");
        }

        if (request.TargetLanguageId == Guid.Empty)
        {
            return ProviderPhraseCreationResult.Failure(
                normalizedText,
                "TargetLanguageId boş Guid olamaz.");
        }

        if (string.IsNullOrWhiteSpace(displayText))
        {
            return ProviderPhraseCreationResult.Failure(
                normalizedText,
                "Phrase metni boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return ProviderPhraseCreationResult.Failure(
                normalizedText,
                "Normalize edilmiş phrase metni boş olamaz.");
        }

        if (string.IsNullOrWhiteSpace(meaningText))
        {
            return ProviderPhraseCreationResult.Failure(
                normalizedText,
                "Meaning metni boş olamaz.");
        }

        var phraseExists = await _learningItemRepository.PhraseExistsAsync(
            normalizedText,
            request.SourceLanguageId,
            cancellationToken);

        if (phraseExists)
        {
            return ProviderPhraseCreationResult.Existing(
                normalizedText,
                $"Phrase zaten mevcut olduğu için provider-created phrase oluşturulmadı. Phrase: {displayText}");
        }

        var externalSourceKey = ResolveExternalSourceKey(
            request.ExternalSourceKey,
            request.SourceProvider,
            normalizedText);

        var learningItem = new LearningItem(
            LearningItemType.Phrase,
            request.SourceLanguageId,
            CefrLevel.Unknown,
            DifficultyGroup.Unknown,
            request.SourceType,
            request.ContentSource,
            request.QualityStatus,
            externalSourceKey,
            DateTime.UtcNow);

        var phrase = new Phrase(
            learningItem.Id,
            displayText,
            normalizedText,
            request.PhraseType);

        var meaning = new Meaning(
            learningItem.Id,
            request.TargetLanguageId,
            meaningText,
            request.ShortDefinition,
            request.PartOfSpeech,
            request.Category,
            isPrimary: true,
            displayOrder: 0,
            request.ContentSource,
            request.QualityStatus,
            request.SourceProvider,
            license: null);

        await _learningItemGenericRepository.AddAsync(
            learningItem,
            cancellationToken);

        await _phraseRepository.AddAsync(
            phrase,
            cancellationToken);

        await _meaningRepository.AddAsync(
            meaning,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProviderPhraseCreationResult.Created(
            learningItem.Id,
            phrase.Id,
            meaning.Id,
            normalizedText,
            $"Provider-created phrase başarıyla oluşturuldu. Phrase: {displayText}");
    }

    /// <summary>
    /// ExternalSourceKey değerini üretir veya güvenli hale getirir.
    /// </summary>
    private static string ResolveExternalSourceKey(
        string? externalSourceKey,
        string sourceProvider,
        string normalizedText)
    {
        var resolvedKey = string.IsNullOrWhiteSpace(externalSourceKey)
            ? $"{sourceProvider}:phrase:{normalizedText}"
            : externalSourceKey.Trim();

        if (resolvedKey.Length <= ImportConstants.MaxExternalSourceKeyLength)
        {
            return resolvedKey;
        }

        return resolvedKey[..ImportConstants.MaxExternalSourceKeyLength];
    }

    /// <summary>
    /// Kullanıcıya gösterilecek metni trimler.
    /// </summary>
    private static string NormalizeDisplayText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    /// <summary>
    /// Arama ve duplicate kontrol için metni normalize eder.
    /// </summary>
    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }
}