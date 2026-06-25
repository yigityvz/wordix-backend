namespace Wordix.Domain.Enums;

/// <summary>
/// Kullanıcıya daha sade gösterilecek zorluk grubunu temsil eder.
/// 
/// CEFR daha teknik bir seviyedir.
/// DifficultyGroup ise kullanıcı dostu gruptur.
/// </summary>
public enum DifficultyGroup
{
    /// <summary>
    /// Zorluk henüz belirlenmemiş.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A1-A2 gibi başlangıç seviyelerine karşılık gelebilir.
    /// </summary>
    Beginner = 1,

    /// <summary>
    /// B1 seviyesine karşılık gelebilir.
    /// </summary>
    Intermediate = 2,

    /// <summary>
    /// B2-C1-C2 gibi daha zor seviyelere karşılık gelebilir.
    /// </summary>
    Hard = 3
}