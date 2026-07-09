using MediatR;
using Wordix.Application.Common.Interfaces.Identity;
using Wordix.Application.Common.Interfaces.Persistence;
using Wordix.Application.Features.Quizzes.Dtos.Responses;

namespace Wordix.Application.Features.Quizzes.Commands.StartQuiz;

/// <summary>
/// Kullanıcının quiz başlatma isteğini temsil eden command modelidir.
/// 
/// Neden command?
/// - Quiz başlatıldığında QuizSession oluşturulur.
/// - QuizQuestion kayıtları oluşturulur.
/// - QuizOption kayıtları oluşturulur.
/// 
/// Yani bu işlem sistem durumunu değiştirir.
/// Bu yüzden CQRS açısından Query değil Command olarak modellenir.
/// 
/// Current user bilgisi:
/// - Client KeycloakUserId göndermez.
/// - CurrentUserBehavior pipeline içinde authenticated user's KeycloakUserId değerini çözer.
/// - Handler request.KeycloakUserId üzerinden kullanıcıya özel quiz oluşturur.
/// </summary>
public sealed record StartQuizCommand
    : IRequest<StartQuizResponse>, IRequiresCurrentUser, ITransactionalRequest
{
    /// <summary>
    /// CurrentUserBehavior tarafından doldurulan Keycloak user id değeridir.
    /// 
    /// QuizSession ownership için kullanılır.
    /// Client tarafından gönderilmez.
    /// </summary>
    public string KeycloakUserId { get; set; } = string.Empty;

    /// <summary>
    /// Quiz türüdür.
    /// 
    /// İlk prototipte sadece Test desteklenir.
    /// </summary>
    public string QuizType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz sorularının kaynağıdır.
    /// 
    /// Faz 18 itibarıyla aktif desteklenen kaynak:
    /// UserDictionary
    /// 
    /// Kullanıcının kendi dictionary'sinden soru üretilir.
    /// </summary>
    public string QuizSourceType { get; init; } = string.Empty;

    /// <summary>
    /// Quiz içerik modudur.
    /// 
    /// Faz 18 itibarıyla desteklenen modlar:
    /// - WordsOnly
    /// - PhrasesOnly
    /// - Mixed
    /// 
    /// SentencesOnly enumda vardır ancak Faz 19'da sentence quiz kapsam dışı bırakılmıştır.
    /// Sentence quiz desteği Faz 21 Writing Quiz kapsamında ayrıca tasarlanacaktır.
    /// </summary>
    public string QuizContentMode { get; init; } = string.Empty;

    /// <summary>
    /// Kullanıcının istediği soru sayısıdır.
    /// 
    /// Handler gerçek üretilebilen soru sayısını dictionary durumuna göre belirleyecek.
    /// </summary>
    public int QuestionCount { get; init; }

    /// <summary>
    /// Quiz kaynağı Deck ise kullanılacak deck id değeridir.
    /// 
    /// QuizSourceType = Deck olduğunda zorunludur.
    /// QuizSourceType = UserDictionary olduğunda null olabilir.
    /// </summary>
    public Guid? DeckId { get; init; }

    /// <summary>
    /// Bu quiz için sistem önerileri dahil edilsin mi?
    /// 
    /// Nullable tutulur:
    /// - null ise handler UserPreference.IncludeSystemRecommendations değerine bakar.
    /// - true/false ise request değeri preference değerini override eder.
    /// </summary>
    public bool? IncludeSystemRecommendations { get; init; }
}