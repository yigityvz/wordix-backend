using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Domain.Entities;

namespace Wordix.Api.Controllers;

/// <summary>
/// Repository ve UnitOfWork DI kayıtlarını test etmek için kullanılan geçici controller.
/// 
/// Bu controller'ın amacı:
/// - Generic repository kaydı çalışıyor mu kontrol etmek.
/// - Özel repository kaydı çalışıyor mu kontrol etmek.
/// - MSSQL'deki seed data gerçekten okunabiliyor mu görmek.
/// 
/// Not:
/// Bu production endpoint değildir.
/// İleride gerçek lookup/language endpointleri yazıldığında kaldırılabilir.
/// </summary>
[ApiController]
[Route("api/repository-test")]
[Authorize(Policy = "AdminOnly")]
public class RepositoryTestController : ControllerBase
{
    private readonly IRepository<Language> _languageRepository;
    private readonly ILearningItemRepository _learningItemRepository;

    /// <summary>
    /// Controller constructor'ıdır.
    /// 
    /// Burada iki dependency istiyoruz:
    /// 
    /// 1. IRepository<Language>
    ///    Generic repository kaydını test eder.
    /// 
    /// 2. ILearningItemRepository
    ///    Özel repository kaydını test eder.
    /// 
    /// Eğer DI kayıtlarında hata varsa uygulama bu controller'ı oluştururken hata verir.
    /// </summary>
    public RepositoryTestController(
        IRepository<Language> languageRepository,
        ILearningItemRepository learningItemRepository)
    {
        _languageRepository = languageRepository;
        _learningItemRepository = learningItemRepository;
    }

    /// <summary>
    /// Generic repository üzerinden Languages tablosunu okur.
    /// 
    /// Bu endpoint şunu test eder:
    /// - IRepository<Language> DI kaydı doğru mu?
    /// - Repository<Language> çalışıyor mu?
    /// - WordixDbContext SQL Server'a bağlanabiliyor mu?
    /// - Seed edilen language kayıtları okunabiliyor mu?
    /// </summary>
    [HttpGet("languages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLanguages(CancellationToken cancellationToken)
    {
        var languages = await _languageRepository.ListAsync(
            cancellationToken: cancellationToken);

        var response = languages.Select(language => new
        {
            language.Id,
            language.Code,
            language.Name,
            language.NativeName,
            language.IsActive
        });

        return Ok(response);
    }

    /// <summary>
    /// Özel repository üzerinden kelime lookup verisini okur.
    /// 
    /// Varsayılan olarak:
    /// - sourceLanguageCode = en
    /// - targetLanguageCode = tr
    /// 
    /// Örnek:
    /// GET /api/repository-test/word/achieve
    /// 
    /// Bu endpoint şunu test eder:
    /// - Language kayıtları generic repository ile bulunabiliyor mu?
    /// - ILearningItemRepository DI kaydı doğru mu?
    /// - LearningItem + Word + Meaning sorgusu çalışıyor mu?
    /// - Seed edilen achieve/perfect/improve verileri okunabiliyor mu?
    /// </summary>
    [HttpGet("word/{normalizedText}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWordLookupData(
        string normalizedText,
        [FromQuery] string sourceLanguageCode = "en",
        [FromQuery] string targetLanguageCode = "tr",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return BadRequest("Kelime boş olamaz.");
        }

        // Önce kaynak dili buluyoruz.
        // Örneğin sourceLanguageCode = "en"
        var sourceLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == sourceLanguageCode,
            cancellationToken);

        if (sourceLanguage is null)
        {
            return NotFound($"Kaynak dil bulunamadı: {sourceLanguageCode}");
        }

        // Sonra hedef dili buluyoruz.
        // Örneğin targetLanguageCode = "tr"
        var targetLanguage = await _languageRepository.FirstOrDefaultAsync(
            language => language.Code == targetLanguageCode,
            cancellationToken);

        if (targetLanguage is null)
        {
            return NotFound($"Hedef dil bulunamadı: {targetLanguageCode}");
        }

        // Kullanıcının girdiği kelimeyi normalize ediyoruz.
        // Repository tarafında da normalize var ama burada response/test için net tutuyoruz.
        var normalized = normalizedText.Trim().ToLowerInvariant();

        var lookupData = await _learningItemRepository.GetWordLookupDataAsync(
            normalized,
            sourceLanguage.Id,
            targetLanguage.Id,
            cancellationToken);

        if (lookupData is null)
        {
            return NotFound($"Kelime bulunamadı: {normalized}");
        }

        var response = new
        {
            LearningItem = new
            {
                lookupData.LearningItem.Id,
                lookupData.LearningItem.ItemType,
                lookupData.LearningItem.CefrLevel,
                lookupData.LearningItem.DifficultyGroup,
                lookupData.LearningItem.SourceType,
                lookupData.LearningItem.IsActive
            },
            Word = new
            {
                lookupData.Word.Id,
                lookupData.Word.Text,
                lookupData.Word.NormalizedText,
                lookupData.Word.PartOfSpeech,
                lookupData.Word.Pronunciation
            },
            Meanings = lookupData.Meanings.Select(meaning => new
            {
                meaning.Id,
                meaning.MeaningText,
                meaning.ShortDefinition,
                meaning.PartOfSpeech,
                meaning.Category,
                meaning.IsPrimary,
                meaning.DisplayOrder
            })
        };

        return Ok(response);
    }
}