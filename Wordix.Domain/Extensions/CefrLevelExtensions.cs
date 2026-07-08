using Wordix.Domain.Enums;

namespace Wordix.Domain.Extensions;

/// <summary>
/// CefrLevel enumu için ortak yardımcı extension methodları içerir.
/// 
/// Neden Domain katmanında?
/// - CefrLevel ve DifficultyGroup domain kavramlarıdır.
/// - CEFR seviyesinden DifficultyGroup üretmek uygulamanın temel iş kuralıdır.
/// - Bu kuralı Application veya Infrastructure içine gömersek aynı mapping farklı yerlerde tekrar edebilir.
/// 
/// Bu class sayesinde:
/// - Import işlemleri
/// - Quiz filtreleri
/// - Recommendation kuralları
/// aynı CEFR mapping mantığını kullanabilir.
/// </summary>
public static class CefrLevelExtensions
{
    /// <summary>
    /// CEFR seviyesini Wordix'in kullanıcı dostu DifficultyGroup değerine çevirir.
    /// 
    /// Mapping kararımız:
    /// - A1/A2 => Beginner
    /// - B1/B2 => Intermediate
    /// - C1/C2 => Hard
    /// - Unknown veya tanımsız değerler => Unknown
    /// </summary>
    public static DifficultyGroup ToDifficultyGroup(this CefrLevel cefrLevel)
    {
        return cefrLevel switch
        {
            CefrLevel.A1 or CefrLevel.A2 => DifficultyGroup.Beginner,

            CefrLevel.B1 or CefrLevel.B2 => DifficultyGroup.Intermediate,

            CefrLevel.C1 or CefrLevel.C2 => DifficultyGroup.Hard,

            _ => DifficultyGroup.Unknown
        };
    }

    /// <summary>
    /// CEFR seviyesinin bilinen/geçerli bir seviye olup olmadığını kontrol eder.
    /// 
    /// Örneğin:
    /// - A1 true
    /// - B2 true
    /// - Unknown false
    /// </summary>
    public static bool IsKnown(this CefrLevel cefrLevel)
    {
        return cefrLevel is
            CefrLevel.A1 or
            CefrLevel.A2 or
            CefrLevel.B1 or
            CefrLevel.B2 or
            CefrLevel.C1 or
            CefrLevel.C2;
    }

    /// <summary>
    /// CSV/import gibi dış kaynaklardan gelen CEFR metnini CefrLevel enumuna çevirmeye çalışır.
    /// 
    /// Örnek inputlar:
    /// - "A1"
    /// - "a2"
    /// - " B1 "
    /// - "A1.1"
    /// - "B2.2"
    /// 
    /// Not:
    /// Wordix domain modelinde CEFR seviyelerini sade A1-C2 olarak tutuyoruz.
    /// Bu yüzden "A1.1" gibi alt seviyeler A1'e indirgenir.
    /// </summary>
    public static bool TryParseCefrLevel(string? value, out CefrLevel cefrLevel)
    {
        cefrLevel = CefrLevel.Unknown;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalizedValue = value.Trim().ToUpperInvariant();

        // Bazı kaynaklarda "A1.1", "A2.2", "B1.1" gibi alt seviyeler gelebilir.
        // Biz bu değerleri ana CEFR seviyesine indirgeriz.
        if (normalizedValue.StartsWith("A1", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.A1;
            return true;
        }

        if (normalizedValue.StartsWith("A2", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.A2;
            return true;
        }

        if (normalizedValue.StartsWith("B1", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.B1;
            return true;
        }

        if (normalizedValue.StartsWith("B2", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.B2;
            return true;
        }

        if (normalizedValue.StartsWith("C1", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.C1;
            return true;
        }

        if (normalizedValue.StartsWith("C2", StringComparison.Ordinal))
        {
            cefrLevel = CefrLevel.C2;
            return true;
        }

        return false;
    }
}