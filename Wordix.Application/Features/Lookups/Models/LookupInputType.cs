namespace Wordix.Application.Features.Lookups.Models;

/// <summary>
/// Lookup'a gelen input metninin hangi tür öğrenilebilir içerik olduğunu temsil eder.
/// 
/// Lookup input sınıflandırmasını temsil eder.
/// 
/// Faz 18 itibarıyla Word ve Phrase lookup aktif olarak desteklenir.
/// Sentence sınıflandırılabilir ama gerçek lookup desteği Faz 19'a bırakılmıştır.
/// Fakat mimari Phrase ve Sentence için hazır tutuluyor.
/// </summary>
public enum LookupInputType
{
    /// <summary>
    /// Tek kelimelik input.
    /// 
    /// Örnek:
    /// - achieve
    /// - improve
    /// - perfect
    /// </summary>
    Word = 1,

    /// <summary>
    /// Birden fazla kelimeden oluşan ama tam cümle olmayan ifade.
    /// 
    /// Örnek:
    /// - give up
    /// - look after
    /// - take off
    /// </summary>
    Phrase = 2,

    /// <summary>
    /// Cümle yapısına sahip input.
    /// 
    /// Örnek:
    /// - I want to improve my English.
    /// - She achieved her goal.
    /// </summary>
    Sentence = 3
}