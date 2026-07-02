using Wordix.Domain.Enums;

namespace Wordix.Application.Features.Quizzes.Services;

/// <summary>
/// QuizType değerine göre uygun question generator'ı seçen servis sözleşmesidir.
/// 
/// Neden gerekli?
/// - StartQuizCommandHandler tek bir generator implementation'ına bağlı kalmasın.
/// - Test quiz, Writing quiz, ileride Listening/Flashcard gibi tipler ayrı generatorlarla eklenebilsin.
/// - Yeni quiz tipi eklenirken handler mümkün olduğunca değişmesin.
/// 
/// Bu yapı Open/Closed Principle'a hizmet eder:
/// Sistemi yeni generator ekleyerek genişletiriz,
/// mevcut handler/generator davranışını bozmayız.
/// </summary>
public interface IQuizQuestionGeneratorResolver
{
    /// <summary>
    /// Verilen QuizType için uygun generator implementation'ını döner.
    /// </summary>
    IQuizQuestionGenerator Resolve(QuizType quizType);
}