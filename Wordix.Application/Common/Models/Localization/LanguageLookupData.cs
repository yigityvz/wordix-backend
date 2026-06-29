namespace Wordix.Application.Common.Models.Localization;

/// <summary>
/// Aktif dil bilgisini application katmanında taşımak için kullanılan sade lookup modelidir.
/// 
/// Bu model neden var?
/// - Language bir EF Core entity'sidir.
/// - EF entity'sini doğrudan cachelemek doğru değildir.
/// - Handler'ların Language entity'sinin tüm davranışını bilmesine gerek yoktur.
/// - Lookup akışında bize sadece Id ve Code gibi temel bilgiler gerekir.
/// 
/// Bu yüzden cache ve resolver sonuçlarında Language entity yerine bu sade model kullanılır.
/// </summary>
public sealed record LanguageLookupData
{
    /// <summary>
    /// Language entity'sinin database id değeridir.
    /// 
    /// LookupHistory, LearningItem, Meaning gibi entity'lerde foreign key olarak kullanılır.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Normalize edilmiş dil kodudur.
    /// 
    /// Örnek:
    /// en
    /// tr
    /// de
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Dilin sistemdeki adıdır.
    /// 
    /// Örnek:
    /// English
    /// Turkish
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Dilin kendi dilindeki adıdır.
    /// 
    /// Örnek:
    /// English
    /// Türkçe
    /// </summary>
    public string NativeName { get; init; } = string.Empty;
}