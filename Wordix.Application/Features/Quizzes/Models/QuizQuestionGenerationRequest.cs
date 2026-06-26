namespace Wordix.Application.Features.Quizzes.Models;

/// <summary>
/// Quiz question generator'a gönderilecek request modelidir.
/// 
/// Bu model StartQuizCommandHandler tarafından hazırlanır.
/// Generator bu request içindeki adaylardan soru üretir.
/// </summary>
public sealed class QuizQuestionGenerationRequest
{
    /// <summary>
    /// Üretilmesi istenen soru sayısıdır.
    /// 
    /// Kullanıcı 10 isteyebilir ama aday sayısı azsa generator daha az soru üretebilir
    /// veya handler business rule ile yetersiz dictionary hatası verebilir.
    /// Bu karar sonraki aşamada netleşecek.
    /// </summary>
    public int RequestedQuestionCount { get; init; }

    /// <summary>
    /// Her soru için üretilecek seçenek sayısıdır.
    /// 
    /// İlk prototipte 4 seçenekli test yapacağız.
    /// </summary>
    public int OptionCountPerQuestion { get; init; } = 4;

    /// <summary>
    /// Soru üretiminde kullanılabilecek aday dictionary itemlarıdır.
    /// </summary>
    public IReadOnlyCollection<QuizQuestionCandidate> Candidates { get; init; }
        = Array.Empty<QuizQuestionCandidate>();
}