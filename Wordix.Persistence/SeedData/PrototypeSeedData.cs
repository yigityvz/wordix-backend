using Microsoft.EntityFrameworkCore;
using Wordix.Domain.Entities;
using Wordix.Domain.Enums;

namespace Wordix.Persistence.SeedData;

/// <summary>
/// İlk prototip için başlangıç verilerini ekler.
/// 
/// Seed data'nın amacı:
/// - Veritabanı ilk oluşturulduğunda temel dillerin hazır gelmesi.
/// - Lookup/quiz testleri için birkaç örnek kelime ve anlamın hazır bulunması.
/// 
/// Not:
/// Buradaki Id değerlerini sabit Guid olarak veriyoruz.
/// Çünkü EF Core migration seed datasında her migration üretiminde değişen Guid istemeyiz.
/// Sabit Guid kullanmak migration'ı stabil ve tekrar üretilebilir hale getirir.
/// </summary>
public static class PrototypeSeedData
{
    private static readonly DateTime SeedCreatedAt =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid EnglishLanguageId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid TurkishLanguageId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid AchieveLearningItemId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");

    public static readonly Guid PerfectLearningItemId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");

    public static readonly Guid ImproveLearningItemId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

    public static readonly Guid AchieveWordId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");

    public static readonly Guid PerfectWordId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

    public static readonly Guid ImproveWordId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3");

    public static readonly Guid AchieveMeaningId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");

    public static readonly Guid PerfectMeaningId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");

    public static readonly Guid ImproveMeaningId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");

    /// <summary>
    /// ModelBuilder üzerine başlangıç verilerini ekler.
    /// 
    /// Bu method WordixDbContext.OnModelCreating içinde çağrılacaktır.
    /// </summary>
    public static void SeedPrototypeData(this ModelBuilder modelBuilder)
    {
        SeedLanguages(modelBuilder);
        SeedLearningItems(modelBuilder);
        SeedWords(modelBuilder);
        SeedMeanings(modelBuilder);
    }

    /// <summary>
    /// Sistemin ilk desteklediği dilleri ekler.
    /// </summary>
    private static void SeedLanguages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>().HasData(
            new
            {
                Id = EnglishLanguageId,
                Code = "en",
                Name = "English",
                NativeName = "English",
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = TurkishLanguageId,
                Code = "tr",
                Name = "Turkish",
                NativeName = "Türkçe",
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            });
    }

    /// <summary>
    /// İlk prototipte test edeceğimiz kelimeler için LearningItem kayıtlarını ekler.
    /// 
    /// Burada ItemType = Word veriyoruz.
    /// Phrase ve Sentence ileride aynı LearningItem çatısı altında eklenecek.
    /// </summary>
    private static void SeedLearningItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningItem>().HasData(
            new
            {
                Id = AchieveLearningItemId,
                ItemType = LearningItemType.Word,
                LanguageId = EnglishLanguageId,
                CefrLevel = CefrLevel.B1,
                DifficultyGroup = DifficultyGroup.Intermediate,
                SourceType = LearningItemSourceType.SystemSeed,
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = PerfectLearningItemId,
                ItemType = LearningItemType.Word,
                LanguageId = EnglishLanguageId,
                CefrLevel = CefrLevel.A2,
                DifficultyGroup = DifficultyGroup.Beginner,
                SourceType = LearningItemSourceType.SystemSeed,
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = ImproveLearningItemId,
                ItemType = LearningItemType.Word,
                LanguageId = EnglishLanguageId,
                CefrLevel = CefrLevel.B1,
                DifficultyGroup = DifficultyGroup.Intermediate,
                SourceType = LearningItemSourceType.SystemSeed,
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            });
    }

    /// <summary>
    /// LearningItem kayıtlarının Word detaylarını ekler.
    /// </summary>
    private static void SeedWords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Word>().HasData(
            new
            {
                Id = AchieveWordId,
                LearningItemId = AchieveLearningItemId,
                Text = "achieve",
                NormalizedText = "achieve",
                PartOfSpeech = "verb",
                Pronunciation = (string?)null,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = PerfectWordId,
                LearningItemId = PerfectLearningItemId,
                Text = "perfect",
                NormalizedText = "perfect",
                PartOfSpeech = "adjective",
                Pronunciation = (string?)null,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = ImproveWordId,
                LearningItemId = ImproveLearningItemId,
                Text = "improve",
                NormalizedText = "improve",
                PartOfSpeech = "verb",
                Pronunciation = (string?)null,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            });
    }

    /// <summary>
    /// Seed edilen kelimelerin Türkçe anlamlarını ekler.
    /// 
    /// Meaning doğrudan Word'e değil LearningItem'a bağlıdır.
    /// Böylece ileride Phrase anlamları da aynı Meaning tablosu üzerinden tutulabilir.
    /// </summary>
    private static void SeedMeanings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meaning>().HasData(
            new
            {
                Id = AchieveMeaningId,
                LearningItemId = AchieveLearningItemId,
                TargetLanguageId = TurkishLanguageId,
                MeaningText = "başarmak",
                ShortDefinition = "Bir hedefe ulaşmak veya istenen sonucu elde etmek.",
                PartOfSpeech = "verb",
                Category = "general",
                IsPrimary = true,
                DisplayOrder = 1,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = PerfectMeaningId,
                LearningItemId = PerfectLearningItemId,
                TargetLanguageId = TurkishLanguageId,
                MeaningText = "mükemmel",
                ShortDefinition = "Eksiksiz, kusursuz veya çok iyi olan.",
                PartOfSpeech = "adjective",
                Category = "general",
                IsPrimary = true,
                DisplayOrder = 1,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            },
            new
            {
                Id = ImproveMeaningId,
                LearningItemId = ImproveLearningItemId,
                TargetLanguageId = TurkishLanguageId,
                MeaningText = "geliştirmek",
                ShortDefinition = "Bir şeyi daha iyi hale getirmek.",
                PartOfSpeech = "verb",
                Category = "general",
                IsPrimary = true,
                DisplayOrder = 1,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = (DateTime?)null
            });
    }
}