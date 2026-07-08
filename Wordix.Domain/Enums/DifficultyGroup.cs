namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcıya daha sade gösterilecek zorluk grubunu temsil eder.
/// 
/// CEFR daha teknik bir seviyedir.
/// DifficultyGroup ise kullanıcı dostu gruptur.
/// 
/// Wordix mapping kararı:
/// - A1-A2 => Beginner
/// - B1-B2 => Intermediate
/// - C1-C2 => Hard
/// </summary>
public enum DifficultyGroup
{
    /// <summary>
    /// Zorluk henüz belirlenmemiş.
    /// 
    /// Provider'dan gelen veya CEFR seviyesi bilinmeyen içeriklerde kullanılabilir.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Başlangıç seviyesi.
    /// 
    /// CEFR karşılığı:
    /// A1-A2
    /// </summary>
    Beginner = 1,

    /// <summary>
    /// Orta/üst orta seviye.
    /// 
    /// CEFR karşılığı:
    /// B1-B2
    /// </summary>
    Intermediate = 2,

    /// <summary>
    /// İleri/zor seviye.
    /// 
    /// CEFR karşılığı:
    /// C1-C2
    /// </summary>
    Hard = 3
}